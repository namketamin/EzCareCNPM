using System;
using System.Data;

public sealed class ScheduleDAL
{
    private readonly Db db;
    public ScheduleDAL(Db db) { this.db = db; }

    public DataTable Detail(int id)
    {
        return db.Query(@"SELECT w.*,u.FullName AS DoctorName
            FROM WorkSchedules w JOIN Doctors d ON d.DoctorID=w.DoctorID
            JOIN Users u ON u.UserID=d.UserID WHERE w.ScheduleID=@id",Db.P("@id",id));
    }

    public int AppointmentCount(int id)
    {
        return Convert.ToInt32(db.Value(@"SELECT COUNT(*) FROM TimeSlots t WHERE t.ScheduleID=@id
            AND (EXISTS(SELECT 1 FROM Appointments a WHERE a.SlotID=t.SlotID)
                OR EXISTS(SELECT 1 FROM RescheduleLogs l WHERE l.OldSlotID=t.SlotID OR l.NewSlotID=t.SlotID))",Db.P("@id",id)));
    }

    public bool HasPendingRequest(int id)
    {
        return db.Value("SELECT 1 FROM DoctorShiftRequests WHERE ScheduleID=@id AND Status='Pending'",Db.P("@id",id))!=null;
    }

    public bool HasCheckedIn(int id)
    {
        return db.Value("SELECT 1 FROM AppointmentDetails WHERE ScheduleID=@id AND Status='CheckedIn'",Db.P("@id",id))!=null;
    }

    public int ConfirmedCount(int id)
    {
        return Convert.ToInt32(db.Value(@"SELECT COUNT(*) FROM Appointments a JOIN TimeSlots t ON t.SlotID=a.SlotID
            WHERE t.ScheduleID=@id AND a.Status IN ('Confirmed','CheckedIn')",Db.P("@id",id)));
    }

    public void Update(int id, DateTime date, TimeSpan start, TimeSpan end, int duration, string room)
    {
        db.Execute(@"DELETE TimeSlots WHERE ScheduleID=@id;
            UPDATE WorkSchedules SET WorkDate=@date,StartTime=@start,EndTime=@end,SlotDuration=@duration,Room=@room
            WHERE ScheduleID=@id",Db.P("@id",id),Db.P("@date",date),Db.P("@start",start),Db.P("@end",end),
            Db.P("@duration",duration),Db.P("@room",room));
    }

    public bool Overlaps(int id, int doctor, DateTime date, TimeSpan start, TimeSpan end)
    {
        return db.Value(@"SELECT 1 FROM WorkSchedules WHERE ScheduleID<>@id AND DoctorID=@doctor
            AND WorkDate=@date AND Status<>'Cancelled' AND StartTime<@end AND EndTime>@start",
            Db.P("@id",id),Db.P("@doctor",doctor),Db.P("@date",date),Db.P("@start",start),Db.P("@end",end)) != null;
    }

    public void SetStatus(int id, string status)
    {
        db.Execute("UPDATE WorkSchedules SET Status=@status WHERE ScheduleID=@id",Db.P("@id",id),Db.P("@status",status));
    }

    public int InsertApprovedRequest(int schedule, string reason)
    {
        db.Execute("UPDATE DoctorShiftRequests SET Status='Rejected' WHERE ScheduleID=@id AND Status='Pending'",Db.P("@id",schedule));
        return (int)db.Value(@"INSERT DoctorShiftRequests(ScheduleID,RequestType,Reason,Status)
            OUTPUT INSERTED.RequestID VALUES(@id,'DoiCa',@reason,'Approved')",Db.P("@id",schedule),Db.P("@reason",reason));
    }
}
