<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="LichHen.aspx.cs" Inherits="LichHen" %>
<%@ Import Namespace="System.Data" %>
<asp:Content ID="PageContent" ContentPlaceHolderID="MainContent" runat="server">
<% if (Screen == "appointments" || Screen == "affected") { %>
<% if (Role == Actor.Patient && NextAppointment != null) { DataRow next = NextAppointment; %>
<section class="upcoming">
    <div class="upcoming-time"><strong><%= Time(next["StartTime"]) %></strong><span><%= Date(next["WorkDate"]) %></span></div>
    <div><h2>Lịch khám sắp tới</h2><strong><%= H(next,"DoctorName") %></strong><p><%= H(next,"SpecialtyName") %> · <%= H(next,"Room") %></p></div>
    <a class="button primary" href="<%= E(Link("ticket","id",next["AppointmentID"])) %>">Mở phiếu hẹn</a>
</section>
<% } %>
<% if (Screen == "appointments") { %>
<div class="summary-strip" aria-label="Tổng hợp theo danh sách đang xem">
    <div><span>Đã xác nhận</span><strong><%= Appointments.Select("Status='Confirmed'").Length %></strong></div>
    <div><span>Đã đến</span><strong><%= Appointments.Select("Status='CheckedIn'").Length %></strong></div>
    <div><span>Hoàn thành</span><strong><%= Appointments.Select("Status='Completed'").Length %></strong></div>
    <div><span>Tổng lịch đang xem</span><strong><%= Appointments.Rows.Count %></strong></div>
</div>
<% } %>
<% if (Screen == "appointments") { %>
<form method="get" action="<%= E(Link("appointments")) %>" class="filters">
    <% if (CanAssist) { %><label>Mã hẹn hoặc số điện thoại<input name="search" maxlength="20" value="<%= E(Search) %>" placeholder="Nhập mã từ phiếu hẹn"></label><% } %>
    <label>Ngày khám<input type="date" name="day" value="<%= Q("day") != "" || Role == Actor.Doctor || (CanAssist && String.IsNullOrEmpty(Search)) ? Iso(Day) : "" %>"></label>
    <% if (CanAssist) { %><label>Bác sĩ<select name="doctor"><option value="0">Tất cả bác sĩ</option>
    <% foreach (DataRow row in Doctors.Rows) { %><option value="<%= H(row,"DoctorID") %>" <%= Selected(DoctorId.ToString(),row["DoctorID"]) %>><%= H(row,"FullName") %></option><% } %>
    </select></label><% } %>
    <label>Trạng thái<select name="status"><option value="">Tất cả trạng thái</option>
    <% foreach (string state in new[] {"Confirmed","CheckedIn","Completed","Cancelled","NoShow"}) { %><option value="<%= state %>" <%= Selected(Q("status"),state) %>><%= Status(state) %></option><% } %></select></label>
    <% if (Role == Actor.Doctor) { %><label>Ca khám<select name="schedule"><option value="0">Tất cả ca</option><% foreach (DataRow row in Schedules.Rows) { %><option value="<%= H(row,"ScheduleID") %>" <%= Selected(Q("schedule"),row["ScheduleID"]) %>><%= Time(row["StartTime"]) %>–<%= Time(row["EndTime"]) %></option><% } %></select></label><% } %>
    <button class="secondary">Tra cứu</button><button type="button" class="quiet no-print" data-print>In danh sách</button>
    <% if (Q("day") != "" || Q("search") != "" || DoctorId > 0) { %><a class="button quiet" href="<%= E(Link("appointments")) %>">Xóa bộ lọc</a><% } %>
    <% if (Role != Actor.Patient && Q("day") == "" && Q("search") == "") { %><a class="button quiet" href="<%= E(Link("appointments","day",Iso(DateTime.Today.AddDays(1)))) %>">Xem ngày mai</a><% } %>
</form>
<% } else { %><p class="context">Liên hệ bệnh nhân có lịch trong ca đã khóa, ghi nhận kết quả và chọn giờ thay thế.</p><% } %>
<section class="panel table-panel">
    <div class="section-head"><h2><%= Screen == "affected" ? "Bệnh nhân cần liên hệ" : "Danh sách lịch hẹn" %></h2><span><%= Appointments.Rows.Count %> lịch</span>
    <% if (Screen == "affected" && Appointments.Rows.Count > 0) { %><a class="button secondary" href="<%= E(Link("affected","export","csv")) %>">Tải CSV</a><% } %></div>
    <% if (Appointments.Rows.Count == 0) { %>
        <div class="empty-state">
            <% if (Screen == "affected") { %><strong>Không có lịch nào cần dời</strong><p>Lịch bị ảnh hưởng sẽ xuất hiện khi quản trị duyệt đơn nghỉ hoặc đổi ca.</p>
            <% } else if (Role == Actor.Patient) { %><strong>Không có lịch hẹn trong danh sách này</strong><p>Bạn có thể tìm giờ còn trống và đặt lịch mới.</p><a class="button secondary" href="<%= E(Link("book")) %>">Tìm lịch khám</a>
            <% } else if (CanAssist) { %><strong>Chưa tìm thấy lịch hẹn</strong><p>Thử đổi ngày khám, kiểm tra mã hẹn hoặc tạo lịch hộ cho bệnh nhân.</p><a class="button secondary" href="<%= E(Link("book")) %>">Đặt lịch hộ</a>
            <% } else { %><strong>Chưa có bệnh nhân đặt lịch ngày này</strong><p>Chọn ngày khác để xem danh sách khám theo giờ.</p><% } %>
        </div>
    <% } else { %>
    <div class="table-scroll"><table>
        <thead><tr><th scope="col">Giờ khám</th><th scope="col">Mã hẹn</th><th scope="col"><%= Role == Actor.Patient ? "Bác sĩ" : "Bệnh nhân" %></th><th scope="col">Phòng</th><th scope="col">Trạng thái</th><th scope="col">Thao tác</th></tr></thead>
        <tbody><% foreach (DataRow row in Appointments.Rows) { %>
            <tr><td><strong><%= Time(row["StartTime"]) %></strong><small><%= Date(row["WorkDate"]) %></small></td>
            <td><a class="code" href="<%= E(Link("ticket","id",row["AppointmentID"])) %>"><%= H(row,"BookingCode") %></a></td>
            <td><%= Role == Actor.Patient ? H(row,"DoctorName") : H(row,"PatientName") %><% if (CanAssist) { %><small><%= H(row,"Phone") %></small><% } %></td>
            <td><%= H(row,"Room") %></td><td><span class="status <%= H(row,"Status") %>"><%= Status(S(row, Screen == "affected" ? "ActionStatus" : "Status")) %></span></td><td><a class="row-action" href="<%= E(Link("ticket","id",row["AppointmentID"])) %>"><%= Screen == "affected" ? "Liên hệ / dời lịch" : "Xem phiếu" %></a></td></tr>
        <% } %></tbody>
    </table></div><% } %>
</section>
<% if (Screen == "appointments" && Role == Actor.Doctor) { %>
<section class="panel table-panel">
    <div class="section-head"><h2>Đã đến khám</h2></div>
    <% DataRow[] arrived = Appointments.Select("Status='CheckedIn'"); if (arrived.Length == 0) { %><p class="empty">Chưa có bệnh nhân đang chờ khám.</p><% } %>
    <% foreach (DataRow row in arrived) { %><a class="person" href="<%= E(Link("ticket","id",row["AppointmentID"])) %>"><strong><%= Time(row["StartTime"]) %></strong><span><%= H(row,"PatientName") %></span><span>Xem phiếu</span></a><% } %>
</section>
<% } %>
<% } %>

</asp:Content>
