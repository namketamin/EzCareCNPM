using System;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;

public abstract class BasePage : Page
{
    public Actor UserAccount;
    public string Screen, Message, Csrf, Search, DoctorName, Phone, TitleText;
    public bool Failed, Available = true;
    public int DoctorId, SpecialtyId, PatientId, AppointmentId, EditId;
    public int AccountPage, AccountSize, AccountTotal, AccountRole;
    public string AccountState;
    public int AccountPages { get { return Math.Max(1,(AccountTotal-1)/AccountSize+1); } }
    public int AccountFirst { get { return AccountTotal == 0 ? 0 : (AccountPage-1)*AccountSize+1; } }
    public int AccountLast { get { return Math.Min(AccountPage*AccountSize,AccountTotal); } }
    public DateTime Day;
    public DateTime? NextOpenDay;
    public DataTable Doctors, Specialties, Slots, Appointments, People, Schedules, Requests, Staff, History;
    public DataRow Appointment, Editor;
    public int FreeSlotCount { get { return Slots == null ? 0 : Slots.Select("SlotState='Free'").Length; } }

    public static string SlotLabel(string state)
    {
        switch (state)
        {
            case "Free": return "Còn trống";
            case "Booked": return "Đã đặt";
            case "Closed": return "Ca đã khóa";
            case "Past": return "Đã qua giờ";
            default: return "Bạn trùng lịch";
        }
    }
    private string nextUrl;
    private NameValueCollection previousForm;
    protected virtual int[] AllowedRoles { get { return new int[0]; } }
    protected virtual bool UsesDatabase { get { return true; } }

    protected BasePage(string screen)
    {
        Screen = screen;
        TitleText = ScreenTitle(screen);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Response.Cache.SetCacheability(HttpCacheability.NoCache);
        Response.Cache.SetNoStore();
        Response.AppendHeader("X-Content-Type-Options", "nosniff");
        Response.AppendHeader("X-Frame-Options", "DENY");
        Csrf = Session["csrf"] as string ?? Guid.NewGuid().ToString("N");
        Session["csrf"] = Csrf;
        Message = Session["message"] as string;
        Session.Remove("message");
        if (Request.HttpMethod == "GET" && Convert.ToString(Session["errorUrl"]) == Request.RawUrl)
        {
            Failed = true;
            previousForm = Session["errorForm"] as NameValueCollection;
        }
        Session.Remove("errorUrl");
        Session.Remove("errorForm");
        if (!UsesDatabase)
        {
            UserAccount = Session["actor"] as Actor;
            LoadData(null);
            return;
        }
        if ((Screen == "login" || Screen == "register") && Session["user"] == null && Request.HttpMethod == "GET") return;
        try
        {
            using (var service = new EzCareService())
            {
                UserAccount = service.Accounts.Current(Session["user"] is int ? (int)Session["user"] : 0);
                Session["actor"] = UserAccount;
                if (UserAccount == null) Session.Remove("user");
                if (Request.HttpMethod == "GET")
                {
                    string destination = ManagementPage();
                    if (destination != null) { Go(ResolveUrl("~/"+destination)+Request.Url.Query); return; }
                }
                if (AllowedRoles.Length > 0 && UserAccount == null && Array.IndexOf(AllowedRoles, 0) < 0)
                {
                    Go(Link("login", "next", Screen == "appointments" ? "appointments" : ""));
                    return;
                }
                if (AllowedRoles.Length > 0 && Array.IndexOf(AllowedRoles, Role) < 0)
                    throw new RuleException("Bạn không có quyền truy cập trang này.");
                if (Request.HttpMethod == "POST")
                {
                    try
                    {
                        Rules.Require(F("csrf") == Csrf, "Phiên đã hết hạn. Hãy tải lại trang rồi thử lại.");
                        if (UserAccount == null && Screen != "login" && Screen != "register")
                        {
                            Go(Link("login"));
                            return;
                        }
                        if (F("action") == "logout")
                        {
                            Session.Clear();
                            Session.Abandon();
                            Go(Link("home"));
                            return;
                        }
                        HandlePost(service);
                        Go(nextUrl ?? Request.RawUrl);
                        return;
                    }
                    catch (RuleException error)
                    {
                        PostError(error.Message);
                        return;
                    }
                }
                LoadData(service);
            }
        }
        catch (RuleException error)
        {
            if (Request.HttpMethod == "POST") { PostError(error.Message); return; }
            Message = error.Message; Failed = true; Available = false;
        }
        catch (Exception error)
        {
            Trace.Warn("EzCare", "Data", error);
            Message = "Chưa thể xử lý dữ liệu. Vui lòng thử lại hoặc kiểm tra kết nối SQL Server.";
            if (Request.HttpMethod == "POST") { PostError(Message); return; }
            Failed = true;
            Available = Screen == "home" || Screen == "login" || Screen == "register";
        }
    }

