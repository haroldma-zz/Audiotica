using System;

namespace Audiotica.Core.Exceptions
{
    public class EchoException : Exception
    {
        public EchoException(string message)
            : base(message){}
    }
}
