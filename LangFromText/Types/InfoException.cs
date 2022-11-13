using System;
using System.Collections.Generic;
using System.Text;

namespace Jpinsoft.LangTainer.Types
{
    public class InfoException : Exception
    {
        public InfoException() : base() { }

        public InfoException(string message) : base(message) { }

        public InfoException(string message, Exception innerException) : base(message, innerException) { }
    }
}
