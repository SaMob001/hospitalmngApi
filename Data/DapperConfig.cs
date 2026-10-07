using System.Data;
using Dapper;

namespace HospitalApi.Data;

/// <summary>
/// One-time Dapper setup: snake_case column mapping and DateOnly/TimeOnly support.
/// Called once from Program.cs at startup.
/// </summary>
public static class DapperConfig
{
    public static void Configure()
    {
        // doctor_name (DB column) -> DoctorName (C# property)
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        SqlMapper.AddTypeHandler(new DateOnlyHandler());
        SqlMapper.AddTypeHandler(new TimeOnlyHandler());
    }
}

public class DateOnlyHandler : SqlMapper.TypeHandler<DateOnly>
{
    // C# -> database (query parameters)
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value;
    }

    // database -> C# (reading rows)
    public override DateOnly Parse(object value) => value switch
    {
        DateOnly d => d,
        DateTime dt => DateOnly.FromDateTime(dt),
        _ => throw new InvalidCastException($"Cannot convert {value.GetType()} to DateOnly.")
    };
}

public class TimeOnlyHandler : SqlMapper.TypeHandler<TimeOnly>
{
    public override void SetValue(IDbDataParameter parameter, TimeOnly value)
    {
        parameter.DbType = DbType.Time;
        parameter.Value = value;
    }

    public override TimeOnly Parse(object value) => value switch
    {
        TimeOnly t => t,
        TimeSpan ts => TimeOnly.FromTimeSpan(ts),
        _ => throw new InvalidCastException($"Cannot convert {value.GetType()} to TimeOnly.")
    };
}