using System;
using System.Data;
using System.Text;

public partial class LichCanDoi : BasePage
{
    private bool exporting;
    public LichCanDoi() : base("affected") { }
    protected override int[] AllowedRoles { get { return new[] { Actor.Admin, Actor.Reception }; } }
    protected override void LoadData(EzCareService service)
    {
        var bookings = service.Bookings;
        Appointments = bookings.Affected(UserAccount);
        if (Q("export") == "csv") ExportAffected();
    }
    private void ExportAffected()
    {
        var csv = new StringBuilder("\uFEFFMã hẹn,Bệnh nhân,Số điện thoại,Bác sĩ,Ngày khám,Giờ khám,Trạng thái\r\n");
        foreach (DataRow row in Appointments.Rows)
        {
            string[] cells = { S(row,"BookingCode"), S(row,"PatientName"), S(row,"Phone"),
                S(row,"DoctorName"), Date(row["WorkDate"]), Time(row["StartTime"]), Status(S(row,"ActionStatus")) };
            for (int i = 0; i < cells.Length; i++)
            {
                string value = cells[i];
                if (i == 2 || (value.Length > 0 && "=+-@".IndexOf(value[0]) >= 0)) value = "'" + value;
                cells[i] = "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            csv.Append(String.Join(",", cells)).Append("\r\n");
        }
        Response.Clear();
        Response.ContentType = "text/csv";
        Response.ContentEncoding = Encoding.UTF8;
        Response.AddHeader("Content-Disposition", "attachment; filename=ezcare-lich-can-doi.csv");
        Response.Write(csv.ToString());
        exporting = true;
        Context.ApplicationInstance.CompleteRequest();
    }
    protected override void Render(System.Web.UI.HtmlTextWriter writer)
    {
        if (!exporting) base.Render(writer);
    }
}
