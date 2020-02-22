using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILWeaveProfiler.Models
{
    public class Method
    {
       
        public string MethodName { get; set; } = "";
        
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();
        public List<string> Labels { get; set; } = new List<string>();
        public List<string> Gotos { get; set; } = new List<string>();
        public List<string> InitTypes { get; set; } = new List<string>();
        public List<string> LinesOfCode { get; set; } = new List<string>();
        public int MaxStack { get; set; }
        public bool IsLoggingMethodOverride { get; set; }

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
        /// <returns></returns>
        public string GenerateMethodILCode(int maxStringLength = 0)
        {
            StringBuilder IL = new StringBuilder();
            foreach(string line in LinesOfCode)
            {
                IL.AppendLine(line);
            }

            return ReplacePlaceholders(IL.ToString(), maxStringLength);
        }

        /// <summary>
        /// Replaces the placeholder strings in the IL code with applicable IL code blocks
        /// </summary>
        /// <param name="IL"></param>
        /// <param name="methods"></param>
        /// <returns>IL Code</returns>
        private string ReplacePlaceholders(string IL, int maxStringLength = 0)
        {
            IL = IL.Replace("***" + MethodName + "_LOCALS INIT***", GenerateBlock_LocalInit());
            IL = IL.Replace("***" + MethodName + "_START***", GenerateBlock_ParameterLogging(maxStringLength));
            IL = IL.Replace("***" + MethodName + "_END***", GenerateBlock_ExecutionLogging());
            IL = IL.Replace("&&&maxstack&&&", (IsLoggingMethodOverride ? MaxStack : (Parameters.Count + 4)).ToString());

            return IL;
        }

        /// <summary>
        /// Generates the Parameter Logging IL code block
        /// </summary>
        /// <param name="maxStringLength">The max length of string values before truncation, 0 means no limit</param>
        /// <returns>IL code</returns>
        private string GenerateBlock_ParameterLogging(int maxStringLength = 0)
        {
            StringBuilder sb = new StringBuilder();

            if (!IsLoggingMethodOverride)
            {
                sb.AppendLine(GenerateUniqueLabel() + "ldc.i4.s " + Parameters.Count * 2);
                sb.AppendLine(GenerateUniqueLabel() + "newarr     [System.Runtime]System.String");
                sb.AppendLine(GenerateUniqueLabel() + "dup\n");

                int y = 0;
                string cliType = "";
                for (int x = 0; x < Parameters.Count; x++)
                {
                    sb.AppendLine(GenerateUniqueLabel() + "ldc.i4." + y);
                    sb.AppendLine(GenerateUniqueLabel() + "ldstr      \"" + (x == 0 ? "Logging -> " : "; ") + Parameters[x].Name + "=\"");
                    sb.AppendLine(GenerateUniqueLabel() + "dup");

                    sb.AppendLine(GenerateUniqueLabel() + "ldc.i4." + (y + 1));
                    sb.AppendLine(GenerateUniqueLabel() + "ldarga.s   " + Parameters[x].Name);

                    cliType = ToCILType(Parameters[x].Type);
                    if (cliType != "IEnumerable")
                        sb.AppendLine(GenerateUniqueLabel() + "call       instance string " + cliType + "::ToString()");
                    else
                        sb.AppendLine(GenerateUniqueLabel() + "******* FIGURE OUT HOW TO CONVERT AN IENUMERABLE ************** ");

                    sb.AppendLine(GenerateUniqueLabel() + "stelem.ref");
                    sb.AppendLine(GenerateUniqueLabel() + "dup\n");

                    y += 2;
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
                        if (type.Contains(""))
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

        private string GenerateBlock_ExecutionLogging()
        {
            StringBuilder sb = new StringBuilder();
            
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
        private string GenerateBlock_LocalInit()
        {
            string ret = ".locals init (";

            if (!IsLoggingMethodOverride)
            {
                ret += "string V_0,\nclass [System.Runtime.Extensions]System.Diagnostics.Stopwatch V_1" + (InitTypes.Count > 0 ? "," : ")") + "\n";

                for (int x = 0; x < InitTypes.Count; x++)
                {
                    ret += InitTypes[x] + " V_" + (x + 2) + (x < InitTypes.Count - 1 ? "," : ")") + "\n";
                }
            }
            else
            {
                for (int x = 0; x < InitTypes.Count; x++)
                {
                    ret += InitTypes[x] + " V_" + (x) + (x < InitTypes.Count - 1 ? "," : ")") + "\n";
                }
            }

            return ret;
        }

        /// <summary>
        /// Generate a new Label value that is unique to the existing list (and adds the new value to the list)
        /// </summary>
        /// <param name="existingLabels"></param>
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
