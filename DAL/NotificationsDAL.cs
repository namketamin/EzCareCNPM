using System.Data;

public sealed class NotificationsDAL
{
    private readonly Db db;
    public NotificationsDAL(Db db) { this.db = db; }

    public DataTable List(int user)
    {
        return db.Query(@"SELECT a.AppointmentID,a.BookingCode,N'Created' AS Event,a.CreatedAt AS EventTime
            FROM Appointments a JOIN Patients p ON p.PatientID=a.PatientID WHERE p.UserID=@user
            UNION ALL
            SELECT a.AppointmentID,a.BookingCode,l.ActionStatus,l.UpdatedAt
            FROM RescheduleLogs l JOIN Appointments a ON a.AppointmentID=l.AppointmentID
            JOIN Patients p ON p.PatientID=a.PatientID
            WHERE p.UserID=@user AND l.ActionStatus IN ('Rescheduled','Cancelled','PendingContact','CheckedIn','Completed','NoShow')
            ORDER BY EventTime DESC",Db.P("@user",user));
    }

    public DataTable Reminders(int user, int hours)
    {
        return db.Query(@"SELECT a.AppointmentID,a.BookingCode,a.WorkDate,a.StartTime,a.DoctorName,a.Room
            FROM AppointmentDetails a
            JOIN SlotDetails s ON s.SlotID=a.SlotID
            WHERE a.PatientUserID=@user AND a.Status='Confirmed' AND a.ScheduleStatus='Open'
                AND s.StartsAt>SYSDATETIME() AND s.StartsAt<=DATEADD(hour,@hours,SYSDATETIME())
            ORDER BY a.WorkDate,a.StartTime",Db.P("@user",user),Db.P("@hours",hours));
    }
}
