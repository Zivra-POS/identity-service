using System.Net;
using System.Text.Json.Serialization;

namespace IdentityService.Shared.Response;

public class Result<T>
{
    [JsonInclude]
    public bool IsSuccess { get; private set; }
    [JsonInclude]
    public string Message { get; private set; } = string.Empty;
    [JsonInclude]
    public T? Data { get; private set; }
    [JsonInclude]
    public List<string>? Errors { get; private set; }
    [JsonInclude]
    public HttpStatusCode StatusCode { get; private set; }

    public Result() { }

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