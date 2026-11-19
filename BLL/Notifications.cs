using System;
using System.Configuration;
using System.Data;

public sealed class Notifications
{
    private readonly NotificationsDAL data;
    internal Notifications(Db db) { data = new NotificationsDAL(db); }

    public DataTable List(Actor actor)
    {
        actor.Allow(Actor.Patient);
        return data.List(actor.Id);
    }

    public DataTable Reminders(Actor actor)
    {
        actor.Allow(Actor.Patient);
        int hours;
        if (!Int32.TryParse(ConfigurationManager.AppSettings["ReminderHoursBefore"],out hours)) hours=24;
        return data.Reminders(actor.Id,Math.Max(0,Math.Min(hours,168)));
    }
}
