using System;
using System.Data;
using System.Text;

public partial class HoSoCaNhan : BasePage
{
    public HoSoCaNhan() : base("profile") { }
    protected override int[] AllowedRoles { get { return new[] { Actor.Admin, Actor.Doctor, Actor.Reception, Actor.Patient }; } }
    protected override void LoadData(EzCareService service)
    {
        var accounts = service.Accounts;
        Editor = accounts.Profile(UserAccount);
    }

    protected override void HandlePost(EzCareService service)
    {
        var accounts = service.Accounts;
        string action = F("action");
        switch (action)
        {
            case "profile":
                accounts.UpdateProfile(UserAccount, F("fullName"), F("phone"), F("email"),
                    F("birthday"), F("gender"), F("address"), F("password"), F("oldPassword"));
                Done("Đã lưu hồ sơ.", Link("profile"));
                break;
            default: throw new RuleException("Thao tác không hợp lệ.");
        }
    }
}
