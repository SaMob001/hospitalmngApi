using System.Globalization;

namespace HospitalApi.Helpers;

/// <summary>
/// The exact display formats the Flutter app expects: "23 Sep 2026" and "10:00 AM".
/// Always invariant culture, so output never depends on the server's locale.
/// </summary>
public static class DateTimeFormat
{
    public static string DisplayDate(DateOnly date) => date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

    public static string DisplayTime(TimeOnly time) => time.ToString("h:mm tt", CultureInfo.InvariantCulture);
}