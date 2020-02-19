using System;
using System.Collections.Generic;

namespace ILWeaveLogger
{
    class Program
    {
        static void Main(string[] args)
        {           
            Weaver w = new Weaver();

            // Disassemble the ReferenceApp.dll
            string IL = w.Disassemble(@"C:\git\PoorMansLogger.ILWeaver\ReferenceApp\bin\Debug\netcoreapp3.1\ReferenceApp.dll");
                        
            // Parse the disassembled IL Code
            Assembly asm = w.ParseILCode(IL);

            // Display the modified IL Code
            Console.WriteLine(asm.GenerateAssemblyILCode());                        
        }
    }
}
