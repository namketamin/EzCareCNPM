<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="QuanLyChuyenKhoa.aspx.cs" Inherits="QuanLyChuyenKhoa" %>
<%@ Import Namespace="System.Data" %>
<asp:Content ID="PageContent" ContentPlaceHolderID="MainContent" runat="server">
<% if (Screen == "specialties") { %>
<div class="manage-grid">
<section class="panel table-panel"><div class="section-head"><h2>Danh mục chuyên khoa</h2><span><%= Specialties.Rows.Count %> mục</span></div>
    <% foreach (DataRow row in Specialties.Rows) { %>
    <a class="person" href="<%= E(Link("specialties","id",row["SpecialtyID"])) %>"><strong><%= H(row,"SpecialtyName") %></strong><span><%= H(row,"Description") %></span><span>Sửa</span></a><% } %>
</section>
<section class="panel"><h2><%= EditId == 0 ? "Thêm chuyên khoa" : "Sửa chuyên khoa" %></h2>
<form method="post">
    <input type="hidden" name="csrf" value="<%= E(Csrf) %>"><input type="hidden" name="action" value="specialty"><input type="hidden" name="id" value="<%= EditId %>">
    <label>Tên chuyên khoa<input name="specialtyName" maxlength="100" required value="<%= E(Input("specialtyName",S(Editor,"SpecialtyName"))) %>"></label>
    <label>Mô tả<textarea name="description" maxlength="500"><%= E(Input("description",S(Editor,"Description"))) %></textarea></label>
    <div class="actions"><button class="primary">Lưu chuyên khoa</button><% if (EditId > 0) { %><a href="<%= E(Link("specialties")) %>">Thêm mục mới</a><% } %></div>
</form></section>
</div><% } %>
</asp:Content>
