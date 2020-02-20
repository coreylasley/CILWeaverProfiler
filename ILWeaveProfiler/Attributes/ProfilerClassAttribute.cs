using System;
using System.Collections.Generic;
using System.Text;

namespace ILWeaveProfiler.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ProfilerClassAttribute : Attribute
    {
        public ILWeaveLoggingTypes LoggingType { get; set; } = ILWeaveLoggingTypes.All;
    }
}
