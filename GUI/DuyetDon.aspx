<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DuyetDon.aspx.cs" Inherits="DuyetDon" %>
<%@ Import Namespace="System.Data" %>
<asp:Content ID="PageContent" ContentPlaceHolderID="MainContent" runat="server">
<% if (Screen == "requests") { %>
<section class="panel table-panel">
    <div class="section-head"><h2>Danh sách đơn</h2><% if (Role == Actor.Doctor) { %><a href="<%= E(Link("schedules")) %>">Chọn ca để gửi đơn</a><% } %></div>
    <% if (Requests.Rows.Count == 0) { %><p class="empty">Chưa có đơn nghỉ hoặc đổi ca.</p><% } else { %>
    <div class="table-scroll"><table>
    <thead><tr><th scope="col">Ca làm việc</th><th scope="col">Bác sĩ</th><th scope="col">Loại đơn</th><th scope="col">Lý do</th><th scope="col">Trạng thái</th><% if (Role == Actor.Admin) { %><th scope="col">Xử lý</th><% } %></tr></thead>
    <tbody><% foreach (DataRow row in Requests.Rows) { %><tr>
        <td><strong><%= Time(row["StartTime"]) %>–<%= Time(row["EndTime"]) %></strong><small><%= Date(row["WorkDate"]) %></small></td>
        <td><%= H(row,"DoctorName") %></td><td><%= Status(S(row,"RequestType")) %></td><td class="reason-cell"><%= H(row,"Reason") %></td>
        <td><span class="status <%= H(row,"Status") %>"><%= Status(S(row,"Status")) %></span></td>
        <% if (Role == Actor.Admin) { %><td><% if (S(row,"Status") == "Pending") { %>
        <form method="post" class="actions">
            <input type="hidden" name="csrf" value="<%= E(Csrf) %>"><input type="hidden" name="action" value="review"><input type="hidden" name="id" value="<%= H(row,"RequestID") %>">
            <button class="secondary" name="decision" value="approve">Duyệt</button>
            <button class="quiet" name="decision" value="reject">Từ chối</button>
        </form><% } else { %>—<% } %></td><% } %>
    </tr><% } %></tbody></table></div><% } %>
</section><% } %>
</asp:Content>

