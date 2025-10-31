using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using IdentityService.Shared.Response;
using System.Net;

namespace IdentityService.API.Filters;

public class ResultWrapperFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception == null && context.Result is ObjectResult objectResult)
        {
            var resultType = objectResult.Value?.GetType();
            if (resultType != null && IsResultType(resultType))
            {
                return;
            }
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
