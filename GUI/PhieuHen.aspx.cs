using System;
using System.Data;

public partial class PhieuHen : BasePage
{
    public PhieuHen() : base("ticket") { }
    protected override int[] AllowedRoles { get { return new[] { Actor.Admin, Actor.Doctor, Actor.Reception, Actor.Patient }; } }
    protected override void LoadData(EzCareService service)
    {
        AppointmentId = Id(Q("id"));
        Appointment = service.Bookings.Detail(UserAccount,AppointmentId);
        History = service.Bookings.History(UserAccount,AppointmentId);
    }

    protected override void HandlePost(EzCareService service)
    {
        var bookings = service.Bookings;
        string action = F("action");
        switch (action)
        {
            case "cancel":
                int appointment = Id(F("appointment"));
                bookings.Change(UserAccount, appointment, 0, F("reason"));
                Done("Đã hủy lịch hẹn.", Link("ticket", "id", appointment));
                break;
            case "contact":
                bookings.Contact(UserAccount, Id(F("appointment")), F("reason"));
                Done("Đã lưu kết quả liên hệ.", Link("ticket", "id", Id(F("appointment"))));
                break;
            case "status":
                bookings.UpdateStatus(UserAccount, Id(F("appointment")), F("status"));
                Done(F("status") == "CheckedIn" ? "Đã ghi nhận bệnh nhân đã đến." :
                    F("status") == "NoShow" ? "Đã ghi nhận bệnh nhân không đến." : "Đã hoàn thành lượt khám.",
                    Link("ticket", "id", Id(F("appointment"))));
                break;
            default: throw new RuleException("Thao tác không hợp lệ.");
        }
    }
}
