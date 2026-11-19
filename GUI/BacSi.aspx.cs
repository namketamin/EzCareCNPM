using System;
public partial class BacSi : BasePage
{
    public BacSi() : base("doctors") { TitleText = "Danh sách bác sĩ"; }
    protected override void LoadData(EzCareService service)
    {
        SpecialtyId = Id(Q("specialty"));
        DoctorName = Q("name");
        Specialties = service.Clinic.Specialties();
        Doctors = service.Clinic.Doctors(SpecialtyId, DoctorName);
    }
}
