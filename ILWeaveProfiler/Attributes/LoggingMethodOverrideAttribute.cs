using System;
using System.Collections.Generic;
using System.Text;

namespace ILWeaveProfiler.Attributes
{
    /// <summary>
    /// Use this attribute on a method with parameters: string methodName, string parameters, long milliseconds
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class LoggingMethodOverrideAttribute : Attribute
    {
    }
}
