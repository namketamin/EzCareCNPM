using System;
using System.Data;

public partial class DonDoiCa : BasePage
{
    public DonDoiCa() : base("requests") { }
    protected override int[] AllowedRoles { get { return new[] { 2 }; } }
    protected override void LoadData(EzCareService service)
    {
        TitleText = "Đơn nghỉ và đổi ca của tôi";
        Requests = service.Clinic.Requests(UserAccount);
    }

}
