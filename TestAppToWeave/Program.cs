using System;
using CILWeaveProfiler.Attributes;

namespace TestAppToWeave
{

    /****************************************************************************************************
     * This is a test program that you can use to test CILWeaver program
     ****************************************************************************************************/

    [ProfilerClass(LoggingType = CILWeaveProfiler.LoggingTypes.All)]
    class Program
    {
        [ProfilerMethod(LoggingType = CILWeaveProfiler.LoggingTypes.None)]
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }

        [LoggingMethodOverride]
        static void LogIt(string methodName, string parameters, long milliseconds)
        {
            Console.WriteLine("LOGGING -> " + methodName + "(" + parameters + ") executed in: " + milliseconds);
        }
    }
}
