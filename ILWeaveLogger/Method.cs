using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILWeaveLogger
{
    public class Method
    {
        public string MethodName { get; set; } = "";
        
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();
        public List<string> Labels { get; set; } = new List<string>();
        public List<string> Gotos { get; set; } = new List<string>();
        public List<string> InitTypes { get; set; } = new List<string>();
        public List<string> LinesOfCode { get; set; } = new List<string>();

        /// <summary>
        /// Replaces the placeholder strings in the IL code with applicable IL code blocks
        /// </summary>
        /// <param name="IL"></param>
        /// <param name="methods"></param>
        /// <returns>IL Code</returns>
        private string ReplacePlaceholders(string IL, List<Method> methods)
        {
            foreach (Method m in methods)
            {
                IL = IL.Replace("***" + m.MethodName + "_LOCALS INIT***", GenerateBlock_LocalInit(m.InitTypes));
                IL = IL.Replace("***" + m.MethodName + "_START***", GenerateBlock_ParameterLogging(m.MethodName, m.Parameters, m.Labels));
            }

            return IL;
        }

        /// <summary>
        /// Generates the Parameter Logging IL code block
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <param name="existingLabels"></param>
        /// <returns>IL code</returns>
        private string GenerateBlock_ParameterLogging(string methodName, List<Parameter> parameters, List<string> existingLabels)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(GenerateUniqueLabel(ref existingLabels) + "ldc.i4.s " + parameters.Count * 2);
            sb.AppendLine(GenerateUniqueLabel(ref existingLabels) + "newarr     [System.Runtime]System.String");
            sb.AppendLine(GenerateUniqueLabel(ref existingLabels) + "dup\n");

            int y = 0;
            for (int x = 0; x < parameters.Count; x++)
            {
                sb.AppendLine(GenerateUniqueLabel(ref existingLabels) + "ldc.i4." + y);
                sb.AppendLine(GenerateUniqueLabel(ref existingLabels) + "ldstr      \"" + (x == 0 ? "Logging -> " : "; ") + parameters[x].Name + "=\"");
                sb.AppendLine(GenerateUniqueLabel(ref existingLabels) + "dup");

                sb.AppendLine(GenerateUniqueLabel(ref existingLabels) + "ldc.i4." + (y + 1));
                sb.AppendLine(GenerateUniqueLabel(ref existingLabels) + "ldarga.s   " + parameters[x].Name);
                sb.AppendLine(GenerateUniqueLabel(ref existingLabels) + "call       instance string " + ToCILType(parameters[x].Type) + "::ToString()");
                sb.AppendLine(GenerateUniqueLabel(ref existingLabels) + "stelem.ref");
                sb.AppendLine(GenerateUniqueLabel(ref existingLabels) + "dup\n");

                y += 2;
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
                        if (type.StartsWith("valuetype "))
                            ret = ret.Replace("valuetype ", "");
                        else
                            ret += "Object";
                        break;
                }
            }

            return ret;
        }

        /// <summary>
        /// Rebuilds the ".locals init" section of a method with the addition of a StopWatch
        /// </summary>
        /// <param name="initTypes"></param>
        /// <returns>IL code</returns>
        private string GenerateBlock_LocalInit(List<string> initTypes)
        {
            string ret = ".locals init (";

            ret += "string V_0,\nclass [System.Runtime.Extensions]System.Diagnostics.Stopwatch V_1" + (initTypes.Count > 0 ? "," : ")") + "\n";

            for (int x = 0; x < initTypes.Count; x++)
            {
                ret += initTypes[x] + " V_" + (x + 2) + (x < initTypes.Count - 1 ? "," : ")") + "\n";
            }

            return ret;
        }

        /// <summary>
        /// Generate a new Label value that is unique to the existing list (and adds the new value to the list)
        /// </summary>
        /// <param name="existingLabels"></param>
        /// <returns></returns>
        private string GenerateUniqueLabel(ref List<string> existingLabels, bool includeColonAndPadding = true)
        {
            bool gotIt = false;
            string ret = "";
            int v = 0;

            while (!gotIt)
            {
                v++;
                ret = "IL_" + v.ToString().PadLeft(4, '0');

                if (existingLabels.Where(x => x == ret).FirstOrDefault() == null)
                    gotIt = true;
            }
            existingLabels.Add(ret);

            return (includeColonAndPadding ? "    " : "") + ret + (includeColonAndPadding ? ":  " : "");
        }
    }

}
