using System;

namespace Audiotica.Web.Exceptions
{
    public class ProviderException : Exception
    {
        public ProviderException()
        {
        }

        public ProviderException(string message) : base(message)
        {
        }
    }

    public class ProviderNotFoundException : Exception
    {
    }
}