using System;
using System.Diagnostics;
using System.IO;

namespace ReferenceApp
{
    class Program
    {
        static void Main(string[] args)
        {
            NoLogging(234, "This is a test", DateTime.Now);

            Logging(234, "This is a test", DateTime.Now);
        }

        static void NoLogging(int intObject, string stringObject, DateTime dateTimeObject)
        {
            string a = "Something to do (" + intObject + ")";
            string b = " to see if this works " + stringObject;
            string c = a + b;
        }

        static void Logging(int intObject, string stringObject, DateTime dateTimeObject)
        {
            File.AppendAllText("Logging -> " + "intObject=" + intObject.ToString() + "; stringObject=" + stringObject.ToString() + "; dateTimeObject=" + dateTimeObject.ToString() , @"c:\Temp\Does\Test.log");
            Stopwatch stopWatch = Stopwatch.StartNew();

            string a = "Something to do (" + intObject + ")";
            string b = " to see if this works " + stringObject;
            string c = a + b;

            stopWatch.Stop();

            File.AppendAllText("Logging -> " + stopWatch.ElapsedMilliseconds, @"c:\Temp\Does\Test.log");
        }
    }
}
