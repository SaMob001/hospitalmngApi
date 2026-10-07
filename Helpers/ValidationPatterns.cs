namespace HospitalApi.Helpers;

/// <summary>
/// Shared validation patterns so every DTO (auth now, admin later) uses the same rules.
/// </summary>
public static class ValidationPatterns
{
    // Shape only: optional leading +, then digits, spaces, dashes or brackets.
    // The service layer normalizes it and checks the digit count (10 to 15).
    public const string Phone = @"^\+?[0-9 \-()]{10,20}$";

    // Matches the database enum gender_type. Null or empty is allowed (gender is optional).
    public const string Gender = "^(male|female|other)$";
}