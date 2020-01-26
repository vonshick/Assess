using System;

namespace ImportModule
{
    public class ImproperFileStructureException : Exception
    {
        public ImproperFileStructureException(string message) : base(message)
        {
        }
    }
}