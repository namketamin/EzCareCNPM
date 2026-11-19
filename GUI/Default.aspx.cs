public partial class Default : BasePage
{
    public Default() : base("home") { }
    protected override int[] AllowedRoles { get { return new[] { 0, 1, 2, 3, 4 }; } }
    protected override void LoadData(EzCareService service)
    {
        Specialties = service.Clinic.Specialties();
        Doctors = service.Clinic.Doctors(0, "");
    }
}
