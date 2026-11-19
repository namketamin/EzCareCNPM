using System;
using System.Data;

public partial class QuanLyChuyenKhoa : BasePage
{
    public QuanLyChuyenKhoa() : base("specialties") { }
    protected override int[] AllowedRoles { get { return new[] { 1 }; } }
    protected override void LoadData(EzCareService service)
    {
        Specialties = service.Clinic.Specialties();
        EditId = Id(Q("id"));
        Editor = Find(Specialties,"SpecialtyID",EditId);
        Rules.Require(EditId == 0 || Editor != null,"Không tìm thấy chuyên khoa.");
    }

    protected override void HandlePost(EzCareService service)
    {
        var clinic = service.Clinic;
        string action = F("action");
        switch (action)
        {
            case "specialty":
                clinic.SaveSpecialty(UserAccount, Id(F("id")), F("specialtyName"), F("description"));
                Done("Đã lưu chuyên khoa.", Link("specialties"));
                break;
            default: throw new RuleException("Thao tác không hợp lệ.");
        }
    }
}
