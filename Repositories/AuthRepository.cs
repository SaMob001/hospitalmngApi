using Dapper;
using HospitalApi.Data;
using HospitalApi.Models;

namespace HospitalApi.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly IDbConnectionFactory _db;

    public AuthRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<int> RegisterPatientAsync(string fullName, string email, string phone,
        string passwordHash, DateOnly? dob, string? gender)
    {
        await using var conn = _db.CreateConnection();

        // Scalar-returning function: SELECT fn(...), not SELECT * FROM.
        // Explicit casts: PostgreSQL cannot infer the type of a NULL parameter,
        // and gender_type is an enum, so text must be cast to it.
        return await conn.ExecuteScalarAsync<int>(
            "SELECT sp_patient_register(@FullName, @Email, @Phone, @PasswordHash, @Dob::date, @Gender::gender_type)",
            new { FullName = fullName, Email = email, Phone = phone, PasswordHash = passwordHash, Dob = dob, Gender = gender });
    }

    public async Task<PatientLoginRecord?> GetPatientByPhoneAsync(string phone)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<PatientLoginRecord>(
            "SELECT * FROM sp_patient_login_lookup_by_phone(@Phone)",
            new { Phone = phone });
    }

    public async Task<DoctorLoginRecord?> GetDoctorByEmailAsync(string email)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<DoctorLoginRecord>(
            "SELECT * FROM sp_doctor_login_lookup(@Email)",
            new { Email = email });
    }

    public async Task<AdminLoginRecord?> GetAdminByEmailAsync(string email)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<AdminLoginRecord>(
            "SELECT * FROM sp_admin_login_lookup(@Email)",
            new { Email = email });
    }
}