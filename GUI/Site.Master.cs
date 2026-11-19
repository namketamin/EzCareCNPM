using System.Configuration;
using System.Web.UI;

public partial class SiteLayout : MasterPage
{
    public BasePage View { get { return (BasePage)Page; } }
    public string Contact(string key)
    {
        string value=ConfigurationManager.AppSettings[key];
        return string.IsNullOrWhiteSpace(value) ? "Chưa cập nhật" : value;
    }
}
