using System;

namespace Audiotica.Core.Exceptions
{
    public class AppException : Exception
    {
        public AppException(string message) : base(message)
        {
        }
    }

    public class NoMatchFoundException : AppException
    {
        public NoMatchFoundException() : base("NoMatchFound")
        {
        }
    }
}