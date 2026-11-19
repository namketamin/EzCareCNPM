using System;
using System.Data;

public sealed class Schedules
{
    private readonly Db db;
    private readonly ScheduleDAL data;
    private readonly ClinicDAL clinic;
    internal Schedules(Db db)
    {
        this.db = db;
        data = new ScheduleDAL(db);
        clinic = new ClinicDAL(db);
    }

    public DataRow Detail(Actor actor, int id)
    {
        actor.Allow(Actor.Admin);
        return Rules.Row(data.Detail(id),"Không tìm thấy ca làm việc.");
    }

    public void Update(Actor actor, int id, DateTime day, TimeSpan start, TimeSpan end, int duration, string room)
    {
        actor.Allow(Actor.Admin);
        room = Rules.Text(room,"phòng khám",50);
        Rules.Require(start>=TimeSpan.Zero && end<TimeSpan.FromDays(1),"Giờ làm việc phải nằm trong cùng một ngày.");
        Rules.Require(duration == 15 || duration == 30,"Mỗi lượt khám dài 15 hoặc 30 phút.");
        Rules.Require(end>start && (end-start).TotalMinutes%duration == 0,"Giờ kết thúc phải sau giờ bắt đầu và chia đủ lượt.");
        Rules.Require(day.Add(start)>DateTime.Now,"Ca khám phải ở tương lai.");
        db.Write(() =>
        {
            DataRow row=Detail(actor,id);
            Rules.Require(((DateTime)row["WorkDate"]).Add((TimeSpan)row["StartTime"])>DateTime.Now,"Không sửa ca đã bắt đầu hoặc đã qua.");
            Rules.Require(!data.HasPendingRequest(id),"Ca đang có đơn nghỉ/đổi ca chờ duyệt. Hãy xử lý đơn trước khi sửa ca.");
            Rules.Require(data.AppointmentCount(id)==0,"Ca đã có lịch hẹn. Hãy khóa ca và xử lý dời lịch để giữ lịch sử.");
            Rules.Require((string)row["Status"]=="Open","Chỉ sửa giờ của ca đang mở.");
            Rules.Require(!data.Overlaps(id,(int)row["DoctorID"],day,start,end),"Bác sĩ đã có ca trùng giờ.");
            data.Update(id,day,start,end,duration,room);
            var step=TimeSpan.FromMinutes(duration);
            for(TimeSpan time=start;time<end;time+=step) clinic.InsertSlot(id,time,time+step);
        });
    }

    public void SetStatus(Actor actor, int id, string status, string reason)
    {
        actor.Allow(Actor.Admin);
        Rules.Require(status=="Open" || status=="Locked" || status=="Cancelled","Trạng thái ca không hợp lệ.");
        reason=Rules.Text(reason,"lý do thay đổi ca",500);
        db.Write(() =>
        {
            DataRow row=Detail(actor,id);
            Rules.Require(((DateTime)row["WorkDate"]).Add((TimeSpan)row["StartTime"])>DateTime.Now,"Chỉ thay đổi ca chưa bắt đầu.");
            Rules.Require((string)row["Status"]!=status,"Ca đã ở trạng thái được chọn.");
            if(status=="Open")
                Rules.Require(!data.Overlaps(id,(int)row["DoctorID"],(DateTime)row["WorkDate"],
                    (TimeSpan)row["StartTime"],(TimeSpan)row["EndTime"]),"Mở lại ca sẽ trùng với một ca khác.");
            if(status=="Cancelled" || status=="Open")
                Rules.Require(data.ConfirmedCount(id)==0,"Hãy xử lý hết lịch xác nhận hoặc đã đến trước khi mở lại hoặc hủy ca.");
            if(status=="Locked")
            {
                Rules.Require(!data.HasCheckedIn(id),"Ca đã có bệnh nhân được tiếp nhận. Hãy xử lý lượt khám trước khi khóa ca.");
                int request=data.InsertApprovedRequest(id,reason);
                clinic.InsertAffectedLogs(request,actor.Id,id);
            }
            data.SetStatus(id,status);
        });
    }
}
