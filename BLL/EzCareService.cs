using System;

public sealed class EzCareService : IDisposable
{
    private readonly Db db;
    public Accounts Accounts { get; private set; }
    public Clinic Clinic { get; private set; }
    public Bookings Bookings { get; private set; }
    public Schedules Schedules { get; private set; }
    public Notifications Notifications { get; private set; }

    public EzCareService()
    {
        db = new Db();
        Accounts = new Accounts(db);
        Clinic = new Clinic(db);
        Bookings = new Bookings(db);
        Schedules = new Schedules(db);
        Notifications = new Notifications(db);
    }

    public void Dispose() { db.Dispose(); }
}
