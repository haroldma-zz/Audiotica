using System;

namespace Audiotica.Core.Exceptions
{
    public class LastException : Exception
    {
        public LastException(string message, string description) : base(message)
        {
            Description = description;
        }
        public string Description { get; set; }
    }
}
