using System;
using System.Data;

public partial class QuanLyNhanSu : BasePage
{
    public QuanLyNhanSu() : base("staff") { }
    protected override int[] AllowedRoles { get { return new[] { 1 }; } }
    protected override void LoadData(EzCareService service)
    {
        Staff = LoadAccounts(service,true);
        Specialties = service.Clinic.Specialties();
        EditId = Id(Q("id"));
        Editor = EditId == 0 ? null : service.Accounts.StaffMember(UserAccount,EditId);
    }

    protected override void HandlePost(EzCareService service)
    {
        var accounts = service.Accounts;
        string action = F("action");
        switch (action)
        {
            case "staff":
                int experience;
                Rules.Require(F("experience") == "" || Int32.TryParse(F("experience"), out experience),
                    "Số năm kinh nghiệm phải là số nguyên.");
                accounts.SaveStaff(UserAccount, Id(F("id")), Id(F("role")), F("username"), F("password"),
                    F("fullName"), F("phone"), F("email"), F("active") == "1", Id(F("specialty")), F("degree"),
                    F("experience") == "" ? (int?)null : Int32.Parse(F("experience")), F("biography"));
                Done("Đã lưu tài khoản nhân sự.", Request.RawUrl);
                break;
            default: throw new RuleException("Thao tác không hợp lệ.");
        }
    }
}
