using System;
using System.Data;
using System.Text;

public partial class DangNhap : BasePage
{
    public DangNhap() : base("login") { }
    protected override int[] AllowedRoles { get { return new[] { 0, 1, 2, 3, 4 }; } }
    protected override void LoadData(EzCareService service)
    {
        if (UserAccount != null) Go(Link(Home(UserAccount.Role)));
    }

    protected override void HandlePost(EzCareService service)
    {
        var accounts = service.Accounts;
        string action = F("action");
        if (action == "login")
        {
            var user = accounts.Login(F("username"), F("password"));
            UserAccount = user;
            Session["user"] = user.Id;
            Session["actor"] = user;
            Session.Remove("csrf");
            string destination = Link(Home(user.Role));
            if (user.Role == Actor.Patient && Id(Q("doctor")) > 0)
                destination = Link("book", "doctor", Id(Q("doctor")), "day", Q("day"),
                    "specialty", Id(Q("specialty")), "name", Q("name"), "slot", Id(Q("slot")));
            else if (user.Role == Actor.Patient && Q("next") == "appointments")
                destination = Link("appointments");
            Done("Đã đăng nhập.", destination);
            return;
        }
    }
}
