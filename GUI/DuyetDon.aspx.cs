using System;
using System.Data;

public partial class DuyetDon : BasePage
{
    public DuyetDon() : base("requests") { }
    protected override int[] AllowedRoles { get { return new[] { 1 }; } }
    protected override void LoadData(EzCareService service)
    {
        TitleText = "Duyệt đơn nghỉ và đổi ca";
        Requests = service.Clinic.Requests(UserAccount);
    }

    protected override void HandlePost(EzCareService service)
    {
        var clinic = service.Clinic;
        string action = F("action");
        switch (action)
        {
            case "review":
                Rules.Require(F("decision") == "approve" || F("decision") == "reject","Kết quả duyệt không hợp lệ.");
                clinic.ReviewShift(UserAccount, Id(F("id")), F("decision") == "approve");
                Done("Đã cập nhật đơn xin nghỉ hoặc đổi ca.", Link("requests"));
                break;
            default: throw new RuleException("Thao tác không hợp lệ.");
        }
    }
}
