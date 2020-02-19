# IL Weave Logger

A proof of concept console application (.NET Standard 3.1) that can be executed in a CICD process that modifies a .NET assembly (EXE/DLL) by injecting method execution time and parameter value logging via the following steps:
- Disassembles a .NET assembly 
- Parses the disassembled IL code
- Modifies the IL code and inserts method execution time and parameter value logging
- Writes the modified IL code
- Assembles the code back to a .NET assembly 

This is a raw IL weaver, that does not depend on any third party libraries. 