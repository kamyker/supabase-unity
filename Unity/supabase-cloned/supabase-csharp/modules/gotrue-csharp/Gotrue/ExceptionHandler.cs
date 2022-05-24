using System;
using System.Diagnostics;
using System.Net.Http;

namespace Supabase.Gotrue
{
    /// <summary>
    /// Internal class for parsing Supabase specific exceptions.
    /// </summary>
    internal static class ExceptionHandler
    {
        internal static Exception Parse(RequestException ex)
        {
            switch (ex.Response.StatusCode)
            {
                case System.Net.HttpStatusCode.Unauthorized:
                    Debug.WriteLine(ex.Message);
                    return new UnauthorizedException(ex);
                case System.Net.HttpStatusCode.BadRequest:
                    Debug.WriteLine(ex.Message);
                    return new BadRequestException(ex);
                case System.Net.HttpStatusCode.Forbidden:
                    Debug.WriteLine("Forbidden, are sign-ups disabled?");
                    return new ForbiddenException(ex);
            }
            return ex;
        }
    }

    public class UnauthorizedException : Exception
    {
        public HttpResponseMessage Response { get; private set; }

        public string Content { get; private set; }
        public UnauthorizedException(RequestException exception)
        {
            Response = exception.Response;
            Content = exception.Error.Message;
        }
    }

    public class BadRequestException : Exception
    {
        public HttpResponseMessage Response { get; private set; }

        public string Content { get; private set; }
        public BadRequestException(RequestException exception)
        {
            Response = exception.Response;
            Content = exception.Error.Message;
        }
    }

    public class ForbiddenException : Exception
    {
        public HttpResponseMessage Response { get; private set; }
        public string Content { get; private set; }
        public ForbiddenException(RequestException exception)
        {
            Response = exception.Response;
            Content = exception.Error.Message;
        }
    }

    public class InvalidEmailOrPasswordException : Exception
    {
        public HttpResponseMessage Response { get; private set; }
        public string Content { get; private set; }
        public InvalidEmailOrPasswordException(RequestException exception)
        {
            Response = exception.Response;
            Content = exception.Error.Message;
        }
    }

    public class ExistingUserException : Exception
    {
        public HttpResponseMessage Response { get; private set; }
        public string Content { get; private set; }
        public ExistingUserException(RequestException exception)
        {
            Response = exception.Response;
            Content = exception.Error.Message;
        }
    }
}
