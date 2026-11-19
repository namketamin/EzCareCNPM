<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="BacSi.aspx.cs" Inherits="BacSi" %>
<%@ Import Namespace="System.Data" %>
<asp:Content ID="PageContent" ContentPlaceHolderID="MainContent" runat="server">
<form method="get" class="filters">
    <label>Chuyên khoa<select name="specialty"><option value="0">Tất cả chuyên khoa</option>
    <% foreach (DataRow row in Specialties.Rows) { %><option value="<%= H(row,"SpecialtyID") %>" <%= Selected(SpecialtyId.ToString(),row["SpecialtyID"]) %>><%= H(row,"SpecialtyName") %></option><% } %>
    </select></label><label>Tên bác sĩ<input name="name" value="<%= E(DoctorName) %>" maxlength="100"></label>
    <button class="primary">Tìm bác sĩ</button>
</form>
<div class="doctor-directory">
<% foreach (DataRow row in Doctors.Rows) { %>
<article class="panel"><div class="selected-doctor"><span class="initial" aria-hidden="true"><%= E(Initials(S(row,"FullName"))) %></span><div><h2><%= H(row,"FullName") %></h2><p><%= H(row,"Degree") %> · <%= H(row,"SpecialtyName") %></p></div></div>
<% if (!row.IsNull("ExperienceYears")) { %><p><%= H(row,"ExperienceYears") %> năm kinh nghiệm</p><% } %>
<p><%= H(row,"Biography") %></p><a class="button primary" href="<%= E(Link("book","doctor",row["DoctorID"])) %>">Xem giờ khám</a></article>
<% } if (Doctors.Rows.Count == 0) { %><p class="empty">Không tìm thấy bác sĩ phù hợp.</p><% } %>
</div>
</asp:Content>