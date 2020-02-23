using System;
using System.Collections.Generic;
using System.Text;
using CILWeaveProfiler;

namespace CILWeaveProfiler.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ProfilerMethodAttribute : Attribute
    {
        public ILWeaveLoggingTypes LoggingType { get; set; } = ILWeaveLoggingTypes.All;
    }
}
