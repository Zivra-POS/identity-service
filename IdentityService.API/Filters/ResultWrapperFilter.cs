using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using IdentityService.Shared.Response;
using System.Net;
using System.Text.Json;

namespace IdentityService.API.Filters;

public class ResultWrapperFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Do nothing before action executes
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Only wrap if the result is not already a Result<T> and no exception occurred
        if (context.Exception == null && context.Result is ObjectResult objectResult)
        {
            // Check if the result is already a Result<T>
            var resultType = objectResult.Value?.GetType();
            if (resultType != null && IsResultType(resultType))
            {
                // Already a Result<T>, don't wrap
                return;
            }

            // Wrap the response in Result<T>
            var statusCode = (HttpStatusCode)(objectResult.StatusCode ?? 200);
            var wrappedResult = Result<object>.Success(
                objectResult.Value,
                GetSuccessMessage(statusCode),
                statusCode
            );

            context.Result = new ObjectResult(wrappedResult)
            {
                StatusCode = objectResult.StatusCode
            };
        }
    }

    private static bool IsResultType(Type type)
    {
        // Check if type is Result<T>
        if (type.IsGenericType)
        {
            var genericType = type.GetGenericTypeDefinition();
            return genericType == typeof(Result<>);
        }
        return false;
    }

    private static string GetSuccessMessage(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.Created => "Resource created successfully",
            HttpStatusCode.NoContent => "Operation completed successfully",
            HttpStatusCode.Accepted => "Request accepted",
            _ => "Operation completed successfully"
        };
    }
}
