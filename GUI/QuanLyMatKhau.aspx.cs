public partial class QuanLyMatKhau : BasePage
{
    public QuanLyMatKhau() : base("passwords") { TitleText="Mật khẩu tài khoản"; }
    protected override int[] AllowedRoles { get { return new[] {Actor.Admin}; } }
    protected override void LoadData(EzCareService service)
    {
        People=LoadAccounts(service,false);
    }
}
