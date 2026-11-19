using System;
using System.Data;

public sealed class BookingsDAL
{
    private readonly Db db;
    public BookingsDAL(Db db) { this.db = db; }

    public void SetBooked(int slot, bool booked)
    {
        db.Execute("UPDATE TimeSlots SET IsBooked=@booked WHERE SlotID=@id",
            Db.P("@booked", booked), Db.P("@id", slot));
    }

    public DataTable ListAppointments(object role, object user, object search, object day, object doctor, int schedule, string status)
    {
        return db.Query(@"SELECT * FROM AppointmentDetails
            WHERE (@role IN (1,3) OR (@role=4 AND PatientUserID=@user) OR (@role=2 AND DoctorUserID=@user))
              AND (@search IS NULL OR BookingCode=@search OR Phone=@search)
              AND (@day IS NULL OR WorkDate=@day)
              AND (@doctor=0 OR DoctorID=@doctor)
              AND (@schedule=0 OR ScheduleID=@schedule)
              AND (@status=N'' OR Status=@status)
            ORDER BY WorkDate,StartTime,AppointmentID",
            Db.P("@role", role), Db.P("@user", user), Db.P("@search", search), Db.P("@day", day), Db.P("@doctor", doctor),
            Db.P("@schedule",schedule), Db.P("@status",status));
    }

    public DataTable GetAppointment(object id, object role, object user)
    {
        return db.Query(@"SELECT * FROM AppointmentDetails WHERE AppointmentID=@id AND
            (@role IN (1,3) OR (@role=4 AND PatientUserID=@user) OR (@role=2 AND DoctorUserID=@user))",
            Db.P("@id", id), Db.P("@role", role), Db.P("@user", user));
    }

    public DataTable ListAffected()
    {
        return db.Query(@"SELECT a.*, r.RequestID,r.ActionStatus,r.Notes
            FROM AppointmentDetails a
            JOIN RescheduleLogs r ON r.AppointmentID=a.AppointmentID AND r.RequestID IS NOT NULL
            WHERE a.Status='Confirmed' AND a.ScheduleStatus='Locked'
                AND r.LogID=(SELECT MAX(l.LogID) FROM RescheduleLogs l
                    WHERE l.AppointmentID=a.AppointmentID AND l.RequestID=r.RequestID)
                AND r.ActionStatus IN ('PendingContact','Contacted')
            ORDER BY a.WorkDate,a.StartTime");
    }

    public DataTable GetHistory(object id)
    {
        return db.Query(@"SELECT l.*,u.FullName AS HandledBy,old.WorkDate AS OldDate,
            old.StartTime AS OldTime, nw.WorkDate AS NewDate, nw.StartTime AS NewTime
            FROM RescheduleLogs l JOIN Users u ON u.UserID=l.HandledByUserID
            LEFT JOIN SlotDetails old ON old.SlotID=l.OldSlotID
            LEFT JOIN SlotDetails nw ON nw.SlotID=l.NewSlotID
            WHERE l.AppointmentID=@id ORDER BY l.LogID DESC",
            Db.P("@id", id));
    }

    public object GetPatientId(object id)
    {
        return db.Value("SELECT PatientID FROM Patients WHERE UserID=@id",
            Db.P("@id", id));
    }

    public object FindPatient(object id)
    {
        return db.Value("SELECT 1 FROM Patients WHERE PatientID=@id",
            Db.P("@id", id));
    }

    public DataTable GetFreeSlot(object id)
    {
        return db.Query(@"SELECT * FROM SlotDetails s WHERE s.SlotID=@id AND s.Status='Open' AND s.IsBooked=0
                AND s.IsActive=1 AND s.StartsAt>SYSDATETIME()
                AND NOT EXISTS (SELECT 1 FROM Appointments WHERE SlotID=s.SlotID AND Status<>'Cancelled')",
            Db.P("@id", id));
    }

    public object FindPatientConflict(object patient, object current, object date, object start, object end)
    {
        return db.Value(@"SELECT 1 FROM AppointmentDetails WHERE PatientID=@patient
            AND AppointmentID<>@current AND Status<>'Cancelled' AND WorkDate=@date
            AND StartTime<@end AND EndTime>@start",
            Db.P("@patient", patient), Db.P("@current", current), Db.P("@date", date), Db.P("@start", start), Db.P("@end", end));
    }

    public object FindDoctorConflict(object doctor, object current, object date, object start, object end)
    {
        return db.Value(@"SELECT 1 FROM AppointmentDetails WHERE DoctorID=@doctor
            AND AppointmentID<>@current AND Status<>'Cancelled' AND WorkDate=@date
            AND StartTime<@end AND EndTime>@start",
            Db.P("@doctor", doctor), Db.P("@current", current), Db.P("@date", date), Db.P("@start", start), Db.P("@end", end));
    }

    public object InsertAppointment(AppointmentDTO appointment)
    {
        return db.Value(@"INSERT Appointments(BookingCode,PatientID,SlotID,Reason,CreatedByUserID)
                OUTPUT INSERTED.AppointmentID VALUES(@code,@patient,@slot,@reason,@by)",
            Db.P("@code", appointment.BookingCode), Db.P("@patient", appointment.PatientID),
            Db.P("@slot", appointment.SlotID), Db.P("@reason", appointment.Reason), Db.P("@by", appointment.CreatedByUserID));
    }

    public object GetApprovedRequest(object id)
    {
        return db.Value(@"SELECT TOP (1) RequestID FROM DoctorShiftRequests
            WHERE ScheduleID=@id AND Status='Approved' ORDER BY RequestID DESC",
            Db.P("@id", id));
    }

    public int InsertLog(object id, object request, object user, object oldSlot, object newSlot, object status, object note)
    {
        return db.Execute(@"INSERT RescheduleLogs(AppointmentID,RequestID,HandledByUserID,OldSlotID,NewSlotID,ActionStatus,Notes)
            VALUES(@id,@request,@by,@old,@new,@status,@note)",
            Db.P("@id", id), Db.P("@request", request), Db.P("@by", user), Db.P("@old", oldSlot), Db.P("@new", newSlot), Db.P("@status", status), Db.P("@note", note));
    }

    public int MoveAppointment(object slot, object id)
    {
        return db.Execute("UPDATE Appointments SET SlotID=@slot WHERE AppointmentID=@id",
            Db.P("@slot", slot), Db.P("@id", id));
    }

    public int CancelAppointment(object id)
    {
        return db.Execute("UPDATE Appointments SET Status='Cancelled' WHERE AppointmentID=@id",
            Db.P("@id", id));
    }

    public int UpdateStatus(object status, object id)
    {
        return db.Execute("UPDATE Appointments SET Status=@status WHERE AppointmentID=@id",
            Db.P("@status", status), Db.P("@id", id));
    }
}
