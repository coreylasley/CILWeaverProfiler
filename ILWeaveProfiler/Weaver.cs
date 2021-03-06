﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using CILWeaveProfiler.Models;

namespace CILWeaveProfiler
{
    public enum LoggingTypes
    {
        All,
        ExecutionTimeOnly,
        ParameterValuesOnly,
        None
    }      

    public class Weaver
    {
        /// <summary>
        /// Closest paths to the ILASM and ILDASM executables (comma delimited)
        /// </summary>
        public string ilasmPaths { get; set; } = @"C:\Program Files (x86)\Microsoft SDKs\Windows\;C:\Windows\Microsoft.NET\Framework\v4.0.30319\";

        private string fileExtension;
        private const string LoggingMethodOverride = "[CILWeaveProfiler]CILWeaveProfiler.Attributes.LoggingMethodOverrideAttribute";
       
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
            ProcessStartInfo procStartInfo = new ProcessStartInfo(ilasm, toPath + " /" + fileExtension);
            procStartInfo.WorkingDirectory = workingPath;
            
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

            fileExtension = Path.GetExtension(executableFileName).Replace(".","");

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
        /// Parses the IL code for into an Assembly object
        /// </summary>
        /// <param name="IL">Disassembled IL code</param>
        /// <returns>Modified IL code with logging functionality</returns>
        public Assembly ParseILCode(string IL)
        {
            bool inClass = false;
            bool inMethodDef = false;
            bool inMethod = false;
            bool inMethodBody = false;
            bool inInit = false;
            int methodCount = 0;

            int nameEnd = 0;
            int lineNumber = -1;
            int classCount = 0;

            string l, label;
                        
            Assembly assembly = new Assembly();
            Class currentClass = new Class();
            Method currentMethod = new Method();
            Parameter paramItem;
            
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

                if (!inClass && l.StartsWith(".assembly") && !l.Contains(" extern "))
                {
                    assembly.AssemblyName = l.Split(' ').LastOrDefault();
                }

                // Did we hit the first line of a Class definition?
                if (!inClass && l.StartsWith(".class"))
                {                    
                    currentClass.ClassName = l.Split('.').LastOrDefault(); ;

                    assembly.LinesOfCode.Add("!!!" + currentClass.ClassName + "!!!");

                    inClass = true;
                }
                               
                // Did we hit the first line of a Method definition?
                if (inClass && l.StartsWith(".method ") && !l.Contains("specialname rtspecialname"))
                {   
                    // Find the end of the method's name
                    nameEnd = l.IndexOf("(");
                    if (nameEnd > 0)
                    {
                        // The start of the method name always seems to have 2 spaces in front of it
                        int nameStart = l.IndexOf("  ");
                        if (nameStart > 0 && nameStart < nameEnd)
                        {
                            if (l.Contains(" static ")) currentMethod.IsStatic = true;

                            // Extract the method name from the line
                            currentMethod.MethodName = l.Substring(nameStart + 2, nameEnd - (nameStart + 2));

                            // Add a placeholder for this method in the class lines of code
                            currentClass.LinesOfCode.Add("%%%" + currentMethod.MethodName + "%%%");

                            // We are now in the method and inside of it's definition
                            inMethod = true;
                            inMethodDef = true;

                            // The first parameter (if one exists) will be on this line as well
                            paramItem = ExtractParameter(l, nameEnd);
                            if (paramItem != null)
                            {
                                // Add it if we got one
                                currentMethod.Parameters.Add(paramItem);

                                // We dont want to check for a parameter on this line again (next section)
                                alreadyGotParameterOnThisLine = true;

                                // If the parameter we obtained had the characteristics of the last parameter in the definition
                                if (paramItem.IsLast)
                                    inMethodDef = false;
                            }
                        }
                    }                    
                } // is .method

                // If we are inside a Method definition
                if (inMethodDef && !alreadyGotParameterOnThisLine)
                {
                    // Extract the parameter from the line, if one exists
                    paramItem = ExtractParameter(l);
                    if (paramItem != null)
                    {
                        currentMethod.Parameters.Add(paramItem);

                        if (paramItem.IsLast)
                           inMethodDef = false;
                    }
                }

                // If we are inside of a method and have hit the {, we are now in the method body
                if (l == "{" && inMethod)
                {
                    inMethodBody = true;   
                }       
                
                if (inClass && currentClass.LoggingType == null)
                {
                    currentClass.LoggingType = GetLoggingType(l);
                }

                if (inMethod && currentMethod.LoggingType == null)
                {
                    currentMethod.LoggingType = GetLoggingType(l);
                }

                if (inMethod && l.Contains(LoggingMethodOverride))
                {
                    currentMethod.IsLoggingMethodOverride = true;
                }

                if (inMethodBody && l.Contains("// end of method"))
                {
                    currentMethod.LinesOfCode.Add(l);

                    // Add the current method to the list of methods in the current class
                    currentClass.Methods.Add(currentMethod);

                    methodCount++;
                    currentMethod = new Method();

                    inMethod = false;
                    inMethodBody = false;
                    includeLine = false;
                }
                
                // If we are inside the body of the method...
                if (inMethodBody)
                {
                    if (l.StartsWith(".maxstack"))
                    {
                        string[] ms = l.Split(' ');
                        currentMethod.MaxStack = Convert.ToInt32(ms[ms.Length - 1]);
                        includeLine = false;
                        currentMethod.LinesOfCode.Add("    .maxstack  &&&maxstack&&&");
                    }

                    if (l.Contains(".locals init"))
                    {
                        inInit = true;
                        includeLine = false;
                        currentMethod.LinesOfCode.Add("    ***" + currentMethod.MethodName + "_LOCALS INIT***");                        
                    }

                    if (inInit && l.Contains("V_"))
                    {
                        string i = GetInitType(l);
                        currentMethod.LocalsInitTypes.Add(i);
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
                        if (currentMethod.LocalsInitTypes.Count == 0)
                        {
                            currentMethod.LinesOfCode.Add("***" + currentMethod.MethodName + "_LOCALS INIT***");
                        }

                        // Insert this line
                        currentMethod.LinesOfCode.Add("    IL_0000:  nop");
                        
                        // Finally, insert the placeholder for the method start logging
                        currentMethod.LinesOfCode.Add("***" + currentMethod.MethodName + "_START***");
                        
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
                            currentMethod.LinesOfCode.Add("***" + currentMethod.MethodName + "_END***");                            
                        }
                    }
                    else if (l.Contains("IL_")) // If it doesnt start with a label, but contains one, we have found a GOTO
                    {
                        // Grab the label value
                        label = l.Substring(l.IndexOf("IL_"), 7);
                        // Add the label value to the List of Gotos
                        currentMethod.LabelsReferenced.Add(label);
                    }                    
                }

