using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CILWeaveProfiler.Models
{
    /// <summary>
    /// A parsed representation of a Method from CIL Code
    /// </summary>
    public class Method
    {       
        public string MethodName { get; set; } = "";     
        
        /// <summary>
        /// The Method's Parameters
        /// </summary>
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();

        /// <summary>
        /// Labels used
        /// </summary>
        public List<string> Labels { get; set; } = new List<string>();

        /// <summary>
        /// Labels that are referenced (i.e. gotos)
        /// </summary>
        public List<string> Gotos { get; set; } = new List<string>();

        /// <summary>
        /// Items defined in the Init block of the Method
        /// </summary>
        public List<string> InitTypes { get; set; } = new List<string>();

        /// <summary>
        /// Lines of code in the method, including placeholder values
        /// </summary>
        public List<string> LinesOfCode { get; set; } = new List<string>();

        /// <summary>
        /// Is this method a static method?
        /// </summary>
        public bool IsStatic { get; set; }

        /// <summary>
        /// The maxstack value
        /// </summary>
        public int MaxStack { get; set; }

        /// <summary>
        /// Is this the method that will be used to handel logging in the .NET code?
        /// </summary>
        public bool IsLoggingMethodOverride { get; set; }

        /// <summary>
        /// The logging type (if any) for this method. Note, if null, the parent Class' logging type will be applied
        /// </summary>
        public LoggingTypes? LoggingType { get; set; }

        /// <summary>
        /// Does this Method contain at least one Parameter that implements IEnumerable?
        /// </summary>
        public bool ContainsEnumerableParameters
        {
            get
            {
                return Parameters.Where(x => x.IsEnumerable).FirstOrDefault() != null ? true : false;
            }
        }

        /// <summary>
        /// Returns the IL Method that has been modified for profiling
        /// </summary>
        /// <param name="classLoggingType">The parent class logging type, which is used if the method does not have one defined</param>
        /// <param name="maxStringLength">The maximum length of a string before truncation, specifiy 0 for no truncation</param>
        /// <returns></returns>
        public string GenerateMethodILCode(LoggingTypes? classLoggingType, int maxStringLength = 0)
        {
            StringBuilder IL = new StringBuilder();
            foreach(string line in LinesOfCode)
            {
                IL.AppendLine(line);
            }

            return ReplacePlaceholders(IL.ToString(), classLoggingType, maxStringLength);
        }

        /// <summary>
        /// Replaces the placeholder strings in the IL code with applicable IL code blocks
        /// </summary>
        /// <param name="IL"></param>        
        /// <param name="classLoggingType">The parent class logging type, which is used if the method does not have one defined</param>
        /// <param name="maxStringLength">The maximum length of a string before truncation, specifiy 0 for no truncation</param>
        /// <returns>IL Code</returns>
        private string ReplacePlaceholders(string IL, LoggingTypes? classLoggingType, int maxStringLength = 0)
        {
            // Be default we are going to use the Class' logging type...
            LoggingTypes? loggingType = classLoggingType;
            // Unless we have a logging type specified for this Method
            if (LoggingType != null) loggingType = LoggingType;
            
            IL = IL.Replace("***" + MethodName + "_LOCALS INIT***", GenerateBlock_LocalInit(loggingType));
            IL = IL.Replace("***" + MethodName + "_START***", GenerateBlock_ParameterLogging(loggingType, maxStringLength));
            IL = IL.Replace("***" + MethodName + "_END***", GenerateBlock_ExecutionLogging(loggingType));
            IL = IL.Replace("&&&maxstack&&&", (IsLoggingMethodOverride ? MaxStack : (Parameters.Count + 4)).ToString());

            return IL;
        }

        /// <summary>
        /// Generates the Parameter Logging IL code block
        /// </summary>
        /// <param name="maxStringLength">The max length of string values before truncation, 0 means no limit</param>
        /// <returns>IL code</returns>
        private string GenerateBlock_ParameterLogging(LoggingTypes? loggingType, int maxStringLength = 0)
        {
            StringBuilder sb = new StringBuilder();

            if (loggingType == null) loggingType = LoggingTypes.None;

            // Don't add any additional code if we are in the Method Override (because it wont be used)
            if (!IsLoggingMethodOverride)
            {
                if (LoggingType == LoggingTypes.All || LoggingType == LoggingTypes.ParameterValuesOnly)
                {
                    sb.AppendLine(GenerateUniqueLabel() + "ldc.i4.s   " + Parameters.Count * 2);
                    sb.AppendLine(GenerateUniqueLabel() + "newarr     [System.Runtime]System.String");
                    sb.AppendLine(GenerateUniqueLabel() + "dup\n");

                    int y = 0;
                    string cliType = "";

                    /* The ldc.i4. instruction can go from ldc.i4.0 to ldc.i4.8
                     * after that, if we need to push more items onto the stack we need
                     * to start using the ldc.i4.s  X instruction (where X is the number)
                     */

                    for (int x = 0; x < Parameters.Count; x++)
                    {
                        int y2 = y + 1;
                        sb.AppendLine(GenerateUniqueLabel() + "ldc.i4." + (y >= 9 ? "s   " + y : y.ToString()));
                        sb.AppendLine(GenerateUniqueLabel() + "ldstr      \"" + (x == 0 ? "" : "; ") + Parameters[x].Name + "=\"");
                        sb.AppendLine(GenerateUniqueLabel() + "stelem.ref");

                        cliType = ToCILType(Parameters[x].Type);
                        if (cliType != "IEnumerable")
                        {
                            sb.AppendLine(GenerateUniqueLabel() + "dup");

                            sb.AppendLine(GenerateUniqueLabel() + "ldc.i4." + (y2 >= 9 ? "s   " + y2.ToString() : y2.ToString()));

                            sb.AppendLine(GenerateUniqueLabel() + "ldarga.s   " + Parameters[x].Name);
                            sb.AppendLine(GenerateUniqueLabel() + "call       instance string " + cliType + "::ToString()");
                        }
                        else
                        {
                            sb.AppendLine(GenerateUniqueLabel() + "dup");
                            sb.AppendLine(GenerateUniqueLabel() + "ldc.i4." + (y2 >= 9 ? "s   " + y2.ToString() : y2.ToString()));
                            sb.AppendLine(GenerateUniqueLabel() + "ldarg." + x);
                            sb.AppendLine(GenerateUniqueLabel() + "ldc.i4.0");
                            sb.AppendLine(GenerateUniqueLabel() + "call       string @@@Assembly@@@.@@@Class@@@::@@@EnumerableMethod@@@" + (IsStatic ? "static" : "") + "(class [System.Runtime]System.Collections.IEnumerable,");
                            sb.AppendLine("                                                                                         bool)");
                        }

                        sb.AppendLine(GenerateUniqueLabel() + "stelem.ref");
                        sb.AppendLine(GenerateUniqueLabel() + "dup\n");

                        y += 2;
                    }
                }

                if (LoggingType == LoggingTypes.All || LoggingType == LoggingTypes.ExecutionTimeOnly)
                {
                    sb.AppendLine(GenerateUniqueLabel() + "call       class [System.Runtime.Extensions]System.Diagnostics.Stopwatch [System.Runtime.Extensions]System.Diagnostics.Stopwatch::StartNew()");
                    sb.AppendLine(GenerateUniqueLabel() + "stloc.0");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts a type keyword to a .NET Type name if not already a .NET Type name
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string ToCILType(string type)
        {
            string ret = type;

            if (!type.Contains("[System.Runtime]"))
            {
                ret = "[System.Runtime]System.";
                switch (type.ToLower())
                {
                    case "int32":
                        ret += "Int32";
                        break;
                    case "int16":
                        ret += "Int16";
                        break;
                    case "int64":
                        ret += "Int64";
                        break;
                    case "uint32":
                        ret += "UInt32";
                        break;
                    case "uint16":
                        ret += "UInt16";
                        break;
                    case "uint64":
                        ret += "UInt64";
                        break;
                    case "long":
                        ret += "Int64";
                        break;
                    case "ulong":
                        ret += "UInt64";
                        break;
                    case "short":
                        ret += "Int16";
                        break;
                    case "ushort":
                        ret += "UInt16";
                        break;
                    case "decimal":
                        ret += "Decimal";
                        break;
                    case "string":
                        ret += "Object";
                        break;
                    case "bool":
                        ret += "Boolean";
                        break;
                    case "float64":
                        ret += "Double";
                        break;
                    case "double":
                        ret += "Double";
                        break;
                    case "float32":
                        ret += "Single";
                        break;
                    case "object":
                        ret += "Object";
                        break;
                    case "byte":
                        ret += "Byte";
                        break;
                    case "sbyte":
                        ret += "SByte";
                        break;
                    case "char":
                        ret += "Char";
                        break;
                    default:
                        if (type.Contains("Collections") || type.Contains("[]")) 
                            ret = "IEnumerable";
                        else if (type.StartsWith("valuetype "))
                            ret = ret.Replace("valuetype ", "");
                        else
                            ret += "Object";
                        break;
                }
            }

            return ret;
        }

        /// <summary>
        /// Generates the IL Code to call the defined Method Override which is actually defined in our .NET code passing in (string methodName, string parameters, long milliseconds)
        /// </summary>
        /// <returns></returns>
        private string GenerateBlock_ExecutionLogging(LoggingTypes? loggingType)
        {
            StringBuilder sb = new StringBuilder();

            if (loggingType == null) loggingType = LoggingTypes.None;

            // We REALLY don't want to do this if this is the logging method override, as this will cause infinite recursion in the re-assembled app :-O
            if (!IsLoggingMethodOverride)
            {
                sb.AppendLine(GenerateUniqueLabel() + "stloc.s    V_" + (InitTypes.Count - 1));
                sb.AppendLine(GenerateUniqueLabel() + "ldloc.1");
                sb.AppendLine(GenerateUniqueLabel() + "callvirt instance void [System.Runtime.Extensions]System.Diagnostics.Stopwatch::Stop()");
                sb.AppendLine(GenerateUniqueLabel() + "nop");
                sb.AppendLine(GenerateUniqueLabel() + "ldstr      \"" + MethodName + "\"");
                sb.AppendLine(GenerateUniqueLabel() + "ldloc.0");
                sb.AppendLine(GenerateUniqueLabel() + "ldloc.1");
                sb.AppendLine(GenerateUniqueLabel() + "callvirt instance int64[System.Runtime.Extensions]System.Diagnostics.Stopwatch::get_ElapsedMilliseconds()");
                sb.AppendLine(GenerateUniqueLabel() + "call       void @@@Assembly@@@.@@@Class@@@::@@@MethodOverride@@@(string,");
                sb.AppendLine("                                                      string,");
                sb.AppendLine("                                                      int64)");
            }
            return sb.ToString();
        }
                

        /// <summary>
        /// Rebuilds the ".locals init" section of a method with the addition of a StopWatch
        /// </summary>
        /// <param name="initTypes"></param>
        /// <returns>IL code</returns>
        private string GenerateBlock_LocalInit(LoggingTypes? loggingType)
        {
            string ret = ".locals init (";

            if (loggingType == null) loggingType = LoggingTypes.None;

            int additionalInits = 0;
            if (!IsLoggingMethodOverride)
            {
                if (loggingType == LoggingTypes.All || loggingType == LoggingTypes.ParameterValuesOnly)
                {
                    ret += "string V_0,\n";
                    additionalInits++;
                }

                if (loggingType == LoggingTypes.All || loggingType == LoggingTypes.ExecutionTimeOnly)
                {
                    ret += "class [System.Runtime.Extensions]System.Diagnostics.Stopwatch V_1" + (InitTypes.Count > 0 ? "," : ")") + "\n";
                    additionalInits++;
                }

                for (int x = 0; x < InitTypes.Count; x++)
                {
                    ret += InitTypes[x] + " V_" + (x + additionalInits) + (x < InitTypes.Count - 1 ? "," : ")") + "\n";
                }
            }
            else
            {
                for (int x = 0; x < InitTypes.Count; x++)
                {
                    ret += InitTypes[x] + " V_" + (x) + (x < InitTypes.Count - 1 ? "," : ")") + "\n";
                }
            }

            return ret == ".locals init (" ? "" : ret;
        }

        /// <summary>
        /// Generate a new Label value that is unique to the existing list (and adds the new value to the list)
        /// </summary>
        /// <param name="includeColonAndPadding">Will also include a colon and standard padding after the label name</param>
        /// <returns></returns>
        private string GenerateUniqueLabel(bool includeColonAndPadding = true)
        {
            bool gotIt = false;
            string ret = "";
            int v = 0;

            while (!gotIt)
            {
                v++;
                ret = "IL_" + v.ToString().PadLeft(4, '0');

                if (Labels.Where(x => x == ret).FirstOrDefault() == null)
                    gotIt = true;
            }
            Labels.Add(ret);

            return (includeColonAndPadding ? "    " : "") + ret + (includeColonAndPadding ? ":  " : "");
        }
    }

}
