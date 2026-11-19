<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SuaCa.aspx.cs" Inherits="SuaCa" %>
<asp:Content ID="PageContent" ContentPlaceHolderID="MainContent" runat="server">
<a href="<%= E(Link("schedules","day",DateInput(Editor["WorkDate"]))) %>">Về danh sách ca làm việc</a>
<section class="panel mt-3">
<h2><%= H(Editor,"DoctorName") %></h2><p><%= Status(S(Editor,"Status")) %></p>
<form method="post" class="form-grid mt-3">
<input type="hidden" name="csrf" value="<%= E(Csrf) %>"><input type="hidden" name="action" value="editSchedule">
<label>Ngày làm việc<input type="date" name="day" value="<%= E(Input("day",DateInput(Editor["WorkDate"]))) %>" required></label>
<label>Phòng khám<input name="room" maxlength="50" value="<%= E(Input("room",S(Editor,"Room"))) %>" required></label>
<label>Giờ bắt đầu<input type="time" name="start" value="<%= E(Input("start",Time(Editor["StartTime"]))) %>" required></label>
<label>Giờ kết thúc<input type="time" name="end" value="<%= E(Input("end",Time(Editor["EndTime"]))) %>" required></label>
<label>Mỗi lượt khám<select name="duration"><option value="15" <%= Selected(Input("duration",S(Editor,"SlotDuration")),15) %>>15 phút</option><option value="30" <%= Selected(Input("duration",S(Editor,"SlotDuration")),30) %>>30 phút</option></select></label>
<div class="full"><button class="primary">Lưu thông tin ca</button></div>
</form></section>
<section class="panel mt-3"><h2>Đóng hoặc mở lại ca</h2><p>Khóa ca sẽ ngừng nhận lịch mới và đưa bệnh nhân đã đặt vào danh sách cần liên hệ.</p>
<form method="post" class="mt-3">
<input type="hidden" name="csrf" value="<%= E(Csrf) %>"><input type="hidden" name="action" value="scheduleStatus">
<label>Trạng thái<select name="status"><option value="Locked">Khóa ca để dời lịch</option><option value="Open">Mở lại ca</option><option value="Cancelled">Hủy ca</option></select></label>
<label>Lý do<textarea name="reason" maxlength="500" required><%= E(Input("reason")) %></textarea></label>
<button class="secondary">Cập nhật trạng thái</button>
</form></section>
</asp:Content>

