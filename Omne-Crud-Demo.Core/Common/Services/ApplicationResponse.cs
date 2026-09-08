using System;
using System.Collections.Generic;
using System.Text;

namespace Omne_Crud_Demo.Core.Common.Services;

public class ApplicationResponse
{
    public bool Success { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static ApplicationResponse Ok()
        => new()
        {
            Success = true
        };

    public static ApplicationResponse Failure(
        string errorCode,
        string errorMessage)
        => new()
        {
            Success = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage
        };
}

public class ApplicationResponse<T> : ApplicationResponse
{
    public T? Data { get; init; }

    public static ApplicationResponse<T> Ok(T data)
        => new()
        {
            Success = true,
            Data = data
        };

    public static new ApplicationResponse<T> Failure(
        string errorCode,
        string errorMessage)
        => new()
        {
            Success = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage
        };
}
