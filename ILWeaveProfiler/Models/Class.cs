using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILWeaveProfiler.Models
{
    public class Class
    {
        public string ClassName { get; set; }
        public List<Method> Methods { get; set; } = new List<Method>();
        public List<string> LinesOfCode { get; set; } = new List<string>();
        public bool InheritsBase { get; set; }
        public bool ContainsMethodsWithEnumerableParameters
        {
            get
            {
                return Methods.Where(x => x.ContainsEnumerableParameters).FirstOrDefault() != null ? true : false;
            }
        }

        /// <summary>
        /// Returns the Class that has been modified for profiling
        /// </summary>
        /// <returns></returns>
        public string GenerateClassILCode(int maxStringLength = 0, int maxEnumerableCount = 0)
        {
            StringBuilder IL = new StringBuilder();
            for (int x = 0; x < LinesOfCode.Count; x++)
            {
                // If the next line is the end of the Class code, and we have Methods that have IEnumerable parameters...
                if (x + 1 == LinesOfCode.Count && ContainsMethodsWithEnumerableParameters)
                {
                    // Generate and append the Generic Enumberable to String Method to the IL Code
                    IL.Append(GenerateBlock_EnumerableToString(maxStringLength, maxEnumerableCount) + "\r");
                }

                IL.AppendLine(LinesOfCode[x]);
            }

            return ReplacePlaceholders(IL.ToString());
        }

        /// <summary>
        /// Replaces the placeholder strings in the IL code with applicable IL code blocks
        /// </summary>
        /// <param name="IL"></param>
        /// <param name="methods"></param>
        /// <returns>IL Code</returns>
        private string ReplacePlaceholders(string IL, int maxStringLength = 0)
        {
            // Loop through each Method in our Class
            foreach (Method m in Methods)
            {
                // Replace the Method's code block with the placeholder in the Class code
                IL = IL.Replace("%%%" + m.MethodName + "%%%", m.GenerateMethodILCode(maxStringLength).Replace("@@@Class@@@", ClassName));
            }

            // Determine if we have a Method Override (used to handle the actual logging)
            Method methodOverride = Methods.Where(x => x.IsLoggingMethodOverride).FirstOrDefault();
            if (methodOverride != null)
            {                
                IL = IL.Replace("@@@MethodOverride@@@", methodOverride.MethodName);
            }
            
            return IL;
        }
        
        private string GenerateBlock_EnumerableToString(int maxStringLength = 0, int maxEnumerableCount = 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("  .method private hidebysig instance string");
            sb.AppendLine("          Get___Enumerable___AsListString___<T>(class [System.Runtime]System.Collections.Generic.IEnumerable`1<!!T> enumerable,");
            sb.AppendLine("                                                bool isNumeric) cil managed");
            sb.AppendLine("  {");
            sb.AppendLine("    // Code size       276 (0x114)");
            sb.AppendLine("    .maxstack  4");
            sb.AppendLine("    .locals init (class [System.Runtime]System.Text.StringBuilder V_0,");
            sb.AppendLine("             int32 V_1,");
            sb.AppendLine("             int32 V_2,");
            sb.AppendLine("             int32 V_3,");
            sb.AppendLine("             string V_4,");
            sb.AppendLine("             class [System.Runtime]System.Collections.Generic.IEnumerator`1<!!T> V_5,");
            sb.AppendLine("             !!T V_6,");
            sb.AppendLine("             bool V_7,");
            sb.AppendLine("             bool V_8,");
            sb.AppendLine("             bool V_9,");
            sb.AppendLine("             bool V_10,");
            sb.AppendLine("             string V_11)");
            sb.AppendLine("    IL_0000:  nop");
            sb.AppendLine("    IL_0001:  newobj     instance void [System.Runtime]System.Text.StringBuilder::.ctor()");
            sb.AppendLine("    IL_0006:  stloc.0");
            sb.AppendLine("    IL_0007:  ldc.i4.0");
            sb.AppendLine("    IL_0008:  stloc.1");
            sb.AppendLine("    IL_0009:  ldc.i4.s   10");
            sb.AppendLine("    IL_000b:  stloc.2");
            sb.AppendLine("    IL_000c:  ldc.i4.s   100");
            sb.AppendLine("    IL_000e:  stloc.3");
            sb.AppendLine("    IL_000f:  ldloc.0");
            sb.AppendLine("    IL_0010:  ldstr      \"[\"");
            sb.AppendLine("    IL_0015:  callvirt   instance class [System.Runtime]System.Text.StringBuilder [System.Runtime]System.Text.StringBuilder::Append(string)");
            sb.AppendLine("    IL_001a:  pop");
            sb.AppendLine("    IL_001b:  nop");
            sb.AppendLine("    IL_001c:  ldarg.1");
            sb.AppendLine("    IL_001d:  callvirt   instance class [System.Runtime]System.Collections.Generic.IEnumerator`1<!0> class [System.Runtime]System.Collections.Generic.IEnumerable`1<!!T>::GetEnumerator()");
            sb.AppendLine("    IL_0022:  stloc.s    V_5");
            sb.AppendLine("    .try");
            sb.AppendLine("    {");
            sb.AppendLine("      IL_0024:  br         IL_00e0");
            sb.AppendLine("");
            sb.AppendLine("      IL_0029:  ldloc.s    V_5");
            sb.AppendLine("      IL_002b:  callvirt   instance !0 class [System.Runtime]System.Collections.Generic.IEnumerator`1<!!T>::get_Current()");
            sb.AppendLine("      IL_0030:  stloc.s    V_6");
            sb.AppendLine("      IL_0032:  nop");
            sb.AppendLine("      IL_0033:  ldloc.1");
            sb.AppendLine("      IL_0034:  ldc.i4.1");
            sb.AppendLine("      IL_0035:  add");
            sb.AppendLine("      IL_0036:  stloc.1");
            sb.AppendLine("      IL_0037:  ldloc.1");
            sb.AppendLine("      IL_0038:  ldc.i4.0");
            sb.AppendLine("      IL_0039:  cgt");
            sb.AppendLine("      IL_003b:  stloc.s    V_7");
            sb.AppendLine("      IL_003d:  ldloc.s    V_7");
            sb.AppendLine("      IL_003f:  brfalse.s  IL_004d");
            sb.AppendLine("");
            sb.AppendLine("      IL_0041:  ldloc.0");
            sb.AppendLine("      IL_0042:  ldstr      \", \"");
            sb.AppendLine("      IL_0047:  callvirt   instance class [System.Runtime]System.Text.StringBuilder [System.Runtime]System.Text.StringBuilder::Append(string)");
            sb.AppendLine("      IL_004c:  pop");
            sb.AppendLine("      IL_004d:  ldloca.s   V_6");
            sb.AppendLine("      IL_004f:  constrained. !!T");
            sb.AppendLine("      IL_0055:  callvirt   instance string [System.Runtime]System.Object::ToString()");
            sb.AppendLine("      IL_005a:  stloc.s    V_4");
            sb.AppendLine("      IL_005c:  ldarg.2");
            sb.AppendLine("      IL_005d:  ldc.i4.0");
            sb.AppendLine("      IL_005e:  ceq");
            sb.AppendLine("      IL_0060:  stloc.s    V_8");
            sb.AppendLine("      IL_0062:  ldloc.s    V_8");
            sb.AppendLine("      IL_0064:  brfalse.s  IL_00b6");
            sb.AppendLine("");
            sb.AppendLine("      IL_0066:  nop");
            sb.AppendLine("      IL_0067:  ldloc.s    V_4");
            sb.AppendLine("      IL_0069:  callvirt   instance int32 [System.Runtime]System.String::get_Length()");
            sb.AppendLine("      IL_006e:  ldloc.3");
            sb.AppendLine("      IL_006f:  ble.s      IL_0077");
            sb.AppendLine("");
            sb.AppendLine("      IL_0071:  ldloc.3");
            sb.AppendLine("      IL_0072:  ldc.i4.0");
            sb.AppendLine("      IL_0073:  cgt");
            sb.AppendLine("      IL_0075:  br.s       IL_0078");
            sb.AppendLine("");
            sb.AppendLine("      IL_0077:  ldc.i4.0");
            sb.AppendLine("      IL_0078:  stloc.s    V_9");
            sb.AppendLine("      IL_007a:  ldloc.s    V_9");
            sb.AppendLine("      IL_007c:  brfalse.s  IL_0093");
            sb.AppendLine("");
            sb.AppendLine("      IL_007e:  ldloc.s    V_4");
            sb.AppendLine("      IL_0080:  ldc.i4.0");
            sb.AppendLine("      IL_0081:  ldloc.3");
            sb.AppendLine("      IL_0082:  callvirt   instance string [System.Runtime]System.String::Substring(int32,");
            sb.AppendLine("                                                                                    int32)");
            sb.AppendLine("      IL_0087:  ldstr      \" ... \"");
            sb.AppendLine("      IL_008c:  call       string [System.Runtime]System.String::Concat(string,");
            sb.AppendLine("                                                                        string)");
            sb.AppendLine("      IL_0091:  stloc.s    V_4");
            sb.AppendLine("      IL_0093:  ldstr      \"\\\"");
            sb.AppendLine("      IL_0098:  ldloc.s    V_4");
            sb.AppendLine("      IL_009a:  ldstr      \"\\\"");
            sb.AppendLine("      IL_009f:  ldstr      \"\\\\\\\"");
            sb.AppendLine("      IL_00a4:  callvirt   instance string [System.Runtime]System.String::Replace(string,");
            sb.AppendLine("                                                                                  string)");
            sb.AppendLine("      IL_00a9:  ldstr      \"\\\"");
            sb.AppendLine("      IL_00ae:  call       string [System.Runtime]System.String::Concat(string,");
            sb.AppendLine("                                                                        string,");
            sb.AppendLine("                                                                        string)");
            sb.AppendLine("      IL_00b3:  stloc.s    V_4");
            sb.AppendLine("      IL_00b5:  nop");
            sb.AppendLine("      IL_00b6:  ldloc.0");
            sb.AppendLine("      IL_00b7:  ldloc.s    V_4");
            sb.AppendLine("      IL_00b9:  callvirt   instance class [System.Runtime]System.Text.StringBuilder [System.Runtime]System.Text.StringBuilder::Append(string)");
            sb.AppendLine("      IL_00be:  pop");
            sb.AppendLine("      IL_00bf:  ldloc.1");
            sb.AppendLine("      IL_00c0:  ldloc.2");
            sb.AppendLine("      IL_00c1:  bne.un.s   IL_00c9");
            sb.AppendLine("");
            sb.AppendLine("      IL_00c3:  ldloc.2");
            sb.AppendLine("      IL_00c4:  ldc.i4.0");
            sb.AppendLine("      IL_00c5:  cgt");
            sb.AppendLine("      IL_00c7:  br.s       IL_00ca");
            sb.AppendLine("");
            sb.AppendLine("      IL_00c9:  ldc.i4.0");
            sb.AppendLine("      IL_00ca:  stloc.s    V_10");
            sb.AppendLine("      IL_00cc:  ldloc.s    V_10");
            sb.AppendLine("      IL_00ce:  brfalse.s  IL_00df");
            sb.AppendLine("");
            sb.AppendLine("      IL_00d0:  nop");
            sb.AppendLine("      IL_00d1:  ldloc.0");
            sb.AppendLine("      IL_00d2:  ldstr      \", ...\"");
            sb.AppendLine("      IL_00d7:  callvirt   instance class [System.Runtime]System.Text.StringBuilder [System.Runtime]System.Text.StringBuilder::Append(string)");
            sb.AppendLine("      IL_00dc:  pop");
            sb.AppendLine("      IL_00dd:  br.s       IL_00ec");
            sb.AppendLine("");
            sb.AppendLine("      IL_00df:  nop");
            sb.AppendLine("      IL_00e0:  ldloc.s    V_5");
            sb.AppendLine("      IL_00e2:  callvirt   instance bool [System.Runtime]System.Collections.IEnumerator::MoveNext()");
            sb.AppendLine("      IL_00e7:  brtrue     IL_0029");
            sb.AppendLine("");
            sb.AppendLine("      IL_00ec:  leave.s    IL_00fb");
            sb.AppendLine("");
            sb.AppendLine("    }  // end .try");
            sb.AppendLine("    finally");
            sb.AppendLine("    {");
            sb.AppendLine("      IL_00ee:  ldloc.s    V_5");
            sb.AppendLine("      IL_00f0:  brfalse.s  IL_00fa");
            sb.AppendLine("");
            sb.AppendLine("      IL_00f2:  ldloc.s    V_5");
            sb.AppendLine("      IL_00f4:  callvirt   instance void [System.Runtime]System.IDisposable::Dispose()");
            sb.AppendLine("      IL_00f9:  nop");
            sb.AppendLine("      IL_00fa:  endfinally");
            sb.AppendLine("    }  // end handler");
            sb.AppendLine("    IL_00fb:  ldloc.0");
            sb.AppendLine("    IL_00fc:  ldstr      \"]\"");
            sb.AppendLine("    IL_0101:  callvirt   instance class [System.Runtime]System.Text.StringBuilder [System.Runtime]System.Text.StringBuilder::Append(string)");
            sb.AppendLine("    IL_0106:  pop");
            sb.AppendLine("    IL_0107:  ldloc.0");
            sb.AppendLine("    IL_0108:  callvirt   instance string [System.Runtime]System.Object::ToString()");
            sb.AppendLine("    IL_010d:  stloc.s    V_11");
            sb.AppendLine("    IL_010f:  br.s       IL_0111");
            sb.AppendLine("");
            sb.AppendLine("    IL_0111:  ldloc.s    V_11");
            sb.AppendLine("    IL_0113:  ret");
            sb.AppendLine("  } // end of method Program::Get___Enumerable___AsListString___");
            sb.AppendLine("");
            sb.AppendLine("  .method public hidebysig specialname rtspecialname");
            sb.AppendLine("          instance void  .ctor() cil managed");
            sb.AppendLine("  {");
            sb.AppendLine("    // Code size       8 (0x8)");
            sb.AppendLine("    .maxstack  8");
            sb.AppendLine("    IL_0000:  ldarg.0");
            sb.AppendLine("    IL_0001:  call       instance void [CILWeaveProfiler]ILWeaveProfiler.CILWeaverLoggerBase::.ctor()");
            sb.AppendLine("    IL_0006:  nop");
            sb.AppendLine("    IL_0007:  ret");
            sb.AppendLine("  } // end of method Program::.ctor");
            sb.AppendLine("");
            sb.AppendLine("} // end of class ReferenceApp.Program");


            return sb.ToString();
        }
    }

}
