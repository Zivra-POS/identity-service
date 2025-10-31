using System.Net;

namespace IdentityService.Core.Exceptions;

public class ValidationException : Exception
{
    public List<string> Errors { get; }
    public HttpStatusCode StatusCode { get; }

    public ValidationException(List<string> errors, HttpStatusCode statusCode = HttpStatusCode.BadRequest) 
        : base("Validasi gagal")
    {
        Errors = errors;
        StatusCode = statusCode;
    }

    public ValidationException(string error, HttpStatusCode statusCode = HttpStatusCode.BadRequest) 
        : base("Validasi gagal")
    {
        Errors = new List<string> { error };
        StatusCode = statusCode;
    }
}

public class BusinessException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public BusinessException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest) 
        : base(message)
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string name, object key) : base($"{name} dengan id '{key}' tidak ditemukan.") { }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}
