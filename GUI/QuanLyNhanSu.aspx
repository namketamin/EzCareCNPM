<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="QuanLyNhanSu.aspx.cs" Inherits="QuanLyNhanSu" %>
<%@ Import Namespace="System.Data" %>
<asp:Content ID="PageContent" ContentPlaceHolderID="MainContent" runat="server">
<% if (Screen == "staff") { %>
<section class="panel table-panel"><div class="section-head"><h2>Danh sách nhân sự</h2><a class="button primary" href="<%= E(AccountLink(AccountPage)) %>#staff-form">Thêm tài khoản</a></div>
    <form method="get" class="filters account-filters" aria-label="Tìm tài khoản nhân sự">
        <label class="account-search">Tìm tài khoản<input type="search" name="q" maxlength="100" placeholder="Tên, tài khoản, số điện thoại hoặc email" value="<%= E(Search) %>"></label>
        <label>Vai trò<select name="role"><option value="">Tất cả</option><option value="2" <%= Selected(AccountRole.ToString(),2) %>>Bác sĩ</option><option value="3" <%= Selected(AccountRole.ToString(),3) %>>Tiếp nhận</option></select></label>
        <label>Trạng thái<select name="state"><option value="">Tất cả</option><option value="active" <%= Selected(AccountState,"active") %>>Hoạt động</option><option value="locked" <%= Selected(AccountState,"locked") %>>Đã khóa</option></select></label>
        <label>Số dòng<select name="size"><option value="20">20 dòng</option><option value="50" <%= Selected(AccountSize.ToString(),50) %>>50 dòng</option><option value="100" <%= Selected(AccountSize.ToString(),100) %>>100 dòng</option></select></label>
        <div class="filter-actions"><button class="primary">Tìm kiếm</button><a class="button quiet" href="<%= E(Link("staff")) %>">Xóa bộ lọc</a></div>
    </form>
    <% if (Staff.Rows.Count == 0) { %><div class="empty-state"><h3>Không tìm thấy tài khoản</h3><p>Thử tìm bằng tên, số điện thoại hoặc email khác.</p><a href="<%= E(Link("staff")) %>">Xem tất cả nhân sự</a></div><% } else { %>
    <div class="table-scroll"><table><thead><tr><th scope="col">Họ tên</th><th scope="col">Tên đăng nhập</th><th scope="col">Vai trò</th><th scope="col">Trạng thái</th><th scope="col">Thao tác</th></tr></thead>
    <tbody><% foreach (DataRow row in Staff.Rows) { %><tr><td><%= H(row,"FullName") %><small><%= H(row,"SpecialtyName") %></small><small><%= H(row,"Phone") %></small><small><%= H(row,"Email") %></small></td>
    <td class="code"><%= H(row,"Username") %></td><td><%= RoleName((int)row["RoleID"]) %></td>
    <td><span class="status <%= (bool)row["IsActive"] ? "Open" : "Locked" %>"><%= (bool)row["IsActive"] ? "Hoạt động" : "Đã khóa" %></span></td>
    <td><a href="<%= E(AccountLink(AccountPage,(int)row["UserID"])) %>#staff-form">Chỉnh sửa</a></td></tr><% } %></tbody></table></div><% } %>
    <div class="list-footer"><p class="result-count" role="status">Hiển thị <strong><%= AccountFirst %>–<%= AccountLast %></strong> trong <strong><%= AccountTotal %></strong> tài khoản</p>
    <% if (AccountPages > 1) { %><nav class="account-pagination" aria-label="Trang danh sách nhân sự">
        <% if (AccountPage > 1) { %><a href="<%= E(AccountLink(1)) %>" aria-label="Trang đầu">Đầu</a><a href="<%= E(AccountLink(AccountPage-1)) %>" rel="prev">Trước</a><% } %>
        <% for(int page = Math.Max(1,AccountPage-2); page <= Math.Min(AccountPages,AccountPage+2); page++) { %><a href="<%= E(AccountLink(page)) %>" <%= page == AccountPage ? "aria-current=\"page\"" : "" %> aria-label="Trang <%= page %>"><%= page %></a><% } %>
        <% if (AccountPage < AccountPages) { %><a href="<%= E(AccountLink(AccountPage+1)) %>" rel="next">Sau</a><a href="<%= E(AccountLink(AccountPages)) %>" aria-label="Trang cuối">Cuối</a><% } %>
    </nav><% } %></div>
</section>
<section class="panel staff-editor" id="staff-form"><h2><%= EditId == 0 ? "Tạo tài khoản" : "Sửa tài khoản" %></h2>
<form method="post" class="form-grid">
    <input type="hidden" name="csrf" value="<%= E(Csrf) %>"><input type="hidden" name="action" value="staff"><input type="hidden" name="id" value="<%= EditId %>">
    <% if (EditId > 0) { %><input type="hidden" name="role" value="<%= H(Editor,"RoleID") %>"><div class="filter-info"><span>Vai trò</span><strong><%= RoleName((int)Editor["RoleID"]) %></strong></div>
    <% } else { %><label>Vai trò<select name="role" id="staff-role"><option value="2">Bác sĩ</option><option value="3" <%= Selected(Input("role"),3) %>>Tiếp nhận</option></select></label><% } %>
    <label>Trạng thái<select name="active"><option value="1">Hoạt động</option><option value="0" <%= Selected(Input("active",Editor == null || (bool)Editor["IsActive"] ? "1" : "0"),0) %>>Khóa tài khoản</option></select></label>
    <label>Họ tên<input name="fullName" maxlength="100" required value="<%= E(Input("fullName",S(Editor,"FullName"))) %>"></label>
    <label>Tên đăng nhập<input name="username" maxlength="50" minlength="3" required value="<%= E(Input("username",S(Editor,"Username"))) %>"></label>
    <label>Số điện thoại<input name="phone" type="tel" maxlength="15" value="<%= E(Input("phone",S(Editor,"Phone"))) %>"></label>
    <label>Email<input name="email" type="email" maxlength="100" value="<%= E(Input("email",S(Editor,"Email"))) %>"></label>
    <label><%= EditId == 0 ? "Mật khẩu" : "Mật khẩu mới (tùy chọn)" %><input name="password" type="password" autocomplete="new-password" minlength="8" maxlength="128" <%= EditId == 0 ? "required" : "" %>></label>
    <% if (Editor == null || (int)Editor["RoleID"] == Actor.Doctor) { %>
    <fieldset class="form-grid full doctor-fields" id="doctor-fields"><legend>Thông tin bác sĩ</legend>
    <label>Chuyên khoa<select name="specialty"><option value="">Chọn chuyên khoa</option><% foreach (DataRow row in Specialties.Rows) { %>
    <option value="<%= H(row,"SpecialtyID") %>" <%= Selected(Input("specialty",S(Editor,"SpecialtyID")),row["SpecialtyID"]) %>><%= H(row,"SpecialtyName") %></option><% } %></select></label>
    <label>Học vị<input name="degree" maxlength="100" value="<%= E(Input("degree",S(Editor,"Degree"))) %>"></label>
    <label>Số năm kinh nghiệm<input name="experience" type="number" min="0" max="80" value="<%= E(Input("experience",S(Editor,"ExperienceYears"))) %>"></label>
    <label class="full">Tiểu sử<textarea name="biography" maxlength="2000"><%= E(Input("biography",S(Editor,"Biography"))) %></textarea></label>
    </fieldset><% } %>
    <div class="full actions"><button class="primary">Lưu tài khoản</button><% if(EditId > 0) { %><a class="button quiet" href="<%= E(AccountLink(AccountPage)) %>">Hủy chỉnh sửa</a><% } %></div>
</form></section><% } %>
</asp:Content>
