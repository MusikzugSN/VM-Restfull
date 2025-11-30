#nullable enable
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
    
    public static implicit operator ReturnValue<TReturnType>(ProblemDetails problemDetails)
    {
        return new ReturnValue<TReturnType>(problemDetails);
    }
    
    public static implicit operator ReturnValue<TReturnType>(TReturnType problemDetails)
    {
        return new ReturnValue<TReturnType>(problemDetails);
    }
}