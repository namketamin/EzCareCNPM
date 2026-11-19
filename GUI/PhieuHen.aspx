<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PhieuHen.aspx.cs" Inherits="PhieuHen" %>
<%@ Import Namespace="System.Data" %>
<asp:Content ID="PageContent" ContentPlaceHolderID="MainContent" runat="server">
<% if (Screen == "ticket") { string status = S(Appointment,"Status"); %>
<div class="ticket-layout"><section class="panel ticket">
    <div class="ticket-brand"><strong>EzCare</strong><span>Phiếu hẹn khám bệnh</span></div>
    <div class="ticket-top"><div><span class="muted">Mã hẹn</span><h2 class="code"><%= H(Appointment,"BookingCode") %></h2></div><span class="status <%= H(Appointment,"Status") %>"><%= Status(status) %></span></div>
    <div class="ticket-time"><strong><%= Time(Appointment["StartTime"]) %></strong><div><span><%= Date(Appointment["WorkDate"]) %></span><small><%= H(Appointment,"Room") %> · Đến trước khi kết thúc lượt khám</small></div></div>
    <dl class="ticket-details">
        <div><dt>Bệnh nhân</dt><dd><%= H(Appointment,"PatientName") %></dd></div>
        <div><dt>Bác sĩ</dt><dd><%= H(Appointment,"DoctorName") %></dd></div>
        <div><dt>Chuyên khoa</dt><dd><%= H(Appointment,"SpecialtyName") %></dd></div>
        <div><dt>Phòng khám</dt><dd><%= H(Appointment,"Room") %></dd></div>
        <div><dt>Khung giờ</dt><dd><%= Time(Appointment["StartTime"]) %>–<%= Time(Appointment["EndTime"]) %> · <%= Date(Appointment["WorkDate"]) %></dd></div>
        <div><dt>Số điện thoại</dt><dd><%= H(Appointment,"Phone") %></dd></div>
        <div class="full"><dt>Lý do khám</dt><dd><%= H(Appointment,"Reason") %></dd></div>
    </dl>
    <% if (Locked && status == "Confirmed") { %><p class="notice error">Ca khám này đã khóa. Quầy tiếp nhận sẽ liên hệ để thống nhất giờ thay thế.</p><% } %>
    <div class="actions no-print">
        <% if (CanChange) { %><a class="button primary" href="<%= E(Link("book","change",AppointmentId)) %>">Đổi lịch</a><% } %>
        <button type="button" class="secondary" data-print>In phiếu hẹn</button>
    </div>
</section>
<aside class="ticket-guide no-print"><h2>Khi đến phòng khám</h2><ol><li>Mở phiếu hẹn hoặc mang theo bản in.</li><li>Đưa mã hẹn hoặc số điện thoại cho quầy tiếp nhận.</li><li>Đến đúng phòng khám ghi trên phiếu.</li></ol><p>Thay đổi lịch được lưu trong lịch sử xử lý bên dưới.</p></aside></div>
<% if ((CanAssist && status == "Confirmed" && !Locked) || (Role == Actor.Doctor && status == "CheckedIn")) { %>
<section class="panel no-print"><h2>Cập nhật tiếp nhận</h2>
<form method="post" class="actions">
    <input type="hidden" name="csrf" value="<%= E(Csrf) %>"><input type="hidden" name="action" value="status"><input type="hidden" name="appointment" value="<%= AppointmentId %>">
    <% if (CanAssist) { %>
        <% if (CanCheckIn(Appointment)) { %><button class="primary" name="status" value="CheckedIn">Ghi nhận đã đến</button>
        <% } else { %><button type="button" class="secondary" disabled>Ghi nhận đã đến</button><% } %>
        <% if (CanMarkNoShow(Appointment)) { %><button class="secondary" name="status" value="NoShow">Ghi nhận không đến</button><% } %>
        <p class="muted full"><%= E(ReceptionHint(Appointment)) %></p>
    <% } else { %><button class="primary" name="status" value="Completed">Hoàn thành lượt khám</button><% } %>
</form></section>
<% } %>
<% if (CanAssist && Locked && status == "Confirmed") { %>
<section class="panel no-print"><h2>Kết quả liên hệ</h2><form method="post">
    <input type="hidden" name="csrf" value="<%= E(Csrf) %>"><input type="hidden" name="action" value="contact"><input type="hidden" name="appointment" value="<%= AppointmentId %>">
    <label>Nội dung cuộc gọi<textarea name="reason" maxlength="500" required><%= F("action") == "contact" ? E(Input("reason")) : "" %></textarea></label><button class="secondary">Lưu kết quả</button>
</form></section><% } %>
<% if (CanChange) { %>
<details class="panel no-print" <%= Failed && F("action") == "cancel" ? "open" : "" %>><summary>Hủy lịch hẹn</summary><form method="post">
    <input type="hidden" name="csrf" value="<%= E(Csrf) %>"><input type="hidden" name="action" value="cancel"><input type="hidden" name="appointment" value="<%= AppointmentId %>">
    <p class="context">Hủy sẽ giải phóng khung giờ. Phiếu và lý do hủy vẫn được giữ trong lịch sử.</p>
    <label>Lý do hủy<textarea name="reason" maxlength="500" required><%= F("action") == "cancel" ? E(Input("reason")) : "" %></textarea></label><button type="submit" class="danger">Xác nhận hủy</button>
</form></details><% } %>
<% if (!CanChange && (Role == Actor.Patient || CanAssist)) { %>
<section class="panel no-print"><h2>Hủy lịch hẹn</h2><button type="button" class="secondary" disabled>Xác nhận hủy</button>
<% if (status == "Confirmed") { %><p class="context">Đã qua hạn đổi/hủy lúc <strong><%= Start(Appointment).AddHours(-24).ToString("HH:mm dd/MM/yyyy") %></strong>. Theo quy định, chỉ đổi/hủy trước giờ khám ít nhất 24 giờ. Tiếp nhận xử lý ngoại lệ khi ca bác sĩ bị khóa và có yêu cầu đã duyệt.</p>
<% } else { %><p class="context">Phiếu đang ở trạng thái <strong><%= Status(status) %></strong>. Chỉ hủy phiếu đang được xác nhận.</p><% } %></section>
<% } %>
<% if (History.Rows.Count > 0) { %>
<section class="panel no-print"><h2>Lịch sử xử lý</h2><ul class="history">
<% foreach (DataRow row in History.Rows) { if (Role == Actor.Patient && !row.IsNull("RequestID") && (S(row,"ActionStatus") == "PendingContact" || S(row,"ActionStatus") == "Contacted")) continue; %>
    <li><strong><%= Status(S(row,"ActionStatus")) %></strong><small><%= ((DateTime)row["UpdatedAt"]).ToString("dd/MM/yyyy HH:mm") %><% if (Role != Actor.Patient) { %> · <%= H(row,"HandledBy") %><% } %></small>
    <% if (!row.IsNull("NewDate")) { %><span><%= Time(row["OldTime"]) %> <%= Date(row["OldDate"]) %> → <%= Time(row["NewTime"]) %> <%= Date(row["NewDate"]) %></span><% } %>
    <% if (!row.IsNull("Notes")) { %><span><%= H(row,"Notes") %></span><% } %></li>
<% } %></ul></section><% } %>
<% } %>

</asp:Content>
