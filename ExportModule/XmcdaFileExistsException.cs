using System;

namespace ExportModule
{
    public class XmcdaFileExistsException : Exception
    {
        public XmcdaFileExistsException(string message) : base(message)
        {
        }
    }
}