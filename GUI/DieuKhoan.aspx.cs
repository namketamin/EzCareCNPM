using System.Configuration;
public partial class DieuKhoan : BasePage
{
    public DieuKhoan() : base("terms") { TitleText="Điều khoản sử dụng"; }
    protected override bool UsesDatabase { get { return false; } }
    protected override void LoadData(EzCareService service) { }
    public string Contact(string key)
    {
        string value=ConfigurationManager.AppSettings[key];
        return string.IsNullOrWhiteSpace(value) ? "Chưa cập nhật" : value;
    }
}
