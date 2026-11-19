using System;
using System.Data;

public partial class LichHen : BasePage
{
    public LichHen() : base("appointments") { }
    protected override int[] AllowedRoles { get { return new[] { 4 }; } }
    protected override void LoadData(EzCareService service)
    {
        TitleText = "Lịch hẹn của tôi";
        Day = DayOr(Q("day"),DateTime.Today);
        DateTime? date = Q("day") == "" ? (DateTime?)null : Rules.Date(Q("day"));
        Appointments = service.Bookings.List(UserAccount,"",date,0,0,Q("status"));
    }

}
