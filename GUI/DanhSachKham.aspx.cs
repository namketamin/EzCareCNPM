using System;
using System.Data;

public partial class DanhSachKham : BasePage
{
    public DanhSachKham() : base("appointments") { }
    protected override int[] AllowedRoles { get { return new[] { 2 }; } }
    protected override void LoadData(EzCareService service)
    {
        TitleText = "Danh sách khám";
        Day = DayOr(Q("day"),DateTime.Today);
        Schedules = service.Clinic.Schedules(UserAccount,Day);
        Appointments = service.Bookings.List(UserAccount,"",Day,0,Id(Q("schedule")),Q("status"));
    }

}