    protected abstract void LoadData(EzCareService service);
    protected virtual void HandlePost(EzCareService service) { throw new RuleException("Thao tác không hợp lệ."); }
    protected void Done(string message, string url) { Session["message"] = message; nextUrl = url; }
    private void PostError(string message)
    {
        var form = new NameValueCollection(Request.Form);
        foreach (string key in new[] { "password", "oldPassword", "confirmPassword", "csrf" }) form.Remove(key);
        Session["message"] = message;
        Session["errorUrl"] = Request.RawUrl;
        Session["errorForm"] = form;
        Go(Request.RawUrl);
    }
    protected void Go(string url)
    {
        Available = false;
        Response.Redirect(url, false);
        if (Request.HttpMethod == "POST") Response.StatusCode = 303;
        Context.ApplicationInstance.CompleteRequest();
    }
    protected void RequireUser() { Rules.Require(UserAccount != null, "Vui lòng đăng nhập."); }

    private string ManagementPage()
    {
        string file = Request.AppRelativeCurrentExecutionFilePath.ToLowerInvariant();
        if (CanAssist && file == "~/datlich.aspx") return "DatLichHo.aspx";
        if (Role != Actor.Admin) return null;
        switch (file)
        {
            case "~/lichhen.aspx":
            case "~/danhsachkham.aspx":
            case "~/thongbao.aspx": return "TiepNhan.aspx";
            case "~/calamviec.aspx": return "QuanLyCa.aspx";
            case "~/dondoica.aspx": return "DuyetDon.aspx";
            default: return null;
        }
    }

    public int Role { get { return UserAccount == null ? 0 : UserAccount.Role; } }
    public bool Reception { get { return Role == Actor.Reception; } }
    public bool CanAssist { get { return Role == Actor.Admin || Reception; } }
    public bool CanChange
    {
        get
        {
            return Appointment != null && (Role == Actor.Patient || CanAssist) &&
                S(Appointment, "Status") == "Confirmed" &&
                (Start(Appointment) >= DateTime.Now.AddHours(24) || (CanAssist && Locked));
        }
    }

    public bool Locked { get { return Appointment != null && S(Appointment, "ScheduleStatus") == "Locked"; } }

    public bool CanCheckIn(DataRow row)
    {
        return CanAssist && S(row,"Status") == "Confirmed" && S(row,"ScheduleStatus") == "Open"
            && Start(row).Date == DateTime.Today && DateTime.Now < End(row);
    }

    public bool CanMarkNoShow(DataRow row)
    {
        return CanAssist && S(row,"Status") == "Confirmed" && S(row,"ScheduleStatus") == "Open"
            && DateTime.Now >= End(row);
    }

