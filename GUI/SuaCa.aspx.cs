using System;

public partial class SuaCa : BasePage
{
    public SuaCa() : base("schedules") { TitleText="Cập nhật ca làm việc"; }
    protected override int[] AllowedRoles { get { return new[] {Actor.Admin}; } }
    protected override void LoadData(EzCareService service)
    {
        EditId=Id(Q("id"));
        Editor=service.Schedules.Detail(UserAccount,EditId);
    }
    protected override void HandlePost(EzCareService service)
    {
        int id=Id(Q("id"));
        if(F("action")=="editSchedule")
            service.Schedules.Update(UserAccount,id,Rules.Date(F("day")),Rules.Time(F("start")),Rules.Time(F("end")),Id(F("duration")),F("room"));
        else if(F("action")=="scheduleStatus")
            service.Schedules.SetStatus(UserAccount,id,F("status"),F("reason"));
        else throw new RuleException("Thao tác không hợp lệ.");
        Done("Đã cập nhật ca làm việc.",Request.RawUrl);
    }
}
