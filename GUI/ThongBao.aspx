<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ThongBao.aspx.cs" Inherits="ThongBao" %>
<%@ Import Namespace="System.Data" %>
<asp:Content ID="PageContent" ContentPlaceHolderID="MainContent" runat="server">
<% if (Slots.Rows.Count>0) { %>
<section class="panel"><h2>Lịch khám sắp đến</h2>
<% foreach(DataRow row in Slots.Rows) { %>
<article class="notification-item"><div><strong><%= Time(row["StartTime"]) %> · <%= Date(row["WorkDate"]) %></strong>
<p><%= H(row,"DoctorName") %> · <%= H(row,"Room") %>. Giữ mã hẹn để làm thủ tục tại quầy.</p></div>
<a href="<%= E(Link("ticket","id",row["AppointmentID"])) %>">Mở phiếu <%= H(row,"BookingCode") %></a></article>
<% } %></section><% } %>
<section class="panel"><h2>Cập nhật về lịch hẹn</h2>
<% foreach(DataRow row in History.Rows) { %>
<article class="notification-item"><div><strong><%= EventTitle(S(row,"Event")) %></strong>
<small><%= ((DateTime)row["EventTime"]).ToString("HH:mm dd/MM/yyyy") %></small>
<p><%= EventText(S(row,"Event")) %></p></div>
<a href="<%= E(Link("ticket","id",row["AppointmentID"])) %>"><%= H(row,"BookingCode") %></a></article>
<% } if(History.Rows.Count==0) { %><div class="empty-state"><strong>Bạn chưa có thông báo</strong><p>Thông tin đặt lịch và thay đổi lịch sẽ xuất hiện tại đây.</p><a class="button secondary" href="<%= E(Link("book")) %>">Tìm giờ khám</a></div><% } %>
</section>
</asp:Content>
