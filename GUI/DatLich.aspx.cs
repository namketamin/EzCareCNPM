 using System;
using System.Data;

public partial class DatLich : BasePage
{
    public DatLich() : base("book") { }
    protected override int[] AllowedRoles { get { return new[] { 0, 4 }; } }
    protected override void LoadData(EzCareService service)
    {
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
            Slots = service.Clinic.SlotOverview(UserAccount,DoctorId,Day);
        }
    }

    protected override void HandlePost(EzCareService service)
    {
        var bookings = service.Bookings;
        string action = F("action");
        switch (action)
        {
            case "book":
                int booking = bookings.Book(UserAccount, Id(F("patient")), Id(F("slot")), F("reason"));
                Done("Đã đặt lịch khám.", Link("ticket", "id", booking));
                break;
            default: throw new RuleException("Thao tác không hợp lệ.");
        }
    }
}
