using System.Text;
using HospitalApi.Data;
using HospitalApi.Helpers;
using HospitalApi.Middleware;
using HospitalApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using HospitalApi.Repositories;


const string CorsPolicyName = "WebApps";

// Local dev helper: dotnet run -- --hash 'Password'   prints a BCrypt hash and exits.
// It never starts the web server, so it is not reachable over HTTP.
if (args.Length == 2 && args[0] == "--hash")
{
    Console.WriteLine(BCrypt.Net.BCrypt.HashPassword(args[1]));
    return;
}

var builder = WebApplication.CreateBuilder(args);

DapperConfig.Configure();

// Add services to the container.

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Our own 400 shape (short friendly "detail") instead of ASP.NET's default.
        options.InvalidModelStateResponseFactory = ValidationProblemFactory.Create;
    });
builder.Services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();

// Built-in OpenAPI document generation (the machine-readable spec).
builder.Services.AddOpenApi();

// ---- JWT authentication -------------------------------------------------
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

var jwt = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key is missing or shorter than 32 characters. Set it with dotnet user-secrets.");
}

builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IAmbulanceRepository, AmbulanceRepository>();
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection(AuthSettings.SectionName));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAmbulanceService, AmbulanceService>();


builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // keep claim names exactly as issued: "sub", "role", "name"
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "name",
            RoleClaimType = "role"
        };
        options.Events = AuthEvents.Create();
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.PatientOnly, policy => policy.RequireRole(Roles.Patient))
    .AddPolicy(Policies.DoctorOnly, policy => policy.RequireRole(Roles.Doctor))
    .AddPolicy(Policies.AdminOnly, policy => policy.RequireRole(Roles.Admin));

// ---- CORS ---------------------------------------------------------------
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
        policy.WithOrigins(allowedOrigins)
              .WithHeaders("Content-Type", "Authorization")
              .WithMethods("GET", "POST", "PUT", "DELETE"));
});

var app = builder.Build();
app.Logger.LogWarning("DEV ONLY: fixed patient OTP login is enabled. Replace it with a real OTP provider before any real use.");

// MUST be first: it can only catch errors from middleware registered after it.
app.UseMiddleware<ExceptionHandlingMiddleware>();

// API documentation is only exposed while developing.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Hospital API v1");
        options.DocumentTitle = "Hospital API";
    });
}

app.UseHttpsRedirection();

app.UseCors(CorsPolicyName);      // 1st: CORS
app.UseAuthentication();          // 2nd: who are you?
app.UseAuthorization();           // 3rd: are you allowed?

app.MapControllers();

app.Run();