<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DangNhap.aspx.cs" Inherits="DangNhap" %>
<%@ Import Namespace="System.Data" %>
<asp:Content ID="PageContent" ContentPlaceHolderID="MainContent" runat="server">
<% if (Screen == "login") { %>
<div class="auth-layout">
    <form method="post" class="panel auth">
        <h2>Thông tin đăng nhập</h2>
        <input type="hidden" name="csrf" value="<%= E(Csrf) %>"><input type="hidden" name="action" value="login">
        <label>Tên đăng nhập<input name="username" maxlength="50" required autocomplete="username" value="<%= E(Input("username")) %>"></label>
        <label>Mật khẩu<span class="password-field"><input id="login-password" name="password" type="password" maxlength="128" required autocomplete="current-password"><button class="password-toggle" type="button" aria-controls="login-password" aria-pressed="false">Hiện</button></span></label>
        <button class="primary">Đăng nhập</button>
        <p>Chưa có tài khoản? <a href="<%= E(Link("register")) %>">Đăng ký bệnh nhân</a></p>
    </form>
    <aside class="account-note">
        <img src="<%= ResolveUrl("~/Content/clinic.jpg") %>" width="1448" height="1086" alt="Trao đổi tại phòng khám">
        <div><h2>Quản lý lịch hẹn</h2><p>Theo dõi ngày giờ, xem mã hẹn và chủ động đổi lịch khi cần.</p></div>
    </aside>
</div>
<% } %>
</asp:Content>
