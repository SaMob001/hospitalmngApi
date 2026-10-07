namespace HospitalApi.Exceptions;

/// <summary>
/// Base class for "expected" business errors. Services throw these; the
/// exception middleware converts each one into the matching HTTP response.
/// </summary>
public abstract class AppException : Exception
{
    public int StatusCode { get; }

    protected AppException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class BadRequestException : AppException
{
    public BadRequestException(string message) : base(400, message) { }
}

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message) : base(401, message) { }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(403, message) { }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(404, message) { }
}

public class ConflictException : AppException
{
    public ConflictException(string message) : base(409, message) { }
}