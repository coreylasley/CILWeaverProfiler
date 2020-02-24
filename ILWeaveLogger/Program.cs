using System;
using System.Collections.Generic;
using System.IO;
using CILWeaveProfiler;
using CILWeaveProfiler.Models;

namespace ILWeaver
{
    class Program
    {
        static void Main(string[] args)
        {           
            Weaver w = new Weaver();

            string IL = "";
#if DEBUG
            // Disassemble the ReferenceApp.dll -- Change the path to the actual location of the file...
            IL = w.Disassemble(@"C:\git\PoorMansLogger.ILWeaver\ReferenceApp\bin\Debug\netcoreapp3.1\ReferenceApp.dll");
#else
            IL = w.Disassemble(args[0]);
#endif

            // Parse the disassembled IL Code
            Assembly asm = w.ParseILCode(IL);

            Console.WriteLine(IL);

            Console.WriteLine("\r\r**************************************************************************\r\r");

            IL = asm.GenerateAssemblyILCode();

            // Display the modified IL Code
            Console.WriteLine(IL);

            
            w.Assemble(IL, @"C:\git\PoorMansLogger.ILWeaver\ReferenceApp\bin\Debug\netcoreapp3.1\ReferenceApp2.dll");
        }


        private static string AssemblyDirectory
        {
            get
            {
                string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
