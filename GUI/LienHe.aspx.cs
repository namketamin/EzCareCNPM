using System.Configuration;
public partial class LienHe : BasePage
{
    public LienHe() : base("contact") { TitleText="Liên hệ phòng khám"; }
    protected override bool UsesDatabase { get { return false; } }
    protected override void LoadData(EzCareService service) { }
    public string Contact(string key)
    {
        string value=ConfigurationManager.AppSettings[key];
        return string.IsNullOrWhiteSpace(value) ? "Chưa cập nhật" : value;
    }
}