    public string ReceptionHint(DataRow row)
    {
        if (S(row,"Status") != "Confirmed") return "Phiếu đang ở trạng thái " + Status(S(row,"Status")).ToLower() + ".";
        if (S(row,"ScheduleStatus") != "Open") return "Ca khám chưa mở. Hãy kiểm tra ca hoặc xử lý dời lịch.";
        if (Start(row).Date > DateTime.Today) return "Chỉ ghi nhận đã đến vào ngày khám " + Date(row["WorkDate"]) + ".";
        if (DateTime.Now >= End(row)) return "Đã hết lượt khám. Có thể ghi nhận không đến.";
        return "Kiểm tra mã phiếu và xác nhận bệnh nhân đã có mặt tại quầy.";
    }

    public static DateTime Start(DataRow r) { return ((DateTime)r["WorkDate"]).Add((TimeSpan)r["StartTime"]); }

    public static DateTime End(DataRow r) { return ((DateTime)r["WorkDate"]).Add((TimeSpan)r["EndTime"]); }

    public string Q(string key) { return Request.QueryString[key] ?? ""; }
    public string F(string key) { return (Request.HttpMethod == "POST" ? Request.Form[key] : previousForm == null ? null : previousForm[key]) ?? ""; }
    public string Input(string key, string fallback = "")
    {
        string value = Request.HttpMethod == "POST" ? Request.Form[key] : previousForm == null ? null : previousForm[key];
        return Failed && value != null ? value : fallback;
    }
    public static int Id(string value) { int id; return Int32.TryParse(value, out id) && id > 0 ? id : 0; }
    protected static DateTime DayOr(string value, DateTime fallback) { return value == "" ? fallback : Rules.Date(value); }

