using System;
using System.Data;

public partial class TiepNhan : BasePage
{
    public TiepNhan() : base("appointments") { }
    protected override int[] AllowedRoles { get { return new[] { Actor.Admin, Actor.Reception }; } }
    protected override void LoadData(EzCareService service)
    {
        TitleText = "Tiếp nhận tại quầy";
        Search = Q("search");
        DoctorId = Id(Q("doctor"));
        Doctors = service.Clinic.Doctors(0,"");
        Day = DayOr(Q("day"),DateTime.Today);
        DateTime? date = Q("day") != "" || Search == "" ? (DateTime?)Day : null;
        Appointments = service.Bookings.List(UserAccount,Search,date,DoctorId,0,Q("status"));
    }

    protected override void HandlePost(EzCareService service)
    {
        Rules.Require(F("action") == "status","Thao tác không hợp lệ.");
        service.Bookings.UpdateStatus(UserAccount,Id(F("appointment")),F("status"));
        Done(F("status") == "CheckedIn" ? "Đã ghi nhận bệnh nhân đã đến." : "Đã ghi nhận bệnh nhân không đến.",Request.RawUrl);
    }
}
