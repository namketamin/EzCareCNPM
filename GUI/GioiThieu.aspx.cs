using System.Configuration;
public partial class GioiThieu : BasePage
{
    public GioiThieu() : base("about") { TitleText="Giới thiệu EzCare"; }
    protected override bool UsesDatabase { get { return false; } }
    protected override void LoadData(EzCareService service) { }
    public string Contact(string key)
    {
        string value=ConfigurationManager.AppSettings[key];
        return string.IsNullOrWhiteSpace(value) ? "Chưa cập nhật" : value;
    }
}
