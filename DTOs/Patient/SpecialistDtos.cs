namespace HospitalApi.DTOs.Patient;

/// <summary>Matches the Flutter Department model exactly: id, name, doctorName, availability.</summary>
public class SpecialistResponse
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string DoctorName { get; set; } = "";
    public string Availability { get; set; } = "";
}

public class SlotResponse
{
    /// <summary>24-hour "HH:mm" format, e.g. "09:30".</summary>
    public string Time { get; set; } = "";
}