    public static string E(object value) { return HttpUtility.HtmlEncode(Convert.ToString(value)); }
    public static string S(DataRow row, string column) { return row == null ? "" : Convert.ToString(row[column]); }
    public static string H(DataRow row, string column) { return E(S(row, column)); }
    public static string Iso(DateTime date) { return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture); }
    public static string Date(object date) { return date is DateTime ? ((DateTime)date).ToString("dd/MM/yyyy") : ""; }
    public static string DateInput(object date) { return date is DateTime ? Iso((DateTime)date) : ""; }
    public static string Time(object time) { return time is TimeSpan ? ((TimeSpan)time).ToString(@"hh\:mm") : ""; }
    public static string Selected(string selected, object value) { return selected == Convert.ToString(value) ? "selected" : ""; }

    public static DataRow Find(DataTable table, string column, int value)
    {
        if (table == null) return null;
        foreach (DataRow row in table.Rows) if ((int)row[column] == value) return row;
        return null;
    }

    protected static bool Contains(DataTable table, string column, int value) { return Find(table, column, value) != null; }

    public string Link(string view, params object[] pairs)
    {
        string file;
        switch (view)
        {
            case "home": file = "Default.aspx"; break;
            case "login": file = "DangNhap.aspx"; break;
            case "register": file = "DangKy.aspx"; break;
            case "profile": file = "HoSoCaNhan.aspx"; break;
            case "notifications": file = "ThongBao.aspx"; break;
            case "doctors": file = "BacSi.aspx"; break;
            case "book": file = CanAssist ? "DatLichHo.aspx" : "DatLich.aspx"; break;
            case "appointments": file = CanAssist ? "TiepNhan.aspx" : Role == Actor.Doctor ? "DanhSachKham.aspx" : "LichHen.aspx"; break;
            case "ticket": file = "PhieuHen.aspx"; break;
            case "affected": file = "LichCanDoi.aspx"; break;
            case "staff": file = "QuanLyNhanSu.aspx"; break;
            case "passwords": file = "QuanLyMatKhau.aspx"; break;
            case "specialties": file = "QuanLyChuyenKhoa.aspx"; break;
            case "schedules": file = Role == Actor.Doctor ? "CaLamViec.aspx" : "QuanLyCa.aspx"; break;
            case "requests": file = Role == Actor.Doctor ? "DonDoiCa.aspx" : "DuyetDon.aspx"; break;
            case "about": file = "GioiThieu.aspx"; break;
            case "contact": file = "LienHe.aspx"; break;
            case "terms": file = "DieuKhoan.aspx"; break;
            case "privacy": file = "BaoMat.aspx"; break;
            default: throw new ArgumentException("Trang không hợp lệ.");
        }
        for (int i = 0; i < pairs.Length; i += 2)
            if (view == "book" && Convert.ToString(pairs[i]) == "change" && Id(Convert.ToString(pairs[i+1])) > 0)
                file = "DoiLich.aspx";
        var url = new StringBuilder(ResolveUrl("~/" + file));
        bool first = true;
        for (int i = 0; i < pairs.Length; i += 2)
        {
            string value = Convert.ToString(pairs[i+1], CultureInfo.InvariantCulture);
            if (value == "" || value == "0") continue;
            url.Append(first ? "?" : "&");
            first = false;
            url.Append(HttpUtility.UrlEncode(Convert.ToString(pairs[i]))).Append("=").Append(HttpUtility.UrlEncode(value));
        }
        return url.ToString();
    }

    public string BookingLink(int doctor)
    {
        return Link("book", "doctor", doctor, "specialty", SpecialtyId, "name", DoctorName,
            "day", Iso(Day), "change", AppointmentId, "phone", Phone, "patient", PatientId);
    }

    protected DataTable LoadAccounts(EzCareService service, bool staffOnly)
    {
        Search = (Request.Unvalidated.QueryString["q"] ?? "").Trim();
        AccountRole = Id(Q("role"));
        if (AccountRole > 4 || (staffOnly && AccountRole != 2 && AccountRole != 3)) AccountRole = 0;
        AccountState = Q("state") == "active" || Q("state") == "locked" ? Q("state") : "";
        AccountSize = Id(Q("size"));
        if (AccountSize != 50 && AccountSize != 100) AccountSize = 20;
        AccountPage = Math.Max(1,Id(Q("page")));
        DataTable rows = service.Accounts.Search(UserAccount,Search,AccountRole,
            AccountState == "" ? (int?)null : AccountState == "active" ? 1 : 0,
            AccountPage,AccountSize,staffOnly,out AccountTotal);
        AccountPage = Math.Min(AccountPage,AccountPages);
        return rows;
    }

    public string AccountLink(int page, int edit = 0)
    {
        return Link(Screen,"id",edit,"q",Search,"role",AccountRole,"state",AccountState,"size",AccountSize,"page",page);
    }

    public string BookingDay(DateTime day)
    {
        return Link("book", "doctor", DoctorId, "specialty", SpecialtyId, "name", DoctorName,
            "day", Iso(day), "change", AppointmentId, "phone", Phone, "patient", PatientId);
    }

    public DataRow NextAppointment
    {
        get
        {
            if (Appointments == null) return null;
            foreach (DataRow row in Appointments.Rows)
                if (S(row, "Status") == "Confirmed" && Start(row) > DateTime.Now)
                    return row;
            return null;
        }
    }

    public string BookingPatientName
    {
        get
        {
            if (Appointment != null) return S(Appointment, "PatientName");
            if (CanAssist) return S(Find(People, "PatientID", PatientId), "FullName");
            return UserAccount == null ? "" : UserAccount.Name;
        }
    }

    public static string Initials(string name)
    {
        string[] words = name.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) return "?";
        return (words.Length == 1 ? words[0].Substring(0, 1) :
            words[words.Length - 2].Substring(0, 1) + words[words.Length - 1].Substring(0, 1)).ToUpperInvariant();
    }

    public string PageDescription()
    {
        switch (Screen)
        {
            case "book": return AppointmentId > 0 ? "Chọn khung giờ thay thế trong cùng chuyên khoa." :
                CanAssist ? "Tìm giờ trống và chọn hồ sơ bệnh nhân để đặt hộ." : "Tìm bác sĩ và chọn khung giờ còn trống.";
            case "appointments": return Role == Actor.Patient ? "Xem phiếu hẹn và theo dõi những thay đổi của lịch khám." :
                CanAssist ? "Tra cứu phiếu hẹn và ghi nhận bệnh nhân đã đến." : "Theo dõi lịch khám và bệnh nhân đã được tiếp nhận.";
            case "profile": return "Cập nhật thông tin được dùng trong lịch hẹn của bạn.";
            case "staff": return "Tạo, cập nhật và khóa tài khoản bác sĩ hoặc nhân viên tiếp nhận.";
            case "schedules": return Role == Actor.Admin ? "Phân ca và mở các khung giờ để bệnh nhân đặt lịch." :
                "Xem ca làm việc và gửi yêu cầu nghỉ hoặc đổi ca.";
            case "requests": return Role == Actor.Admin ? "Duyệt đơn để khóa ca và chuyển lịch bị ảnh hưởng cho tiếp nhận." :
                "Theo dõi kết quả xử lý các đơn nghỉ và đổi ca của bạn.";
            case "affected": return "Liên hệ bệnh nhân, ghi nhận kết quả và thống nhất giờ thay thế.";
            case "specialties": return "Quản lý danh mục hiển thị trong trang tìm bác sĩ.";
            case "login": return "Đăng nhập để xem và quản lý lịch khám của bạn.";
            case "passwords": return "Tìm tài khoản và tra cứu mật khẩu.";
            case "register": return "Tạo hồ sơ bệnh nhân để đặt và theo dõi lịch khám.";
            case "ticket": return "Thông tin tiếp nhận và lịch sử xử lý của lịch khám.";
            default: return "";
        }
    }

    public string Nav(string view, string label)
    {
        bool active = Screen == view || (Screen == "ticket" && view == "appointments");
        return "<a href=\"" + E(Link(view)) + "\"" + (active ? " aria-current=\"location\"" : "") + ">" + E(label) + "</a>";
    }

    public static string Home(int role)
    {
        if (role == Actor.Admin) return "schedules";
        if (role == Actor.Doctor) return "schedules";
        if (role == Actor.Patient) return "book";
        return "appointments";
    }

    public static string RoleName(int role)
    {
        switch (role)
        {
            case Actor.Admin: return "Quản trị";
            case Actor.Doctor: return "Bác sĩ";
            case Actor.Reception: return "Tiếp nhận";
            case Actor.Patient: return "Bệnh nhân";
            default: return "";
        }
    }

    public static string Status(string status)
    {
        switch (status)
        {
            case "Confirmed": return "Đã xác nhận";
            case "CheckedIn": return "Đã đến";
            case "Completed": return "Hoàn thành";
            case "Cancelled": return "Đã hủy";
            case "NoShow": return "Không đến";
            case "Open": return "Đang mở";
            case "Locked": return "Đã khóa";
            case "Pending": return "Chờ duyệt";
            case "Approved": return "Đã duyệt";
            case "Rejected": return "Từ chối";
            case "PendingContact": return "Chờ liên hệ";
            case "Contacted": return "Đã ghi nhận liên hệ";
            case "Rescheduled": return "Đã dời lịch";
            case "NghiDotXuat": return "Nghỉ đột xuất";
            case "DoiCa": return "Đổi ca";
            default: return status;
        }
    }

    public static string ScreenTitle(string screen)
    {
        switch (screen)
        {
            case "book": return "Đặt lịch khám";
            case "login": return "Đăng nhập";
            case "register": return "Đăng ký tài khoản";
            case "profile": return "Hồ sơ cá nhân";
            case "appointments": return "Lịch hẹn";
            case "ticket": return "Phiếu hẹn";
            case "affected": return "Lịch cần dời";
            case "staff": return "Tài khoản nhân sự";
            case "specialties": return "Chuyên khoa";
            case "schedules": return "Ca làm việc";
            case "requests": return "Đơn nghỉ và đổi ca";
            default: return "Đặt lịch khám";
        }
    }

}
