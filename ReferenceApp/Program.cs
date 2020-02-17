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

            //Logging(234, "This is a test", false, 234, 56456, 12312, 1, 2, DateTime.Now);
        }

        static void NoLogging(int intObject, string stringObject, DateTime dateTimeObject)
        {
            string a = "Something to do (" + intObject + ")";
            string b = " to see if this works " + stringObject;
            string c = a + b;
        }

        /*

        static void Logging(int intObject, string stringObject, bool boolObject, float floatObject, long longObject, double doubleObject, short shortObject, uint uintObject,  DateTime dateTimeObject)
        {
            File.AppendAllText("Logging -> " + "intObject=" + intObject.ToString()
                + "; stringObject=" + stringObject.ToString()
                + "; dateTimeObject=" + dateTimeObject.ToString()
                + "; boolObject=" + boolObject.ToString()
                + "; floatObject=" + floatObject.ToString()
                + "; longObject=" + longObject.ToString()
                + "; doubleObject=" + doubleObject.ToString()
                + "; shortObject=" + shortObject.ToString()
                + "; uintObject=" + uintObject.ToString()
                , @"c:\Temp\Does\Test.log");
            ;
            Stopwatch stopWatch = Stopwatch.StartNew();

            string a = "Something to do (" + intObject + ")";
            string b = " to see if this works " + stringObject;
            string c = a + b;

            stopWatch.Stop();

            File.AppendAllText("Logging -> " + stopWatch.ElapsedMilliseconds, @"c:\Temp\Does\Test.log");
        }
        */
    }
}
