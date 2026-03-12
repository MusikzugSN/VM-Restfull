#nullable enable
using Microsoft.AspNetCore.Mvc;

namespace Vereinsmanager.Utils;

public static class ErrorUtils
{
    public static ProblemDetails ValueNotFound(string item, string? identifier = null)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = item + "(" + identifier + ") not found"
        };
    }
    
    public static ProblemDetails AlreadyExists(string item, string identifier)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status303SeeOther,
            Title = item + "(" + identifier + ") already exists"
        };
    }
    
    public static ProblemDetails NotPermitted(string item, string identifier)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = item + "(" + identifier + ") is not permitted"
        };
    }

    public static ProblemDetails ValueOutOfRange(string item, string identifier)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = item + "(" + identifier + ") is out of range"
        };
    }
    
    public static ProblemDetails ValueValidationFailed(string item, string identifier)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = item + "(" + identifier + ") validation failed"
        };
    }
}