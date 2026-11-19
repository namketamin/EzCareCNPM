using System;
using System.Data;

public sealed class ClinicDAL
{
    private readonly Db db;
    public ClinicDAL(Db db) { this.db = db; }

    public DataTable ListSpecialties()
    {
        return db.Query("SELECT * FROM Specialties ORDER BY SpecialtyName");
    }

    public object FindSpecialtyName(object name, object id)
    {
        return db.Value(@"SELECT 1 FROM Specialties WHERE SpecialtyName=@name AND SpecialtyID<>@id",
            Db.P("@name", name), Db.P("@id", id));
    }

    public int InsertSpecialty(object name, object description)
    {
        return db.Execute("INSERT Specialties(SpecialtyName,Description) VALUES(@name,@description)",
            Db.P("@name", name), Db.P("@description", description));
    }

    public int UpdateSpecialty(object name, object description, object id)
    {
        return db.Execute("UPDATE Specialties SET SpecialtyName=@name,Description=@description WHERE SpecialtyID=@id",
            Db.P("@name", name), Db.P("@description", description), Db.P("@id", id));
    }

    public DataTable ListDoctors(object specialty, object name)
    {
        return db.Query(@"SELECT d.DoctorID,u.FullName,d.Degree,d.ExperienceYears,d.Biography,s.SpecialtyName
            FROM Doctors d JOIN Users u ON u.UserID=d.UserID
            JOIN Specialties s ON s.SpecialtyID=d.SpecialtyID
            WHERE u.IsActive=1 AND (@specialty=0 OR d.SpecialtyID=@specialty)
                AND (@name=N'' OR CHARINDEX(@name,u.FullName)>0)
            ORDER BY s.SpecialtyName,u.FullName",
            Db.P("@specialty", specialty), Db.P("@name", name));
    }

    public DataTable ListSchedules(object day, object role, object user)
    {
        return db.Query(@"SELECT w.*,u.FullName AS DoctorName FROM WorkSchedules w
            JOIN Doctors d ON d.DoctorID=w.DoctorID JOIN Users u ON u.UserID=d.UserID
            WHERE w.WorkDate=@day AND (@role=1 OR d.UserID=@user) ORDER BY w.StartTime,u.FullName",
            Db.P("@day", day), Db.P("@role", role), Db.P("@user", user));
    }

    public object FindActiveDoctor(object doctor)
    {
        return db.Value(@"SELECT 1 FROM Doctors d JOIN Users u ON u.UserID=d.UserID
                WHERE d.DoctorID=@doctor AND u.IsActive=1",
            Db.P("@doctor", doctor));
    }

    public object FindOverlappingSchedule(object doctor, object day, object start, object end)
    {
        return db.Value(@"SELECT 1 FROM WorkSchedules
                WHERE DoctorID=@doctor AND WorkDate=@day AND Status<>'Cancelled'
                AND StartTime<@end AND EndTime>@start",
            Db.P("@doctor", doctor), Db.P("@day", day), Db.P("@start", start), Db.P("@end", end));
    }

    public object InsertSchedule(object doctor, object day, object start, object end, object duration, object room)
    {
        return db.Value(@"INSERT WorkSchedules(DoctorID,WorkDate,StartTime,EndTime,SlotDuration,Room)
                OUTPUT INSERTED.ScheduleID VALUES(@doctor,@day,@start,@end,@duration,@room)",
            Db.P("@doctor", doctor), Db.P("@day", day), Db.P("@start", start), Db.P("@end", end), Db.P("@duration", duration), Db.P("@room", room));
    }

    public int InsertSlot(object id, object start, object end)
    {
        return db.Execute("INSERT TimeSlots(ScheduleID,StartTime,EndTime) VALUES(@id,@start,@end)",
            Db.P("@id", id), Db.P("@start", start), Db.P("@end", end));
    }

    public DataTable ListAvailableSlots(object doctor, object day)
    {
        return db.Query(@"SELECT * FROM SlotDetails s
            WHERE s.DoctorID=@doctor AND s.WorkDate=@day AND s.IsActive=1 AND s.Status='Open' AND s.IsBooked=0
            AND s.StartsAt>SYSDATETIME() AND NOT EXISTS
                (SELECT 1 FROM Appointments a WHERE a.SlotID=s.SlotID AND a.Status<>'Cancelled')
            AND NOT EXISTS (SELECT 1 FROM AppointmentDetails a WHERE a.DoctorID=s.DoctorID
                AND a.WorkDate=s.WorkDate AND a.Status<>'Cancelled'
                AND a.StartTime<s.EndTime AND a.EndTime>s.StartTime)
            ORDER BY s.StartTime",
            Db.P("@doctor", doctor), Db.P("@day", day));
    }

