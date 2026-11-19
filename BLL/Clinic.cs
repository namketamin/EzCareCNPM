using System;
using System.Data;

public sealed class Clinic
{
    private readonly Db db;
    private readonly ClinicDAL data;
    internal Clinic(Db db)
    {
        this.db = db;
        data = new ClinicDAL(db);
    }

    public DataTable Specialties()
    {
        return data.ListSpecialties();
    }

    public void SaveSpecialty(Actor actor, int id, string name, string description)
    {
        actor.Allow(Actor.Admin);
        name = Rules.Text(name, "tên chuyên khoa", 100);
        description = Rules.Text(description, "mô tả", 500, false);
        db.Write(() =>
        {
            Rules.Require(data.FindSpecialtyName(name, id) == null, "Tên chuyên khoa đã tồn tại.");
            if (id == 0)
                data.InsertSpecialty(name, description);
            else
                Rules.Require(data.UpdateSpecialty(name, description, id)==1,"Không tìm thấy chuyên khoa.");
        });
    }

    public DataTable Doctors(int specialty, string name)
    {
        name = Rules.Text(name, "tên bác sĩ", 100, false);
        return data.ListDoctors(specialty, name ?? "");
    }

    public DataTable Schedules(Actor actor, DateTime day)
    {
        actor.Allow(Actor.Admin, Actor.Doctor);
        return data.ListSchedules(day, actor.Role, actor.Id);
    }

    public void CreateSchedule(Actor actor, int doctor, DateTime day, TimeSpan start, TimeSpan end, int duration, string room)
    {
        actor.Allow(Actor.Admin);
        room = Rules.Text(room, "phòng khám", 50);
        Rules.Require(start>=TimeSpan.Zero && end<TimeSpan.FromDays(1),"Giờ làm việc phải nằm trong cùng một ngày.");
        Rules.Require(duration == 15 || duration == 30, "Lượt khám phải dài 15 hoặc 30 phút.");
        double minutes = (end - start).TotalMinutes;
        Rules.Require(minutes >= duration && minutes % duration == 0, "Ca khám phải chia hết thành các lượt.");
        Rules.Require(day.Add(start) > DateTime.Now, "Ca khám phải bắt đầu trong tương lai.");
        db.Write(() =>
        {
            Rules.Require(data.FindActiveDoctor(doctor) != null, "Bác sĩ không khả dụng.");
            Rules.Require(data.FindOverlappingSchedule(doctor, day, start, end) == null,
                "Bác sĩ đã có ca trùng giờ.");
            int schedule = (int)data.InsertSchedule(doctor, day, start, end, duration, room);
            TimeSpan step = TimeSpan.FromMinutes(duration);
            for (TimeSpan time = start; time < end; time += step)
                data.InsertSlot(schedule, time, time + step);
        });
    }

    public DataTable Slots(int doctor, DateTime day)
    {
        return data.ListAvailableSlots(doctor, day);
    }

    public DataTable SlotOverview(Actor actor, int doctor, DateTime day, int selectedPatient = 0, int current = 0)
    {
        int patient = 0;
        Rules.Require(current == 0 || actor != null,"Vui lòng đăng nhập để đổi lịch.");
        if (actor != null)
        {
            actor.Allow(Actor.Admin, Actor.Patient, Actor.Reception);
            var bookings = new BookingsDAL(db);
            if (actor.Role == Actor.Patient)
            {
                object id = bookings.GetPatientId(actor.Id);
                Rules.Require(id is int,"Không tìm thấy hồ sơ bệnh nhân.");
                patient = (int)id;
            }
            else if (selectedPatient > 0)
            {
                Rules.Require(bookings.FindPatient(selectedPatient) != null,"Không tìm thấy hồ sơ bệnh nhân.");
                patient = selectedPatient;
            }
            if (current > 0)
            {
                DataRow appointment = Rules.Row(bookings.GetAppointment(current,actor.Role,actor.Id),"Không tìm thấy lịch hẹn của bạn.");
                patient = (int)appointment["PatientID"];
            }
        }
        return data.ListSlotOverview(doctor,day,patient,current);
    }

    public DateTime? NextAvailableDay(int doctor)
    {
        object day = data.NextAvailableDay(doctor);
        return day is DateTime ? (DateTime?)day : null;
    }

    public DateTime? NextScheduleDay(Actor actor)
    {
        actor.Allow(Actor.Doctor);
        object day = data.NextScheduleDay(actor.Id);
        return day is DateTime ? (DateTime?)day : null;
    }

    public DataTable Requests(Actor actor)
    {
        actor.Allow(Actor.Admin, Actor.Doctor);
        return data.ListShiftRequests(actor.Role, actor.Id);
    }

    public void RequestShift(Actor actor, int schedule, string type, string reason)
    {
        actor.Allow(Actor.Doctor);
        Rules.Require(type == "NghiDotXuat" || type == "DoiCa", "Loại đơn không hợp lệ.");
        reason = Rules.Text(reason, "lý do xin nghỉ hoặc đổi ca", 500);
        db.Write(() =>
        {
            Rules.Require(data.FindFutureDoctorSchedule(schedule, actor.Id) != null, "Chỉ gửi đơn cho ca của bạn chưa bắt đầu.");
            Rules.Require(data.FindPendingRequest(schedule) == null, "Ca này đã có đơn đang chờ duyệt.");
            data.InsertShiftRequest(schedule, type, reason);
        });
    }

    public void ReviewShift(Actor actor, int request, bool approve)
    {
        actor.Allow(Actor.Admin);
        db.Write(() =>
        {
            DataRow row = Rules.Row(data.GetPendingRequest(request), "Đơn không còn chờ duyệt.");
            int schedule = (int)row["ScheduleID"];
            if (approve)
            {
                Rules.Require(!new ScheduleDAL(db).HasCheckedIn(schedule),"Ca đã có bệnh nhân được tiếp nhận. Hãy xử lý lượt khám trước khi duyệt nghỉ.");
                Rules.Require((string)row["Status"] == "Open" &&
                    ((DateTime)row["WorkDate"]).Add((TimeSpan)row["StartTime"]) > DateTime.Now,
                    "Ca này đã bắt đầu hoặc đã khóa. Bạn có thể từ chối đơn.");
                data.LockSchedule(schedule);
                data.InsertAffectedLogs(request, actor.Id, schedule);
            }
            data.ReviewShiftRequest(approve ? "Approved" : "Rejected", request);
        });
    }
}

