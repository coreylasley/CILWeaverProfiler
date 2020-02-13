using System;

namespace ILWeaveLogger
{
    class Program
    {
        static void Main(string[] args)
        {            
            Weaver w = new Weaver();

            string IL = w.Disassemble(@"C:\Users\Corey\source\repos\ILWeaveLogger\ReferenceApp\bin\Debug\netcoreapp3.1\ReferenceApp.dll");
                        
            Console.WriteLine(w.ModifyILForLogging(IL));
        }
    }
}
