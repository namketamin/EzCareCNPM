using System;
using System.Data;
using System.Text;

public partial class DangKy : BasePage
{
    public DangKy() : base("register") { }
    protected override int[] AllowedRoles { get { return new[] { 0, 1, 2, 3, 4 }; } }
    protected override void LoadData(EzCareService service)
    {
        if (UserAccount != null) Go(Link(Home(UserAccount.Role)));
    }

    protected override void HandlePost(EzCareService service)
    {
        var accounts = service.Accounts;
        string action = F("action");
        if (action == "register")
        {
            accounts.Register(F("username"), F("password"), F("fullName"), F("phone"), F("email"),
                F("birthday"), F("gender"), F("address"));
            Done("Đã tạo tài khoản. Bạn có thể đăng nhập.", Link("login"));
            return;
        }
    }
}
