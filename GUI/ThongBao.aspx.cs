public partial class ThongBao : BasePage
{
    public ThongBao() : base("notifications") { TitleText="Thông báo và nhắc lịch"; }
    protected override int[] AllowedRoles { get { return new[] {Actor.Patient}; } }
    protected override void LoadData(EzCareService service)
    {
        History=service.Notifications.List(UserAccount);
        Slots=service.Notifications.Reminders(UserAccount);
    }
    public string EventTitle(string action)
    {
        switch(action)
        {
            case "Created": return "Đã tạo phiếu hẹn";
            case "PendingContact": return "Ca khám thay đổi";
            case "Rescheduled": return "Đã đổi lịch khám";
            case "Cancelled": return "Đã hủy lịch hẹn";
            case "CheckedIn": return "Đã tiếp nhận tại quầy";
            case "Completed": return "Đã hoàn thành lượt khám";
            default: return "Đã ghi nhận không đến";
        }
    }
    public string EventText(string action)
    {
        if(action=="PendingContact") return "Ca khám đã khóa. Quầy tiếp nhận sẽ liên hệ để thống nhất giờ thay thế.";
        return "Mở phiếu hẹn để xem thông tin hiện tại và lịch sử xử lý.";
    }
}
