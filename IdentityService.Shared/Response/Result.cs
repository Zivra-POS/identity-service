using System.Net;

namespace IdentityService.Shared.Response;

public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public T? Data { get; private set; }
    public List<string>? Errors { get; private set; }
    public HttpStatusCode StatusCode { get; private set; }

    private Result() { }

    public static Result<T> Success(T data, string message = "Success", HttpStatusCode code = HttpStatusCode.OK)
        => new()
        {
            IsSuccess = true,
            Data = data,
            Message = message,
            StatusCode = code
        };

    public static Result<T> Failure(List<string> errors, string message = "Failed", HttpStatusCode code = HttpStatusCode.BadRequest)
        => new()
        {
            IsSuccess = false,
            Errors = errors,
            Message = message,
            StatusCode = code
        };
}