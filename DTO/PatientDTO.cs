using System;

public sealed class PatientDTO
{
    public int? UserID { get; set; }
    public string FullName { get; set; }
    public string Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Gender { get; set; }
    public string Address { get; set; }
}
