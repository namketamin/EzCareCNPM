using System;
using System.Data;

public partial class QuanLyCa : BasePage
{
    public QuanLyCa() : base("schedules") { }
    protected override int[] AllowedRoles { get { return new[] { 1 }; } }
    protected override void LoadData(EzCareService service)
    {
        Day = DayOr(Q("day"),DateTime.Today.AddDays(1));
        Schedules = service.Clinic.Schedules(UserAccount,Day);
        Doctors = service.Clinic.Doctors(0,"");
    }

    protected override void HandlePost(EzCareService service)
    {
        var clinic = service.Clinic;
        string action = F("action");
        switch (action)
        {
            case "schedule":
                DateTime day = Rules.Date(F("day"));
                clinic.CreateSchedule(UserAccount, Id(F("doctor")), day, Rules.Time(F("start")),
                    Rules.Time(F("end")), Id(F("duration")), F("room"));
                Done("Đã mở ca và tạo các khung giờ khám.", Link("schedules", "day", Iso(day)));
                break;
            default: throw new RuleException("Thao tác không hợp lệ.");
        }
    }
}
