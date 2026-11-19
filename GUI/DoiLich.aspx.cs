using System;
using System.Data;

public partial class DoiLich : BasePage
{
    public DoiLich() : base("book") { }
    protected override int[] AllowedRoles { get { return new[] { Actor.Admin, Actor.Reception, Actor.Patient }; } }
    protected override void LoadData(EzCareService service)
    {
        TitleText = "Đổi lịch khám";
        AppointmentId = Id(Q("change"));
        Rules.Require(AppointmentId > 0,"Vui lòng mở chức năng đổi từ phiếu hẹn.");
        Appointment = service.Bookings.Detail(UserAccount,AppointmentId);
        SpecialtyId = (int)Appointment["SpecialtyID"];
        DoctorId = Id(Q("doctor"));
        if (DoctorId == 0) DoctorId = (int)Appointment["DoctorID"];
        DoctorName = Q("name");
        Day = DayOr(Q("day"),DateTime.Today.AddDays(1));
        Specialties = service.Clinic.Specialties();
        Doctors = service.Clinic.Doctors(SpecialtyId,DoctorName);
        if (!Contains(Doctors,"DoctorID",DoctorId))
            DoctorId = Doctors.Rows.Count == 0 ? 0 : (int)Doctors.Rows[0]["DoctorID"];
        if (DoctorId > 0)
        {
            NextOpenDay = service.Clinic.NextAvailableDay(DoctorId);
            if (Q("day") == "" && NextOpenDay.HasValue) Day = NextOpenDay.Value;
            Slots = service.Clinic.SlotOverview(UserAccount,DoctorId,Day,0,AppointmentId);
        }
    }

    protected override void HandlePost(EzCareService service)
    {
        var bookings = service.Bookings;
        string action = F("action");
        switch (action)
        {
            case "change":
                int appointment = Id(F("appointment"));
                int slot = Id(F("slot"));
                Rules.Require(slot > 0, "Vui lòng chọn giờ khám mới.");
                bookings.Change(UserAccount, appointment, slot, F("reason"));
                Done("Đã đổi lịch khám.", Link("ticket", "id", appointment));
                break;
            default: throw new RuleException("Thao tác không hợp lệ.");
        }
    }
}
