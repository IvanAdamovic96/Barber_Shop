namespace Hair.Application.Common.Exceptions;

public class LoginException : BaseException
{
    public int StatusCode { get; }
    public LoginException(string? message, int statusCode = 400) : base(message, statusCode)
    {
        StatusCode = statusCode;
    }
}