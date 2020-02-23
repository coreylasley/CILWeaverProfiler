using System;
using System.Collections.Generic;
using System.Text;

namespace CILWeaveProfiler.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ProfilerClassAttribute : Attribute
    {
        public LoggingTypes LoggingType { get; set; } = LoggingTypes.All;
    }
}
