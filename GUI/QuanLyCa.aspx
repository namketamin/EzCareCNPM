<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="QuanLyCa.aspx.cs" Inherits="QuanLyCa" %>
<%@ Import Namespace="System.Data" %>
<asp:Content ID="PageContent" ContentPlaceHolderID="MainContent" runat="server">
<% if (Screen == "schedules") { %>
<form method="get" action="<%= E(Link("schedules")) %>" class="filters schedule-filters">
    <label>Ngày làm việc<input type="date" name="day" value="<%= Iso(Day) %>" required></label>
    <button class="secondary">Xem ca</button>
</form>
<section class="panel table-panel">
    <div class="section-head"><h2>Ca ngày <%= Date(Day) %></h2><span><%= Schedules.Rows.Count %> ca</span></div>
    <% if (Schedules.Rows.Count == 0) { %><p class="empty">Chưa có ca làm việc ngày này.</p><% } else { %>
    <div class="table-scroll"><table>
    <thead><tr><th scope="col">Giờ làm việc</th><th scope="col">Bác sĩ</th><th scope="col">Phòng</th><th scope="col">Mỗi lượt</th><th scope="col">Trạng thái</th><th scope="col">Thao tác</th></tr></thead>
    <tbody><% foreach (DataRow row in Schedules.Rows) { %><tr>
        <td><strong><%= Time(row["StartTime"]) %>–<%= Time(row["EndTime"]) %></strong></td>
        <td><%= H(row,"DoctorName") %></td><td><%= H(row,"Room") %></td><td><%= H(row,"SlotDuration") %> phút</td>
        <td><span class="status <%= H(row,"Status") %>"><%= Status(S(row,"Status")) %></span></td>
        <% if (Role == Actor.Admin) { %><td><a href="<%= ResolveUrl("~/SuaCa.aspx") %>?id=<%= H(row,"ScheduleID") %>">Cập nhật</a></td><% } %>
        <% if (Role == Actor.Doctor) { %><td><% if (S(row,"Status") == "Open" && Start(row) > DateTime.Now) { %>
        <a href="<%= E(Link("schedules","day",Iso(Day),"id",row["ScheduleID"])) %>#shift-form">Gửi đơn</a><% } else { %>—<% } %></td><% } %>
    </tr><% } %></tbody>
    </table></div><% } %>
</section>
<% if (Role == Actor.Admin) { %>
<section class="panel schedule-editor"><div class="editor-heading"><h2>Mở ca khám</h2><p>Khung giờ 15 hoặc 30 phút được tạo tự động trong ca.</p></div>
<form method="post" class="form-grid">
    <input type="hidden" name="csrf" value="<%= E(Csrf) %>"><input type="hidden" name="action" value="schedule">
    <label>Bác sĩ<select name="doctor" required><option value="">Chọn bác sĩ</option>
    <% foreach (DataRow row in Doctors.Rows) { %><option value="<%= H(row,"DoctorID") %>" <%= Selected(Input("doctor"),row["DoctorID"]) %>><%= H(row,"FullName") %> · <%= H(row,"SpecialtyName") %></option><% } %></select></label>
    <label>Ngày làm việc<input type="date" name="day" value="<%= E(Input("day",Iso(Day))) %>" min="<%= Iso(DateTime.Today) %>" required></label>
    <label>Giờ bắt đầu<input type="time" name="start" value="<%= E(Input("start","08:00")) %>" required></label>
    <label>Giờ kết thúc<input type="time" name="end" value="<%= E(Input("end","11:00")) %>" required></label>
    <label>Thời lượng lượt khám<select name="duration"><option value="15">15 phút</option><option value="30" <%= Selected(Input("duration","30"),30) %>>30 phút</option></select></label>
    <label>Phòng khám<input name="room" maxlength="50" value="<%= E(Input("room")) %>" required></label>
    <div class="full"><button class="primary">Mở ca và tạo khung giờ</button></div>
</form></section>
<% } else if (Editor != null && S(Editor,"Status") == "Open" && Start(Editor) > DateTime.Now) { %>
<section class="panel" id="shift-form"><h2>Gửi đơn nghỉ hoặc đổi ca</h2>
<p class="context"><%= Date(Editor["WorkDate"]) %> · <%= Time(Editor["StartTime"]) %>–<%= Time(Editor["EndTime"]) %></p>
<form method="post">
    <input type="hidden" name="csrf" value="<%= E(Csrf) %>"><input type="hidden" name="action" value="request"><input type="hidden" name="schedule" value="<%= EditId %>">
    <label>Loại đơn<select name="type"><option value="NghiDotXuat">Nghỉ đột xuất</option><option value="DoiCa" <%= Selected(Input("type"),"DoiCa") %>>Đổi ca</option></select></label>
    <label>Lý do<textarea name="reason" maxlength="500" required><%= E(Input("reason")) %></textarea></label>
    <button class="primary">Gửi đơn</button>
</form></section><% } %>
<% } %>
</asp:Content>
