using System;
using System.Collections.Generic;
using System.Text;

namespace SqsMessageHandle
{
    public class Events
    {
        public delegate void LogHandler(string message);
    }
}
