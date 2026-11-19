<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DoiLich.aspx.cs" Inherits="DoiLich" %>
<%@ Import Namespace="System.Data" %>
<asp:Content ID="PageContent" ContentPlaceHolderID="MainContent" runat="server">
<% if (Screen == "book") { %>
<ol class="booking-progress" aria-label="Quy trình đặt lịch"><li aria-current="step">Chọn giờ khám</li><li>Xác nhận thông tin</li><li>Nhận phiếu hẹn</li></ol>
<% if (Appointment != null) { %>
<p class="context">Lịch hiện tại <strong><%= H(Appointment,"BookingCode") %></strong> · <%= H(Appointment,"DoctorName") %> · <%= Date(Appointment["WorkDate"]) %> lúc <%= Time(Appointment["StartTime"]) %></p>
<% } %>
<form method="get" action="<%= E(Link("book")) %>" class="filters booking-filters">
    <input type="hidden" name="doctor" value="<%= DoctorId %>">
    <input type="hidden" name="change" value="<%= AppointmentId %>">
    <input type="hidden" name="patient" value="<%= PatientId %>">
    <input type="hidden" name="phone" value="<%= E(Phone) %>">
    <% if (Appointment == null) { %>
    <label>Chuyên khoa<select name="specialty"><option value="0">Tất cả chuyên khoa</option>
    <% foreach (DataRow row in Specialties.Rows) { %><option value="<%= H(row,"SpecialtyID") %>" <%= Selected(SpecialtyId.ToString(),row["SpecialtyID"]) %>><%= H(row,"SpecialtyName") %></option><% } %>
    </select></label>
    <% } else { %><div class="filter-info"><span>Chuyên khoa</span><strong><%= H(Appointment,"SpecialtyName") %></strong></div><% } %>
    <label>Tên bác sĩ<input name="name" maxlength="100" value="<%= E(DoctorName) %>" placeholder="Nhập tên bác sĩ"></label>
    <label>Ngày khám<input type="date" name="day" min="<%= Iso(DateTime.Today) %>" value="<%= Iso(Day) %>" required></label>
    <button class="secondary">Tìm lịch</button>
</form>
<div class="booking-grid">
    <section class="panel doctors">
        <div class="section-head"><h2>Chọn bác sĩ</h2><span><%= Doctors.Rows.Count %> kết quả</span></div>
        <% if (Doctors.Rows.Count == 0) { %><p class="empty">Không tìm thấy bác sĩ phù hợp.</p><% } %>
        <% foreach (DataRow row in Doctors.Rows) { int id = (int)row["DoctorID"]; %>
        <a class="doctor-row <%= DoctorId == id ? "chosen" : "" %>" href="<%= E(BookingLink(id)) %>" <%= DoctorId == id ? "aria-current=\"true\"" : "" %>>
            <span class="initial" aria-hidden="true"><%= E(Initials(S(row,"FullName"))) %></span>
            <span><strong><%= H(row,"FullName") %></strong><small><%= H(row,"SpecialtyName") %><%= row.IsNull("Degree") ? "" : " · " + H(row,"Degree") %></small>
            <% if (!row.IsNull("ExperienceYears")) { %><small><%= H(row,"ExperienceYears") %> năm kinh nghiệm</small><% } %></span>
            <span class="doctor-choice"><%= DoctorId == id ? "Đã chọn" : "Chọn" %></span>
        </a>
        <% } %>
    </section>
    <section class="panel availability" id="availability">
        <h2>Chọn khung giờ khám</h2>
        <% DataRow selectedDoctor = Find(Doctors, "DoctorID", DoctorId); %>
        
        <% if (selectedDoctor != null) { %>
        <div class="selected-doctor"><span class="initial" aria-hidden="true"><%= E(Initials(S(selectedDoctor,"FullName"))) %></span><div><strong><%= H(selectedDoctor,"FullName") %></strong><small><%= H(selectedDoctor,"Degree") %> · <%= H(selectedDoctor,"SpecialtyName") %></small></div></div>
        <% if (!selectedDoctor.IsNull("Biography") && S(selectedDoctor,"Biography") != "") { %><details class="doctor-bio"><summary>Thông tin chuyên môn</summary><p><%= H(selectedDoctor,"Biography") %></p></details><% } %>
        <% } %>
        <nav class="day-strip" aria-label="Ngày khám gần ngày đã chọn">
            <% DateTime firstDay = Day > DateTime.Today ? Day.AddDays(-1) : DateTime.Today;
            for (int offset = 0; offset < 5 && firstDay < DateTime.MaxValue.AddDays(-offset); offset++) { DateTime date = firstDay.AddDays(offset); %>
            <a href="<%= E(BookingDay(date)) %>" <%= date == Day ? "aria-current=\"date\"" : "" %>><small><%= date == DateTime.Today ? "Hôm nay" : date.ToString("ddd") %></small><strong><%= date.ToString("dd/MM") %></strong></a><% } %>
        </nav>
        <% if (Slots == null) { %><p class="empty">Chọn bác sĩ để xem giờ khám.</p>
        <% } else if (Slots.Rows.Count == 0) { %><div class="empty-state"><strong>Chưa có giờ khám khả dụng ngày <%= Date(Day) %></strong><p>Bác sĩ chưa được phân ca trong ngày này. Bạn có thể chọn ngày khác hoặc bác sĩ khác.</p>
        <% if (NextOpenDay.HasValue && NextOpenDay.Value != Day) { %><a class="button secondary" href="<%= E(BookingDay(NextOpenDay.Value)) %>">Xem giờ trống ngày <%= Date(NextOpenDay.Value) %></a><% } %></div>
        <% } else { %>
        <div class="slot-toolbar"><strong><%= FreeSlotCount %>/<%= Slots.Rows.Count %> giờ có thể chọn</strong><a href="<%= E(Request.RawUrl) %>">Tải lại giờ khám</a></div>
        <div class="slot-legend" aria-label="Trạng thái khung giờ"><span class="legend-free">Còn trống</span><span class="legend-selected">Đang chọn</span><span class="legend-booked">Đã đặt / không thể chọn</span></div>
        <% if (FreeSlotCount == 0) { %><div class="notice slot-notice" role="status">Không có giờ có thể chọn trong ngày này.
        <% if (NextOpenDay.HasValue && NextOpenDay.Value != Day) { %><a href="<%= E(BookingDay(NextOpenDay.Value)) %>">Xem giờ trống ngày <%= Date(NextOpenDay.Value) %></a><% } %></div><% } %>
        <form class="booking-form" method="<%= Role == 0 ? "get" : "post" %>" action="<%= E(Role == 0 ? Link("login") : Request.RawUrl) %>">
            <% if (Role == 0) { %>
            
            <input type="hidden" name="doctor" value="<%= DoctorId %>">
            <input type="hidden" name="day" value="<%= Iso(Day) %>">
            <input type="hidden" name="specialty" value="<%= SpecialtyId %>">
            <input type="hidden" name="name" value="<%= E(DoctorName) %>">
            <% } %>
            <fieldset class="slot-field"><legend class="visually-hidden">Giờ khám</legend>
            <div class="slot-grid">
                <% foreach (DataRow row in Slots.Rows) { %>
                <% bool free = S(row,"SlotState") == "Free"; %>
                <label class="slot slot-<%= S(row,"SlotState").ToLowerInvariant() %>"><input type="radio" name="slot" value="<%= H(row,"SlotID") %>" <%= free && Input("slot",Q("slot")) == S(row,"SlotID") ? "checked" : "" %> <%= free ? "" : "disabled" %> required><span><strong><%= Time(row["StartTime"]) %>–<%= Time(row["EndTime"]) %></strong><small><%= H(row,"Room") %></small><small class="slot-state"><%= SlotLabel(S(row,"SlotState")) %></small></span></label>
                <% } %>
            </div></fieldset>
            <div class="selection-note" aria-live="polite" data-slot-summary>Chọn một khung giờ để xem thông tin đặt lịch.</div>
            <% if (Role == 0) { %>
            <button class="primary" data-book-submit <%= FreeSlotCount == 0 ? "disabled" : "" %>>Đăng nhập để đặt lịch</button>
            <% } else if (!CanAssist || PatientId > 0 || AppointmentId > 0) { %>
            <input type="hidden" name="action" value="<%= AppointmentId == 0 ? "book" : "change" %>">
            <input type="hidden" name="csrf" value="<%= E(Csrf) %>">
            <input type="hidden" name="patient" value="<%= PatientId %>">
            <input type="hidden" name="appointment" value="<%= AppointmentId %>">
            <h3 class="booking-confirm-title">Xác nhận thông tin</h3>
            <p class="booking-person"><%= E(BookingPatientName) %> · <%= Date(Day) %></p>
            <label><%= AppointmentId == 0 ? "Lý do khám" : "Lý do đổi lịch" %><textarea name="reason" maxlength="500" <%= FreeSlotCount == 0 ? "disabled" : "" %> required><%= E(Input("reason")) %></textarea></label>
            <button class="primary" data-book-submit <%= FreeSlotCount == 0 ? "disabled" : "" %>><%= AppointmentId == 0 ? "Xác nhận đặt lịch" : "Xác nhận đổi lịch" %></button>
            <% } else { %><p class="empty">Chọn hồ sơ bệnh nhân bên dưới để đặt lịch hộ.</p><% } %>
        </form>
        <% } %>
        <p class="booking-policy">Chọn giờ mới trong cùng chuyên khoa. Lịch cũ được giữ đến khi bạn xác nhận đổi thành công.</p>
    </section>
</div>
<% if (CanAssist && AppointmentId == 0) { %>
<section class="panel patient-panel">
    <h2>Hồ sơ bệnh nhân</h2>
    <form method="get" action="<%= E(Link("book")) %>" class="filters compact"><input type="hidden" name="doctor" value="<%= DoctorId %>"><input type="hidden" name="specialty" value="<%= SpecialtyId %>">
        <input type="hidden" name="name" value="<%= E(DoctorName) %>"><input type="hidden" name="day" value="<%= Iso(Day) %>">
        <label>Số điện thoại<input name="phone" type="tel" maxlength="15" value="<%= E(Phone) %>" required></label>
        <button class="secondary">Tìm hồ sơ</button>
    </form>
    <% if (People != null && People.Rows.Count == 0) { %><p class="empty">Chưa có hồ sơ với số điện thoại này.</p><% } %>
    <% if (People != null) foreach (DataRow row in People.Rows) { %>
    <a class="person" href="<%= E(Link("book","doctor",DoctorId,"specialty",SpecialtyId,"name",DoctorName,"day",Iso(Day),"phone",Phone,"patient",row["PatientID"])) %>">
    <strong><%= H(row,"FullName") %></strong> <span><%= Date(row["DateOfBirth"]) %></span>
    <span><%= PatientId == (int)row["PatientID"] ? "Đã chọn" : "Chọn hồ sơ" %></span></a>
    <% } %>
    <details <%= Failed && F("action") == "guest" ? "open" : "" %>><summary>Tạo hồ sơ khách tại quầy</summary>
        <form method="post" class="form-grid">
            <input type="hidden" name="csrf" value="<%= E(Csrf) %>"><input type="hidden" name="action" value="guest">
            <label>Họ và tên<input name="fullName" type="text" maxlength="100" required autocomplete="name" value="<%= E(Input("fullName")) %>"></label>
<label>Số điện thoại<input name="guestPhone" type="tel" maxlength="15" required autocomplete="tel" value="<%= E(Input("guestPhone", Phone)) %>"></label>
<label>Ngày sinh<input name="birthday" type="date" value="<%= E(Input("birthday")) %>"></label>
<label>Giới tính<select name="gender"><option value="">Chưa chọn</option>
<% foreach (string gender in new[] {"Nam", "Nữ", "Khác"}) { %>
<option <%= Selected(Input("gender"), gender) %>><%= gender %></option><% } %></select></label>
<label>Địa chỉ<input name="address" type="text" maxlength="200" autocomplete="street-address" value="<%= E(Input("address")) %>"></label>
            <div class="full"><button class="secondary">Lưu hồ sơ khách</button></div>
        </form>
    </details>
</section>
<% } %>
<% } %>

</asp:Content>

