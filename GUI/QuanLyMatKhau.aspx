<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="QuanLyMatKhau.aspx.cs" Inherits="QuanLyMatKhau" %>
<%@ Import Namespace="System.Data" %>
<asp:Content ID="PageContent" ContentPlaceHolderID="MainContent" runat="server">
<section class="panel table-panel">
<div class="section-head"><h2>Danh sách tài khoản</h2><span><%= AccountTotal %> tài khoản</span></div>
<form method="get" class="filters account-filters" aria-label="Tìm tài khoản">
    <label class="account-search">Tìm tài khoản<input type="search" name="q" maxlength="100" placeholder="Tên, tài khoản, số điện thoại hoặc email" value="<%= E(Search) %>"></label>
    <label>Vai trò<select name="role"><option value="">Tất cả</option><% for(int role=1;role<=4;role++) { %><option value="<%= role %>" <%= Selected(AccountRole.ToString(),role) %>><%= RoleName(role) %></option><% } %></select></label>
    <label>Trạng thái<select name="state"><option value="">Tất cả</option><option value="active" <%= Selected(AccountState,"active") %>>Hoạt động</option><option value="locked" <%= Selected(AccountState,"locked") %>>Đã khóa</option></select></label>
    <label>Số dòng<select name="size"><option value="20">20 dòng</option><option value="50" <%= Selected(AccountSize.ToString(),50) %>>50 dòng</option><option value="100" <%= Selected(AccountSize.ToString(),100) %>>100 dòng</option></select></label>
    <div class="filter-actions"><button class="primary">Tìm kiếm</button><a class="button quiet" href="<%= E(Link("passwords")) %>">Xóa bộ lọc</a></div>
</form>
<% if(People.Rows.Count == 0) { %><div class="empty-state"><h3>Không tìm thấy tài khoản</h3><p>Thử tìm bằng tên, số điện thoại hoặc email khác.</p><a href="<%= E(Link("passwords")) %>">Xem tất cả tài khoản</a></div><% } else { %>
<div class="table-scroll"><table><thead><tr><th scope="col">Tên đăng nhập</th><th scope="col">Họ tên</th><th scope="col">Vai trò</th><th scope="col">Mật khẩu</th><th scope="col">Trạng thái</th><th scope="col">Đổi mật khẩu gần nhất</th></tr></thead>
<tbody><% foreach(DataRow row in People.Rows) { %><tr><td class="code"><%= H(row,"Username") %></td><td><%= H(row,"FullName") %><small><%= H(row,"Phone") %></small><small><%= H(row,"Email") %></small></td><td><%= RoleName((int)row["RoleID"]) %></td>
<td class="password-value"><% if(row.IsNull("Password")) { %><span class="muted">Chưa có mật khẩu để hiển thị</span><% } else { %><code><%= H(row,"Password") %></code><% } %></td>
<td><span class="status <%= (bool)row["IsActive"] ? "Open" : "Locked" %>"><%= (bool)row["IsActive"] ? "Hoạt động" : "Đã khóa" %></span></td><td><%= row.IsNull("UpdatedAt") ? "—" : ((DateTime)row["UpdatedAt"]).ToString("HH:mm dd/MM/yyyy") %></td></tr><% } %></tbody></table></div><% } %>
<div class="list-footer"><p class="result-count" role="status">Hiển thị <strong><%= AccountFirst %>–<%= AccountLast %></strong> trong <strong><%= AccountTotal %></strong> tài khoản</p>
<% if(AccountPages > 1) { %><nav class="account-pagination" aria-label="Trang danh sách tài khoản">
    <% if(AccountPage > 1) { %><a href="<%= E(AccountLink(1)) %>" aria-label="Trang đầu">Đầu</a><a href="<%= E(AccountLink(AccountPage-1)) %>" rel="prev">Trước</a><% } %>
    <% for(int page=Math.Max(1,AccountPage-2);page<=Math.Min(AccountPages,AccountPage+2);page++) { %><a href="<%= E(AccountLink(page)) %>" <%= page == AccountPage ? "aria-current=\"page\"" : "" %> aria-label="Trang <%= page %>"><%= page %></a><% } %>
    <% if(AccountPage < AccountPages) { %><a href="<%= E(AccountLink(AccountPage+1)) %>" rel="next">Sau</a><a href="<%= E(AccountLink(AccountPages)) %>" aria-label="Trang cuối">Cuối</a><% } %>
</nav><% } %></div>
<a class="text-link" href="<%= E(Link("staff")) %>">Quản lý tài khoản nhân sự</a>
</section>
</asp:Content>
