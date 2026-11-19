using System;
using System.Data;

public sealed class Bookings
{
    private readonly Db db;
    private readonly BookingsDAL data;
    internal Bookings(Db db)
    {
        this.db = db;
        data = new BookingsDAL(db);
    }

    public DataTable List(Actor actor, string search, DateTime? day, int doctor, int schedule = 0, string status = "")
    {
        actor.Allow(Actor.Admin, Actor.Patient, Actor.Reception, Actor.Doctor);
        search = Rules.Text(search, "mã hẹn hoặc số điện thoại", 20, false);
        Rules.Require(Array.IndexOf(new[] {"","Confirmed","CheckedIn","Completed","Cancelled","NoShow"},status)>=0,
            "Trạng thái lọc không hợp lệ.");
        return data.ListAppointments(actor.Role, actor.Id, search, day, doctor, schedule, status);
    }

    public DataRow Detail(Actor actor, int id)
    {
        actor.Allow(Actor.Admin, Actor.Patient, Actor.Reception, Actor.Doctor);
        return Rules.Row(data.GetAppointment(id, actor.Role, actor.Id), "Không tìm thấy lịch hẹn của bạn.");
    }

    public DataTable Affected(Actor actor)
    {
        actor.Allow(Actor.Admin, Actor.Reception);
        return data.ListAffected();
    }

    public DataTable History(Actor actor, int id)
    {
        Detail(actor, id);
        return data.GetHistory(id);
    }

    private int PatientId(Actor actor, int selected)
    {
        if (actor.Role == Actor.Patient)
        {
            object id = data.GetPatientId(actor.Id);
            Rules.Require(id is int, "Không tìm thấy hồ sơ bệnh nhân.");
            return (int)id;
        }
        Rules.Require(data.FindPatient(selected) != null,
            "Vui lòng tìm và chọn hồ sơ bệnh nhân.");
        return selected;
    }

    private DataRow FreeSlot(int slot, int patient, int current)
    {
        var row = Rules.Row(data.GetFreeSlot(slot), "Khung giờ không còn trống. Hãy chọn giờ khác.");
        object doctorConflict = data.FindDoctorConflict(row["DoctorID"], current, row["WorkDate"], row["StartTime"], row["EndTime"]);
        Rules.Require(doctorConflict == null, "Bác sĩ đã có lịch khám trùng giờ. Hãy chọn khung giờ khác.");
        object conflict = data.FindPatientConflict(patient, current, row["WorkDate"], row["StartTime"], row["EndTime"]);
        Rules.Require(conflict == null, "Bệnh nhân đã có lịch khám trùng giờ.");
        return row;
    }

    public int Book(Actor actor, int selected, int slot, string reason)
    {
        actor.Allow(Actor.Admin, Actor.Patient, Actor.Reception);
        reason = Rules.Text(reason, "lý do khám", 500);
        int id = 0;
        db.Write(() =>
        {
            int patient = PatientId(actor, selected);
            FreeSlot(slot, patient, 0);
            string code = "EZ" + Guid.NewGuid().ToString("N").Substring(0, 12).ToUpperInvariant();
            var appointment = new AppointmentDTO {
                BookingCode = code, PatientID = patient, SlotID = slot,
                Reason = reason, CreatedByUserID = actor.Id
            };
            id = (int)data.InsertAppointment(appointment);
            data.SetBooked(slot, true);
        });
        return id;
    }

    private DateTime Starts(DataRow row)
    {
        return ((DateTime)row["WorkDate"]).Add((TimeSpan)row["StartTime"]);
    }

    private int? ApprovedRequest(DataRow row)
    {
        if ((string)row["ScheduleStatus"] != "Locked") return null;
        return data.GetApprovedRequest(row["ScheduleID"]) as int?;
    }

    private void CheckChange(Actor actor, DataRow row, int? request)
    {
        Rules.Require((string)row["Status"] == "Confirmed", "Chỉ đổi hoặc hủy lịch đã xác nhận.");
        bool clinicException = (actor.Role == Actor.Admin || actor.Role == Actor.Reception) && request.HasValue;
        Rules.Require(clinicException || Starts(row) >= DateTime.Now.AddHours(24),
            "Chỉ đổi hoặc hủy lịch trước giờ khám ít nhất 24 giờ. Hãy liên hệ quầy tiếp nhận để được hỗ trợ.");
    }

    private void Log(Actor actor, DataRow row, int? request, int? newSlot, string status, string note)
    {
        data.InsertLog(row["AppointmentID"], request, actor.Id, row["SlotID"], newSlot, status, note);
    }

    public void Change(Actor actor, int id, int slot, string note)
    {
        actor.Allow(Actor.Admin, Actor.Patient, Actor.Reception);
        note = Rules.Text(note, "lý do đổi hoặc hủy", 500);
        db.Write(() =>
        {
            DataRow old = Detail(actor, id);
            int? request = ApprovedRequest(old);
            CheckChange(actor, old, request);
            if (slot > 0)
            {
                DataRow next = FreeSlot(slot, (int)old["PatientID"], id);
                Rules.Require((int)next["SpecialtyID"] == (int)old["SpecialtyID"], "Hãy chọn bác sĩ cùng chuyên khoa.");
                data.MoveAppointment(slot, id);
                data.SetBooked(slot, true);
            }
            else
                data.CancelAppointment(id);
            data.SetBooked((int)old["SlotID"], false);
            Log(actor, old, request, slot > 0 ? (int?)slot : null, slot > 0 ? "Rescheduled" : "Cancelled", note);
        });
    }

    public void Contact(Actor actor, int id, string note)
    {
        actor.Allow(Actor.Admin, Actor.Reception);
        note = Rules.Text(note, "kết quả liên hệ", 500);
        db.Write(() =>
        {
            DataRow row = Detail(actor, id);
            int? request = ApprovedRequest(row);
            Rules.Require(request.HasValue && (string)row["Status"] == "Confirmed", "Lịch này không còn cần liên hệ dời.");
            Log(actor, row, request, null, "Contacted", note);
        });
    }

    public void UpdateStatus(Actor actor, int id, string status)
    {
        actor.Allow(Actor.Admin, Actor.Reception, Actor.Doctor);
        db.Write(() =>
        {
            DataRow row = Detail(actor, id);
            string old = (string)row["Status"];
            DateTime now = DateTime.Now;
            bool open = (string)row["ScheduleStatus"] == "Open";
            DateTime start = Starts(row);
            DateTime end = ((DateTime)row["WorkDate"]).Add((TimeSpan)row["EndTime"]);
            if (status == "CheckedIn")
                Rules.Require((actor.Role == Actor.Admin || actor.Role == Actor.Reception) && old == "Confirmed" && open &&
                    start.Date == now.Date && now < end, "Chỉ tiếp nhận lịch xác nhận trong ngày, trước khi hết lượt.");
            else if (status == "NoShow")
                Rules.Require((actor.Role == Actor.Admin || actor.Role == Actor.Reception) && old == "Confirmed" && open && now >= end,
                    "Chỉ đánh dấu không đến sau giờ kết thúc lượt khám.");
            else
                Rules.Require(status == "Completed" && actor.Role == Actor.Doctor && old == "CheckedIn",
                    "Chỉ hoàn thành lịch đã được tiếp nhận.");
            data.UpdateStatus(status, id);
            Log(actor, row, null, null, status, null);
        });
    }
}

