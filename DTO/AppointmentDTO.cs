public sealed class AppointmentDTO
{
    public string BookingCode { get; set; }
    public int PatientID { get; set; }
    public int SlotID { get; set; }
    public string Reason { get; set; }
    public int CreatedByUserID { get; set; }
}
