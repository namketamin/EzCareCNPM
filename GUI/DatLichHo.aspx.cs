using System;
using System.Data;

public partial class DatLichHo : BasePage
{
    public DatLichHo() : base("book") { }
    protected override int[] AllowedRoles { get { return new[] { Actor.Admin, Actor.Reception }; } }
    protected override void LoadData(EzCareService service)
    {
        TitleText = "Đặt lịch hộ";
        SpecialtyId = Id(Q("specialty"));
        DoctorId = Id(Q("doctor"));
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
        }
        Phone = Q("phone");
        if (Phone != "")
        {
            People = service.Accounts.FindPatients(UserAccount,Phone);
            PatientId = Id(Q("patient"));
            if (!Contains(People,"PatientID",PatientId)) PatientId = 0;
        }
        if (DoctorId > 0) Slots = service.Clinic.SlotOverview(UserAccount,DoctorId,Day,PatientId);
    }

    protected override void HandlePost(EzCareService service)
    {
        var accounts = service.Accounts;
        var bookings = service.Bookings;
        string action = F("action");
        switch (action)
        {
            case "guest":
                int guestId = accounts.AddGuest(UserAccount, F("fullName"), F("guestPhone"),
                    F("birthday"), F("gender"), F("address"));
                Done("Đã tạo hồ sơ khách.", Link("book", "phone", F("guestPhone").Trim(), "patient", guestId,
                    "doctor", Q("doctor"), "day", Q("day"), "specialty", Q("specialty"), "name", Q("name")));
                break;
            case "book":
                int booking = bookings.Book(UserAccount, Id(F("patient")), Id(F("slot")), F("reason"));
                Done("Đã đặt lịch khám.", Link("ticket", "id", booking));
                break;
            default: throw new RuleException("Thao tác không hợp lệ.");
        }
    }
}
