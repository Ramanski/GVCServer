using System;

namespace ModelsLibrary
{
    public class RailProcessException : Exception
    {
        public RailProcessException() : base()
        {
        }
        public RailProcessException(string message) : base(message)
        {
        }

        public RailProcessException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
