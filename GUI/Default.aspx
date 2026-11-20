<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Default" %>
<%@ Import Namespace="System.Data" %>
<asp:Content ID="PageContent" ContentPlaceHolderID="MainContent" runat="server">
<div class="home-layout">
    <div class="home-content">
        <section class="hero-copy">
            <h1>Đặt lịch khám<br>trực tuyến</h1>
            <p>Chọn chuyên khoa, bác sĩ và giờ khám. Dùng mã trên phiếu hẹn để làm thủ tục tại quầy.</p>
            <a href="<%= E(Link("appointments")) %>" class="text-link">Đã đặt lịch? Mở phiếu hẹn của tôi</a>
        </section>
        <section class="quick-search" aria-labelledby="quick-title" data-ui>
            <h2 id="quick-title">Tìm bác sĩ và giờ khám</h2>
            <form method="get" action="<%= E(Link("book")) %>">
                <label>Chuyên khoa<select name="specialty"><option value="0">Tất cả chuyên khoa</option>
                <% if (Specialties != null) foreach (DataRow row in Specialties.Rows) { %>
                    <option value="<%= H(row,"SpecialtyID") %>"><%= H(row,"SpecialtyName") %></option>
                <% } %></select></label>
                <label>Tên bác sĩ<input name="name" maxlength="100" placeholder="Tìm theo tên bác sĩ"></label>
                <label>Ngày khám<input type="date" name="day" min="<%= Iso(DateTime.Today) %>" value="<%= Iso(DateTime.Today.AddDays(1)) %>"></label>
                <button class="primary">Tìm giờ khám</button>
            </form>
        </section>
    </div>
    <figure class="home-visual">
        <img src="<%= ResolveUrl("~/Content/clinic.jpg") %>" alt="Bác sĩ trao đổi với bệnh nhân trong phòng khám" width="1448" height="1086" fetchpriority="high">
        <!-- <figcaption>Ảnh minh họa buổi tư vấn tại phòng khám</figcaption> -->
    </figure>
</div>
<div class="home-catalog">
        <section class="home-section" id="chuyen-khoa">
            <div class="home-heading"><h2>Chuyên khoa</h2><a href="<%= E(Link("doctors")) %>">Xem tất cả chuyên khoa</a></div>
            <div class="specialty-list">
                <% if (Specialties != null) for(int i=0;i<Math.Min(6,Specialties.Rows.Count);i++) { DataRow row=Specialties.Rows[i]; %>
                <a href="<%= E(Link("book","specialty",row["SpecialtyID"])) %>"><strong><%= H(row,"SpecialtyName") %></strong><span><%= H(row,"Description") %></span><b aria-hidden="true">↗</b></a>
                <% } %>
                <% if (Specialties == null || Specialties.Rows.Count == 0) { %><p>Chưa có chuyên khoa.</p><a href="<%= E(Link("book")) %>">Tra cứu lịch khám</a><% } %>
            </div>
        </section>
        <% if (Doctors != null && Doctors.Rows.Count > 0) { %>
        <section class="home-section home-doctors">
            <div class="home-heading"><h2>Bác sĩ của phòng khám</h2><a href="<%= E(Link("doctors")) %>">Xem tất cả bác sĩ</a></div>
            <% for(int i=0;i<Math.Min(6,Doctors.Rows.Count);i++) { DataRow row=Doctors.Rows[i]; %>
            <a class="home-doctor" href="<%= E(Link("book","doctor",row["DoctorID"])) %>">
                <span class="initial" aria-hidden="true"><%= E(Initials(S(row,"FullName"))) %></span>
                <span><strong><%= H(row,"FullName") %></strong><small><%= H(row,"Degree") %> · <%= H(row,"SpecialtyName") %></small></span>
                <span class="doctor-action">Xem giờ</span>
            </a>
            <% } %>
        </section>
        <% } else { %><section class="home-section"><h2>Bác sĩ</h2><p class="empty">Chưa có thông tin bác sĩ.</p></section><% } %>
</div>
<div class="home-guide">
        <section class="home-section steps-section" id="cach-dat">
            <h2>Các bước đặt lịch khám</h2>
            <ol class="steps">
                <li><strong>Chọn bác sĩ và khung giờ</strong><span>Lịch hiển thị các giờ còn trống trong ca đang mở.</span></li>
                <li><strong>Xác nhận thông tin khám</strong><span>Đăng nhập và nhập lý do khám để nhận mã hẹn.</span></li>
                <li><strong>Xuất trình mã tại quầy</strong><span>Nhân viên tiếp nhận xác nhận bạn đã đến trước lượt khám.</span></li>
            </ol>
        </section>
        <section class="home-section help-section" id="truoc-khi-kham">
            <h2>Về phiếu hẹn của bạn</h2>
            <details><summary>Xem và in phiếu hẹn ở đâu?</summary><p>Đăng nhập, vào <a href="<%= E(Link("appointments")) %>">Lịch hẹn của tôi</a> và mở phiếu. Phiếu có mã hẹn, ngày giờ, bác sĩ và phòng khám.</p></details>
            <details><summary>Đổi hoặc hủy lịch thế nào?</summary><p>Mở phiếu hẹn để đổi hoặc hủy trước giờ khám ít nhất 24 giờ. Khi đổi, bạn có thể chọn bác sĩ khác cùng chuyên khoa.</p></details>
            <details><summary>Nếu bác sĩ thay đổi ca làm việc?</summary><p>Quầy tiếp nhận sẽ liên hệ để thống nhất giờ thay thế. Bạn có thể xem lịch mới và lịch sử thay đổi trên phiếu hẹn.</p></details>
            <details><summary>Có thể đặt hộ tại quầy không?</summary><p>Nhân viên tiếp nhận tìm hồ sơ bằng số điện thoại hoặc tạo hồ sơ khách để hỗ trợ đặt lịch.</p></details>
        </section>
</div>

</asp:Content>
