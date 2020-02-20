using System;
using System.Collections.Generic;
using System.Text;
using ILWeaveProfiler;

namespace ILWeaveProfiler.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ProfilerMethodAttribute : Attribute
    {
        public ILWeaveLoggingTypes LoggingType { get; set; } = ILWeaveLoggingTypes.All;
    }
}
