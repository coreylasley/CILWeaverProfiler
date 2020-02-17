using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ILWeaveLogger
{
    public class Weaver
    {
        public string ilasmPaths { get; set; } = @"C:\Program Files (x86)\Microsoft SDKs\Windows\;C:\Windows\Microsoft.NET\Framework\v4.0.30319\"; // "v10.0A\bin\NETFX 4.8 Tools\";

        /// <summary>
        /// Assembles IL code into an executable
        /// </summary>
        /// <param name="IL"></param>
        /// <param name="newExecutableFileName"></param>
        /// <returns>bool representing assembly success</returns>
        public bool Assemble(string IL, string newExecutableFileName, string temporaryPath = @"C:\Temp\")
        {
            bool ret = true;

            IL = IL.Replace("***", "//");

            if (File.Exists(newExecutableFileName))
            {               
                File.Delete(newExecutableFileName);
            }

            string toPath = temporaryPath + "temp.il";

            File.WriteAllText(toPath, IL);
                       
            string ilasm = GetLatestILExe(ilasmPaths, "ilasm.exe");
            string workingPath = Path.GetDirectoryName(ilasm);
            ProcessStartInfo procStartInfo = new ProcessStartInfo(ilasm, toPath + " /dll");
            procStartInfo.WorkingDirectory = workingPath;
            //procStartInfo.CreateNoWindow = false;

            Process process = new Process();
            process.StartInfo = procStartInfo;

            process.Start();
            process.WaitForExit();

            if (File.Exists(toPath))
            {                
                File.Delete(toPath);
            }

            if (!File.Exists(newExecutableFileName))
            {
                ret = false;
            }

            return ret;
        }

        /// <summary>
        /// Disassembles the executable to IL code
        /// </summary>
        /// <param name="executableFileName"></param>
        /// <param name="temporaryPath"></param>
        /// <returns>The executable's disassembled IL code</returns>
        public string Disassemble(string executableFileName, string temporaryPath = @"C:\Temp\")
        {
            string text = "";
            string toPath = temporaryPath + "temp.il";
            ProcessStartInfo procStartInfo = new ProcessStartInfo(GetLatestILExe(ilasmPaths, "ildasm.exe"), executableFileName + " /output:" + toPath);
            procStartInfo.CreateNoWindow = false;

            Process process = new Process();
            process.StartInfo = procStartInfo;
            
            process.Start();
            process.WaitForExit();

            if (File.Exists(toPath))
            {
                text = File.ReadAllText(toPath);
                File.Delete(toPath);
            }

            return text;
        }

        /// <summary>
        /// Modifies the IL code for method time execution and parameter logging
        /// </summary>
        /// <param name="IL">Disassembled IL code</param>
        /// <returns>Modified IL code with logging functionality</returns>
        public string ModifyILForLogging(string IL)
        {            
            bool inMethodDef = false;
            bool inMethod = false;
            bool inMethodBody = false;
            bool inInit = false;

            int nameEnd = 0;

            int lineNumber = -1;
                        
            string l, label;

            Parameter paramItem;

            List<Method> methods = new List<Method>();
            Method currentMethod = new Method();
            int methodCount = 0;

            StringBuilder template = new StringBuilder();

            bool includeLine = true;
            bool alreadyGotParameterOnThisLine = false;

            // Split the IL code into a List of lines
            List<string> lines = IL.Split('\n').ToList();

            // Loop through each line of IL code
            foreach (string line in lines)
            {
                alreadyGotParameterOnThisLine = false;
                includeLine = true;
                lineNumber++;

                l = line.Trim();
             
                // Did we hit the first line of a Method definition?
                if (l.StartsWith(".method "))
                {
                    // Add the last method to the list if there was one
                    if (methodCount > 0) methods.Add(currentMethod);

                    methodCount++;
                    currentMethod = new Method();
                    
                    nameEnd = l.IndexOf("(");
                    if (nameEnd > 0)
                    {
                        int nameStart = l.IndexOf("  ");
                        if (nameStart > 0 && nameStart < nameEnd)
                        {
                            currentMethod.MethodName = l.Substring(nameStart + 2, nameEnd - (nameStart + 2));

                            inMethod = true;
                            inMethodDef = true;

                            paramItem = GetParameter(l, nameEnd);
                            if (paramItem != null)
                            {
                                currentMethod.Parameters.Add(paramItem);
                                alreadyGotParameterOnThisLine = true;

                                if (paramItem.IsLast)
                                    inMethodDef = false;
                            }
                        }
                    }                    
                } // is .method

                // If we are inside a Method definition
                if (inMethodDef && !alreadyGotParameterOnThisLine)
                {
                    paramItem = GetParameter(l);
                    if (paramItem != null)
                    {
                        currentMethod.Parameters.Add(paramItem);

                        if (paramItem.IsLast)
                           inMethodDef = false;
                    }
                }

                if (l == "{" && inMethod)
                {
                    inMethodBody = true;   
                }                

                if (l.Contains("// end of method"))
                {
                    inMethod = false;
                    inMethodBody = false;                    
                }
                
                // If we are inside the body of the method...
                if (inMethodBody)
                {
                    if (l.Contains(".locals init"))
                    {
                        inInit = true;
                        includeLine = false;
                        template.AppendLine("    ***" + currentMethod.MethodName + "_LOCALS INIT***");
                    }

                    if (inInit && l.Contains("V_"))
                    {
                        string[] initLineParts = l.Split(' ');
                        if (initLineParts.Length >= 2)
                        {
                            currentMethod.InitTypes.Add(initLineParts[initLineParts.Length - 2].Replace("(",""));
                        }

                        includeLine = false;
                    }

                    if (inInit && l.EndsWith(")"))
                    {
                        inInit = false;
                        includeLine = false;
                    }                    

                    // This is the line where we will want to insert our start method logging code
                    if (l == "IL_0000:  nop")
                    {
                        // If an ".locals init" section was not found before this, lets insert a placeholder for it
                        if (currentMethod.InitTypes.Count == 0)
                            template.AppendLine("***" + currentMethod.MethodName + "_LOCALS INIT***");

                        // Insert this line
                        template.AppendLine("    IL_0000:  nop");

                        // Finally, insert the placeholder for the method start logging
                        template.AppendLine("***" + currentMethod.MethodName + "_START***");

                        // Since we already inserted the line from the IL above, we dont want to add it again at the end of the loop
                        includeLine = false;
                    }

                    // If the line starts with a label...
                    if (l.StartsWith("IL_"))
                    {
                        // Grab the label value
                        label = l.Substring(l.IndexOf("IL_"), 7);

                        // Add it to our List of Label values for this method
                        currentMethod.Labels.Add(label);

                        // Is this line where the method returns?
                        if (l == label + ":  ret")
                        {
                            template.AppendLine("***" + currentMethod.MethodName + "_END***");
                        }
                    }
                    else if (l.Contains("IL_")) // If it doesnt start with a label, but contains one, we have found a GOTO
                    {
                        // Grab the label value
                        label = l.Substring(l.IndexOf("IL_"), 7);
                        // Add the label value to the List of Gotos
                        currentMethod.Gotos.Add(label);
                    }                    
                }

                // Append the line to our running template (essentially the IL, with our placeholders)
                if (includeLine)
                    template.AppendLine(line);
            }

            return ReplacePlaceholders(template.ToString(), methods);
        }


        /// <summary>
        /// Replaces the placeholder strings in the IL code with applicable IL code blocks
        /// </summary>
        /// <param name="IL"></param>
        /// <param name="methods"></param>
        /// <returns>IL Code</returns>
        private string ReplacePlaceholders(string IL, List<Method> methods)
        {
            foreach(Method m in methods)
            {
                IL = IL.Replace("***" + m.MethodName + "_LOCALS INIT***", GetLocalInit(m.InitTypes));
                IL = IL.Replace("***" + m.MethodName + "_START***", GetParameterLoggingCodeBlock(m.MethodName, m.Parameters, m.Labels));
            }

            return IL;
        }

        /// <summary>
        /// Locates the latest version of an executable in all sub-directories of a root path if the executable is not found in the path specified
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="exeToFind"></param>
        /// <returns></returns>
        private string GetLatestILExe(string rootPaths, string exeToFind)
        {
            string latestPath = "";

            string[] paths = rootPaths.Split(';');

            if (File.Exists(paths[0] + exeToFind))
            {
                latestPath = paths[0] + exeToFind;
            }
            else
            {
                DateTime latestDate = DateTime.MinValue;

                foreach (string p in paths)
                {
                    try
                    {
                        string[] files = Directory.GetFiles(p, exeToFind, SearchOption.AllDirectories);

                        foreach (string file in files)
                        {
                            FileInfo fi = new FileInfo(file);
                            if (fi.CreationTime > latestDate)
                            {
                                latestDate = fi.CreationTime;
                                latestPath = file;
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine("ERROR: " + ex);
                    }
                }
            }

            return latestPath;
        }

        /// <summary>
        /// Generates the Parameter Logging IL code block
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <param name="existingLabels"></param>
        /// <returns>IL code</returns>
        private string GetParameterLoggingCodeBlock(string methodName, List<Parameter> parameters, List<string> existingLabels)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(GetUniqueLabel(ref existingLabels) + "ldc.i4.s " + parameters.Count * 2);
            sb.AppendLine(GetUniqueLabel(ref existingLabels) + "newarr     [System.Runtime]System.String");
            sb.AppendLine(GetUniqueLabel(ref existingLabels) + "dup\n");

            int y = 0;
            for (int x = 0; x < parameters.Count; x++)
            {
                sb.AppendLine(GetUniqueLabel(ref existingLabels) + "ldc.i4." + y);
                sb.AppendLine(GetUniqueLabel(ref existingLabels) + "ldstr      \"" + (x == 0 ? "Logging -> " : "; ") + parameters[x].Name + "=\"");
                sb.AppendLine(GetUniqueLabel(ref existingLabels) + "dup");

                sb.AppendLine(GetUniqueLabel(ref existingLabels) + "ldc.i4." + (y+1));
                sb.AppendLine(GetUniqueLabel(ref existingLabels) + "ldarga.s   " + parameters[x].Name);
                sb.AppendLine(GetUniqueLabel(ref existingLabels) + "call       instance string " + ToCILType(parameters[x].Type) + "::ToString()");
                sb.AppendLine(GetUniqueLabel(ref existingLabels) + "stelem.ref");
                sb.AppendLine(GetUniqueLabel(ref existingLabels) + "dup\n");

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
        private string GetLocalInit(List<string> initTypes)
        {
            string ret = ".locals init (";

            ret += "string V_0,\nclass [System.Runtime.Extensions]System.Diagnostics.Stopwatch V_1" + (initTypes.Count > 0 ? "," : ")") + "\n";

            for (int x=0;x<initTypes.Count; x++)
            {
                ret += initTypes[x] + " V_" + (x+2) + (x < initTypes.Count - 1 ? "," : ")") + "\n";
            }

            return ret;
        }

        /// <summary>
        /// Generate a new Label value that is unique to the existing list (and adds the new value to the list)
        /// </summary>
        /// <param name="existingLabels"></param>
        /// <returns></returns>
        private string GetUniqueLabel(ref List<string> existingLabels, bool includeColonAndPadding = true)
        {
            bool gotIt = false;
            string ret = "";
            int v = 0;

            while(!gotIt)
            {
                v++;
                ret = "IL_" + v.ToString().PadLeft(4, '0');

                if (existingLabels.Where(x => x == ret).FirstOrDefault() == null)
                    gotIt = true;
            }
            existingLabels.Add(ret);

            return (includeColonAndPadding ? "    " : "") + ret + (includeColonAndPadding ? ":  " : "");
        }

        /// <summary>
        /// Extracts a method parameter type from a line of IL code
        /// </summary>
        /// <param name="l"></param>
        /// <param name="nameEnd"></param>
        /// <returns>the type of the method parameter</returns>
        private Parameter GetParameter(string l, int nameEnd = -1)
        {
            Parameter ret = new Parameter();

            // If we are NOT looking at the last parameter
            if (l.IndexOf("cil managed") < 0)
            {
                string[] paramParts = l.Substring(nameEnd + 1, l.Length - (nameEnd + 1) - 1).Trim().Split(' ');
                               
                ret.Name = paramParts[paramParts.Length - 1];
                for (int x = 0; x < paramParts.Length - 1; x++)
                {
                    if (ret.Type != "") ret.Type += " ";
                    ret.Type += paramParts[x].Trim().Replace("(","");
                }
            }
            else
            {
                int methodDefEnd = l.IndexOf(")");
                if (methodDefEnd > nameEnd)
                {
                    if (nameEnd < 0) nameEnd = 0;
                    string[] paramParts = l.Substring(nameEnd, methodDefEnd - nameEnd).Split(' ');
                    if (paramParts.Length > 1)
                    {
                        ret.IsLast = true;
                        ret.Name = paramParts[paramParts.Length - 1];
                        for (int x = 0; x < paramParts.Length - 1; x++)
                        {
                            if (ret.Type != "") ret.Type += " ";
                            ret.Type += paramParts[x].Trim().Replace("(", "");
                        }
                    }
                }
            }

            return ret.Name != "" ? ret : null;
        }



    }

    public class Method
    {
        public string MethodName { get; set; } = "";
        
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();
        public List<string> Labels { get; set; } = new List<string>();
        public List<string> Gotos { get; set; } = new List<string>();
        public List<string> InitTypes { get; set; } = new List<string>();
    }

    public class Parameter
    {
        public string Type { get; set; } = "";
        public string Name { get; set; } = "";
        public bool IsLast { get; set; }
    }

}
