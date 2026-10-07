using System.Security.Cryptography;
using System.Text;
using HospitalApi.DTOs.Auth;
using HospitalApi.Exceptions;
using HospitalApi.Helpers;
using HospitalApi.Repositories;
using Microsoft.Extensions.Options;
using Npgsql;

namespace HospitalApi.Services;

public class AuthService : IAuthService
{
    private const string InvalidPatientLogin = "Invalid mobile number or OTP.";
    private const string InvalidStaffLogin = "Invalid email or password.";
    private const string InvalidPhone = "Please enter a valid mobile number.";

    // Used to spend the same time on a BCrypt check even when the email does not exist,
    // so response time does not reveal which emails are registered.
    private static readonly string DummyHash =
        BCrypt.Net.BCrypt.HashPassword("timing-equalizer-not-a-real-password");

    private readonly IAuthRepository _repo;
    private readonly IJwtTokenService _jwt;
    private readonly AuthSettings _settings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IAuthRepository repo, IJwtTokenService jwt,
        IOptions<AuthSettings> settings, ILogger<AuthService> logger)
    {
        _repo = repo;
        _jwt = jwt;
        _settings = settings.Value;
        _logger = logger;
    }

    // ---------------------------------------------------------------- patient

    public async Task<PatientAuthResponse> RegisterPatientAsync(RegisterPatientRequest request)
    {
        // Business rules (the DTO already checked the shape).
        var phone = PhoneNumber.Normalize(request.Phone)
            ?? throw new BadRequestException(InvalidPhone);

        var fullName = request.FullName.Trim();
        if (fullName.Length < 2)
        {
            throw new BadRequestException("Full name must be between 2 and 120 characters.");
        }

        var email = request.Email.Trim().ToLowerInvariant();

        if (request.Dob is { } dob)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (dob > today)
            {
                throw new BadRequestException("Date of birth cannot be in the future.");
            }
            if (dob < new DateOnly(1900, 1, 1))
            {
                throw new BadRequestException("Please enter a valid date of birth.");
            }
        }

        var gender = string.IsNullOrWhiteSpace(request.Gender) ? null : request.Gender;

        // patients.password_hash is NOT NULL but unused (patients log in with an OTP),
        // so store a hash of a random token as a placeholder. Nobody knows the token.
        var placeholderHash = BCrypt.Net.BCrypt.HashPassword(
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));

        int patientId;
        try
        {
            patientId = await _repo.RegisterPatientAsync(
                fullName, email, phone, placeholderHash, request.Dob, gender);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            var message = ex.ConstraintName switch
            {
                { } c when c.Contains("phone", StringComparison.OrdinalIgnoreCase)
                    => "This mobile number is already registered. Please log in instead.",
                { } c when c.Contains("email", StringComparison.OrdinalIgnoreCase)
                    => "This email address is already registered.",
                _ => "An account with these details already exists."
            };
            throw new ConflictException(message);
        }

        _logger.LogInformation("Patient {PatientId} registered", patientId);
        return BuildPatientResponse(patientId, fullName, phone);
    }

    public async Task<PatientAuthResponse> LoginPatientAsync(PatientLoginRequest request)
    {
        var phone = PhoneNumber.Normalize(request.Phone)
            ?? throw new BadRequestException(InvalidPhone);

        // Check the OTP first: a wrong OTP needs no database call.
        if (!OtpMatches(request.Otp))
        {
            _logger.LogWarning("Failed patient login (invalid credentials)");
            throw new UnauthorizedException(InvalidPatientLogin);
        }

        var patient = await _repo.GetPatientByPhoneAsync(phone);
        if (patient is null)
        {
            // Same message as a wrong OTP, so attackers cannot tell which numbers are registered.
            _logger.LogWarning("Failed patient login (invalid credentials)");
            throw new UnauthorizedException(InvalidPatientLogin);
        }

        _logger.LogInformation("Patient {PatientId} logged in", patient.PatientId);
        return BuildPatientResponse(patient.PatientId, patient.FullName, patient.Phone);
    }

    // ------------------------------------------------------------------ staff

    public async Task<StaffAuthResponse> LoginDoctorAsync(StaffLoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var doctor = await _repo.GetDoctorByEmailAsync(email);

        // Always runs one BCrypt check, even when the doctor does not exist.
        var passwordOk = VerifyPassword(request.Password, doctor?.PasswordHash);

        if (doctor is null || !passwordOk)
        {
            _logger.LogWarning("Failed doctor login (invalid credentials)");
            throw new UnauthorizedException(InvalidStaffLogin);
        }

        // Only reveal the account status to someone who proved they know the password.
        if (!string.Equals(doctor.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Doctor {DoctorId} login blocked: status is {Status}", doctor.DoctorId, doctor.Status);
            throw new ForbiddenException(
                "Your account is not active. Please contact the hospital administrator.");
        }

        _logger.LogInformation("Doctor {DoctorId} logged in", doctor.DoctorId);
        return BuildStaffResponse(doctor.DoctorId, doctor.FullName, doctor.Email, Roles.Doctor);
    }

    public async Task<StaffAuthResponse> LoginAdminAsync(StaffLoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var admin = await _repo.GetAdminByEmailAsync(email);

        var passwordOk = VerifyPassword(request.Password, admin?.PasswordHash);

        if (admin is null || !passwordOk)
        {
            _logger.LogWarning("Failed admin login (invalid credentials)");
            throw new UnauthorizedException(InvalidStaffLogin);
        }

        _logger.LogInformation("Admin {AdminId} logged in", admin.AdminId);
        return BuildStaffResponse(admin.AdminId, admin.FullName, admin.Email, Roles.Admin);
    }

    // ---------------------------------------------------------------- helpers

    private bool OtpMatches(string supplied)
    {
        // Constant-time comparison: the time taken does not depend on how many characters match.
        var a = Encoding.UTF8.GetBytes(supplied);
        var b = Encoding.UTF8.GetBytes(_settings.DevOnlyFixedOtp);
        return CryptographicOperations.FixedTimeEquals(a, b);
    }

    private static bool VerifyPassword(string password, string? storedHash)
    {
        if (storedHash is null)
        {
            BCrypt.Net.BCrypt.Verify(password, DummyHash); // spend the same time, ignore the result
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }
        catch (Exception)
        {
            // The stored value is not a valid BCrypt hash (for example the seeded placeholder values).
            // Treat it as a wrong password instead of crashing with a 500.
            return false;
        }
    }

    private PatientAuthResponse BuildPatientResponse(int patientId, string fullName, string phone)
    {
        var (token, _) = _jwt.CreateToken(patientId, fullName, Roles.Patient);
        return new PatientAuthResponse
        {
            Token = token,
            User = new PatientUserDto
            {
                Id = IdFormatter.Patient(patientId),
                Name = fullName,
                MobileNumber = phone,
                HospitalName = _settings.HospitalName
            }
        };
    }

    private StaffAuthResponse BuildStaffResponse(int id, string fullName, string email, string role)
    {
        var (token, _) = _jwt.CreateToken(id, fullName, role);
        return new StaffAuthResponse
        {
            Token = token,
            User = new StaffUserDto { Id = id, Name = fullName, Email = email, Role = role }
        };
    }
}