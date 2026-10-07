namespace HospitalApi.Helpers;

/// <summary>Settings from the "Auth" section of appsettings.json.</summary>
public class AuthSettings
{
    public const string SectionName = "Auth";

    /// <summary>
    /// DEV ONLY. A fixed OTP accepted for every patient, so the app can be built and tested
    /// without an SMS provider. This is insecure by design: anyone who knows a registered phone
    /// number can log in as that patient. Replace with a real OTP service before any real use.
    /// </summary>
    public string DevOnlyFixedOtp { get; set; } = "";

    /// <summary>Shown in the patient login response (the Flutter app displays it).</summary>
    public string HospitalName { get; set; } = "";
}