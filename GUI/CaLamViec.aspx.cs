using System;
using System.Data;

public partial class CaLamViec : BasePage
{
    public CaLamViec() : base("schedules") { }
    protected override int[] AllowedRoles { get { return new[] { 2 }; } }
    protected override void LoadData(EzCareService service)
    {
        TitleText = "Ca của tôi";
        Day = DayOr(Q("day"),service.Clinic.NextScheduleDay(UserAccount) ?? DateTime.Today);
        Schedules = service.Clinic.Schedules(UserAccount,Day);
        EditId = Id(Q("id"));
        Editor = Find(Schedules,"ScheduleID",EditId);
    }

    protected override void HandlePost(EzCareService service)
    {
        var clinic = service.Clinic;
        string action = F("action");
        switch (action)
        {
            case "request":
                clinic.RequestShift(UserAccount, Id(F("schedule")), F("type"), F("reason"));
                Done("Đã gửi đơn cho quản trị viên.", Link("requests"));
                break;
            default: throw new RuleException("Thao tác không hợp lệ.");
        }
    }
}
