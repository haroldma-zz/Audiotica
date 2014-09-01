using System;

namespace Audiotica.Core.Exceptions
{
    public class XboxException : Exception
    {
        public XboxException(string message, string description) : base(message)
        {
            Description = description;
        }
        public string Description { get; set; }
    }
}
