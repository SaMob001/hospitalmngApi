namespace HospitalApi.Models;

/// <summary>Row from sp_get_specialists().</summary>
public class SpecialistRecord
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string DoctorName { get; set; } = "";
    public string Availability { get; set; } = "";
}