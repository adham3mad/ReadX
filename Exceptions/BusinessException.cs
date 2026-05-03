using System;

namespace ReadX.Api.Exceptions;

public class BusinessException : Exception
{
    public string ErrorCode { get; }
    public int StatusCode { get; }

    public BusinessException(string errorCode, int statusCode = 400) : base(errorCode)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}
