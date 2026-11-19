using System.Configuration;
public partial class BaoMat : BasePage
{
    public BaoMat() : base("privacy") { TitleText="Thông tin và quyền truy cập"; }
    protected override bool UsesDatabase { get { return false; } }
    protected override void LoadData(EzCareService service) { }
    public string Contact(string key)
    {
        string value=ConfigurationManager.AppSettings[key];
        return string.IsNullOrWhiteSpace(value) ? "Chưa cập nhật" : value;
    }
}
