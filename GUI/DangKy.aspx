<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DangKy.aspx.cs" Inherits="DangKy" %>
<%@ Import Namespace="System.Data" %>
<asp:Content ID="PageContent" ContentPlaceHolderID="MainContent" runat="server">
<% if (Screen == "register" || Screen == "profile") { %>
<div class="profile-layout">
    <aside class="profile-summary">
        <% if (Screen == "profile") { %>
        <span class="profile-avatar" aria-hidden="true"><%= E(Initials(S(Editor,"FullName"))) %></span>
        <h2><%= H(Editor,"FullName") %></h2><p><%= H(Editor,"Phone") %></p>
        <a href="<%= E(Link("appointments")) %>">Xem lịch hẹn của tôi</a>
        <% } else { %>
        <h2>Một tài khoản cho các lịch khám.</h2><p>Thông tin hồ sơ được dùng để đặt lịch và liên hệ khi ca khám thay đổi.</p>
        <a href="<%= E(Link("login")) %>">Đã có tài khoản? Đăng nhập</a>
        <% } %>
        
    </aside>
<form method="post" class="panel form-grid"><h2 class="full">Thông tin hồ sơ</h2>
    <% if (Screen == "register") { %>
    <input type="hidden" name="csrf" value="<%= E(Csrf) %>"><input type="hidden" name="action" value="register">
    <label>Tên đăng nhập<input name="username" type="text" maxlength="50" required autocomplete="username" minlength="3" value="<%= E(Input("username")) %>"></label>
    <label>Mật khẩu<input name="password" type="password" maxlength="128" required autocomplete="new-password" minlength="8" aria-describedby="register-password-help"><small id="register-password-help">Từ 8 đến 128 ký tự.</small></label>
    <label>Họ và tên<input name="fullName" type="text" maxlength="100" required autocomplete="name" value="<%= E(Input("fullName")) %>"></label>
<label>Số điện thoại<input name="phone" type="tel" maxlength="15" required autocomplete="tel" value="<%= E(Input("phone")) %>"></label>
<label>Email<input name="email" type="email" maxlength="100" autocomplete="email" value="<%= E(Input("email")) %>"></label>
<label>Ngày sinh<input name="birthday" type="date" value="<%= E(Input("birthday")) %>"></label>
<label>Giới tính<select name="gender"><option value="">Chưa chọn</option>
<% foreach (string gender in new[] {"Nam", "Nữ", "Khác"}) { %>
<option <%= Selected(Input("gender"), gender) %>><%= gender %></option><% } %></select></label>
<label>Địa chỉ<input name="address" type="text" maxlength="200" autocomplete="street-address" value="<%= E(Input("address")) %>"></label>
    <% } else { %>
    <input type="hidden" name="csrf" value="<%= E(Csrf) %>"><input type="hidden" name="action" value="profile">
    <label>Họ và tên<input name="fullName" type="text" maxlength="100" required autocomplete="name" value="<%= E(Input("fullName", S(Editor,"FullName"))) %>"></label>
<label>Số điện thoại<input name="phone" type="tel" maxlength="15" required autocomplete="tel" value="<%= E(Input("phone", S(Editor,"Phone"))) %>"></label>
<label>Email<input name="email" type="email" maxlength="100" autocomplete="email" value="<%= E(Input("email", S(Editor,"Email"))) %>"></label>
<label>Ngày sinh<input name="birthday" type="date" value="<%= E(Input("birthday", DateInput(Editor["DateOfBirth"]))) %>"></label>
<label>Giới tính<select name="gender"><option value="">Chưa chọn</option>
<% foreach (string gender in new[] {"Nam", "Nữ", "Khác"}) { %>
<option <%= Selected(Input("gender", S(Editor,"Gender")), gender) %>><%= gender %></option><% } %></select></label>
<label>Địa chỉ<input name="address" type="text" maxlength="200" autocomplete="street-address" value="<%= E(Input("address", S(Editor,"Address"))) %>"></label>
    <h2 class="full password-heading">Đổi mật khẩu</h2><label>Mật khẩu hiện tại<input name="oldPassword" type="password" maxlength="128" autocomplete="current-password"></label>
    <label>Mật khẩu mới (tùy chọn)<input name="password" type="password" maxlength="128" autocomplete="new-password" minlength="8"></label>
    <% } %>
    <div class="actions full"><button class="primary"><%= Screen == "register" ? "Tạo tài khoản" : "Lưu hồ sơ" %></button></div>
</form>
</div>
<% } %>
</asp:Content>
