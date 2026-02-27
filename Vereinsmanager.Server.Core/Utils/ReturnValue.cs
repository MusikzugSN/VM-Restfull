#nullable enable
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;

namespace Vereinsmanager.Utils;

public class ReturnValue<TReturnType>
{
    private readonly TReturnType? _value;
    private readonly ProblemDetails? _problemDetails;
    
    public ReturnValue(TReturnType value)
    {
        _value = value;
    }
    
    public ReturnValue(ProblemDetails problemDetails)
    {
        _problemDetails = problemDetails;
    }
    
    public TReturnType? GetValue()
    {
        return _value;
    }

    public ProblemDetails? GetProblemDetails()
    {
        return _problemDetails;
    }
    
    public bool IsSuccessful()
    {
        return _value != null;
    }

    public static implicit operator TReturnType?(ReturnValue<TReturnType> value)
    {
        return value.IsSuccessful() ? value.GetValue() : default;
    }

    public static implicit operator ObjectResult(ReturnValue<TReturnType> value)
    {
        var problemDetails = value.GetProblemDetails();
        return value.IsSuccessful() ? new ObjectResult(null)
        {
            StatusCode = 204 
        } : new ObjectResult(problemDetails?.Title ?? "Unknown Error")
        {
            StatusCode = problemDetails?.Status ?? 500
        };
    }
    
    public static implicit operator ReturnValue<TReturnType>(ProblemDetails problemDetails)
    {
        return new ReturnValue<TReturnType>(problemDetails);
    }
    
    public static implicit operator ReturnValue<TReturnType>(TReturnType problemDetails)
    {
        return new ReturnValue<TReturnType>(problemDetails);
    }
}