    public DataTable ListSlotOverview(int doctor, DateTime day, int patient, int current)
    {
        return db.Query(@"SELECT s.*,
            CASE WHEN s.Status<>'Open' OR s.IsActive=0 THEN 'Closed'
                WHEN s.StartsAt<=SYSDATETIME() THEN 'Past'
                WHEN s.IsBooked=1 OR EXISTS (
                    SELECT 1 FROM AppointmentDetails a WHERE a.DoctorID=s.DoctorID
                        AND a.WorkDate=s.WorkDate AND a.Status<>'Cancelled'
                        AND a.StartTime<s.EndTime AND a.EndTime>s.StartTime) THEN 'Booked'
                WHEN @patient>0 AND EXISTS (
                    SELECT 1 FROM AppointmentDetails a WHERE a.PatientID=@patient
                        AND a.AppointmentID<>@current AND a.WorkDate=s.WorkDate AND a.Status<>'Cancelled'
                        AND a.StartTime<s.EndTime AND a.EndTime>s.StartTime) THEN 'PatientBusy'
                ELSE 'Free' END AS SlotState
            FROM SlotDetails s WHERE s.DoctorID=@doctor AND s.WorkDate=@day
            ORDER BY s.StartTime,s.SlotID",Db.P("@doctor",doctor),Db.P("@day",day),
            Db.P("@patient",patient),Db.P("@current",current));
    }

    public object NextAvailableDay(int doctor)
    {
        return db.Value(@"SELECT MIN(s.WorkDate) FROM SlotDetails s
            WHERE s.DoctorID=@doctor AND s.IsActive=1 AND s.Status='Open'
                AND s.IsBooked=0 AND s.StartsAt>SYSDATETIME()
                AND NOT EXISTS (SELECT 1 FROM AppointmentDetails a
                    WHERE a.DoctorID=s.DoctorID AND a.WorkDate=s.WorkDate AND a.Status<>'Cancelled'
                    AND a.StartTime<s.EndTime AND a.EndTime>s.StartTime)",Db.P("@doctor",doctor));
    }

    public object NextScheduleDay(int user)
    {
        return db.Value(@"SELECT MIN(w.WorkDate) FROM WorkSchedules w
            JOIN Doctors d ON d.DoctorID=w.DoctorID
            WHERE d.UserID=@user AND w.WorkDate>=CAST(SYSDATETIME() AS date)
                AND w.Status<>'Cancelled'",Db.P("@user",user));
    }

    public DataTable ListShiftRequests(object role, object user)
    {
        return db.Query(@"SELECT r.*,w.WorkDate,w.StartTime,w.EndTime,u.FullName AS DoctorName
            FROM DoctorShiftRequests r JOIN WorkSchedules w ON w.ScheduleID=r.ScheduleID
            JOIN Doctors d ON d.DoctorID=w.DoctorID JOIN Users u ON u.UserID=d.UserID
            WHERE (@role=1 OR d.UserID=@user)
            ORDER BY CASE WHEN r.Status='Pending' THEN 0 ELSE 1 END,r.CreatedAt DESC",
            Db.P("@role", role), Db.P("@user", user));
    }

    public object FindFutureDoctorSchedule(object id, object user)
    {
        return db.Value(@"SELECT 1 FROM WorkSchedules w JOIN Doctors d ON d.DoctorID=w.DoctorID
                WHERE w.ScheduleID=@id AND d.UserID=@user AND w.Status='Open'
                AND (w.WorkDate>CAST(SYSDATETIME() AS date) OR
                     (w.WorkDate=CAST(SYSDATETIME() AS date) AND w.StartTime>CAST(SYSDATETIME() AS time)))",
            Db.P("@id", id), Db.P("@user", user));
    }

    public object FindPendingRequest(object id)
    {
        return db.Value(@"SELECT 1 FROM DoctorShiftRequests WHERE ScheduleID=@id AND Status='Pending'",
            Db.P("@id", id));
    }

    public int InsertShiftRequest(object id, object type, object reason)
    {
        return db.Execute("INSERT DoctorShiftRequests(ScheduleID,RequestType,Reason) VALUES(@id,@type,@reason)",
            Db.P("@id", id), Db.P("@type", type), Db.P("@reason", reason));
    }

    public DataTable GetPendingRequest(object id)
    {
        return db.Query(@"SELECT r.ScheduleID,w.WorkDate,w.StartTime,w.Status
                FROM DoctorShiftRequests r JOIN WorkSchedules w ON w.ScheduleID=r.ScheduleID
                WHERE r.RequestID=@id AND r.Status='Pending'",
            Db.P("@id", id));
    }

    public int LockSchedule(object id)
    {
        return db.Execute("UPDATE WorkSchedules SET Status='Locked' WHERE ScheduleID=@id",
            Db.P("@id", id));
    }

    public int InsertAffectedLogs(object request, object user, object schedule)
    {
        return db.Execute(@"INSERT RescheduleLogs(AppointmentID,RequestID,HandledByUserID,OldSlotID,ActionStatus)
                    SELECT AppointmentID,@request,@user,SlotID,'PendingContact' FROM AppointmentDetails
                    WHERE ScheduleID=@schedule AND Status='Confirmed'",
            Db.P("@request", request), Db.P("@user", user), Db.P("@schedule", schedule));
    }

    public int ReviewShiftRequest(object status, object id)
    {
        return db.Execute("UPDATE DoctorShiftRequests SET Status=@status WHERE RequestID=@id",
            Db.P("@status", status), Db.P("@id", id));
    }
}