                if (!inMethod && inClass)
                {
                    if (l.Contains("// end of class"))
                    {
                        currentClass.LinesOfCode.Add(l);

                        // Add the current class to our class list
                        assembly.Classes.Add(currentClass);
                        classCount++;

                        currentClass = new Class();

                        inClass = false;
                        includeLine = false;
                    }
                }

                // Append the line to our running template (essentially the IL, with our placeholders)
                if (includeLine)
                {
                    if (inMethod)
                        currentMethod.LinesOfCode.Add(line);
                    else if (inClass)
                        currentClass.LinesOfCode.Add(line);
                    else
                        assembly.LinesOfCode.Add(line);                                        
                }
            }
            
            return assembly;
        }

        private string GetInitType(string line)
        {
            string ret = "";
            line = line.Replace(".locals init (", "").Trim();
            string[] parts = line.Split(' ');
            for (int x = 0; x < parts.Count() - 1; x++)
                ret += parts[x] + " ";

            return ret.Trim();
        }

        /// <summary>
        /// Gets the Logging Type defined in the Profiler Attribute
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private LoggingTypes? GetLoggingType(string line)
        {
            // Pretty cryptic :P

            LoggingTypes? ret = null;

            if (line.EndsWith("// ggingType...."))
            {
                if (line.Contains("67 67 69 6E 67 54 79 70 65 00 00 00 00 )")) ret = LoggingTypes.All;                
                if (line.Contains("67 67 69 6E 67 54 79 70 65 01 00 00 00 )")) ret = LoggingTypes.ExecutionTimeOnly;                
                if (line.Contains("67 67 69 6E 67 54 79 70 65 02 00 00 00 )")) ret = LoggingTypes.ParameterValuesOnly;
                if (line.Contains("67 67 69 6E 67 54 79 70 65 03 00 00 00 )")) ret = LoggingTypes.None;
            }

            return ret;
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
        /// Extracts a method parameter type from a line of IL code
        /// </summary>
        /// <param name="l"></param>
        /// <param name="nameEnd"></param>
        /// <returns>the type of the method parameter</returns>
        private Parameter ExtractParameter(string l, int nameEnd = -1)
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

}
