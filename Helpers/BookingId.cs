namespace HospitalApi.Helpers;

/// <summary>Parses the "APT-00021" display format back into the raw database id.</summary>
public static class BookingId
{
    private const string Prefix = "APT-";

    /// <summary>Returns null if the format is not exactly "APT-" followed by digits.</summary>
    public static int? Parse(string? bookingId)
    {
        if (string.IsNullOrWhiteSpace(bookingId) || !bookingId.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return null;
        }

        var digits = bookingId[Prefix.Length..];
        return int.TryParse(digits, out var id) ? id : null;
    }

    /// <summary>Formats 21 into "APT-00021".</summary>
    public static string Format(int appointmentId) => $"{Prefix}{appointmentId:D5}";
}