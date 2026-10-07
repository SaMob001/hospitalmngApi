namespace HospitalApi.Helpers;

/// <summary>
/// One rule for phone numbers, used by register AND login so they always compare the same string.
/// </summary>
public static class PhoneNumber
{
    /// <summary>
    /// Removes spaces, dashes and brackets; keeps a leading '+'.
    /// Returns null if the result does not have 10 to 15 digits.
    /// </summary>
    public static string? Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var hasPlus = input.TrimStart().StartsWith('+');
        var digits = new string(input.Where(char.IsAsciiDigit).ToArray());

        if (digits.Length is < 10 or > 15)
        {
            return null;
        }

        return hasPlus ? "+" + digits : digits;
    }
}