// Copyright (c) Andrey Kurdyumov
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

namespace ICSharpCode.Decompiler.Tests
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using ICSharpCode.Decompiler.Ast;
    using ICSharpCode.Decompiler.Tests.Helpers;
    using Microsoft.CSharp;
    using Mono.Cecil;

    /// <summary>
    /// Set of helper methods which simplify testing of the code.
    /// </summary>
    public static class CodeTest
    {
        /// <summary>
        /// Tests file for ability to decompile identically.
        /// </summary>
        /// <param name="fileName">Filename which has to be decompiled.</param>
        /// <param name="useDebug">Debug settings for the compiler.</param>
        internal static void TestFile(string fileName, bool useDebug = false)
        {
            TestFile(fileName, false, useDebug);
            TestFile(fileName, true, useDebug);
        }

        /// <summary>
        /// Assert that code compiled with given settings decompiled to expected results.
        /// </summary>
        /// <param name="expectedCode">Expected decompilation output.</param>
        /// <param name="codeToTest">Code which has to be compiled, and then decompiled.</param>
        /// <param name="useDebug">Use debug settings when compiling.</param>
        internal static void TestCode(string expectedCode, string codeToTest, bool useDebug = false)
        {
            TestCode(expectedCode, codeToTest, false, useDebug);
            TestCode(expectedCode, codeToTest, true, useDebug);
        }

        /// <summary>
        /// Tests that contents of file decompiled to it's output.
        /// </summary>
        /// <param name="fileName">Filename which should be tested on the ability to round-trip compile/decompile.</param>
        /// <param name="optimize">A value indicating whether use optimization settings for the compiler.</param>
        /// <param name="useDebug">A value indicating whether use debug settings for the compiler.</param>
        internal static void TestFile(string fileName, bool optimize, bool useDebug = false)
        {
            string code = File.ReadAllText(fileName);
            TestCode(code, code, optimize, useDebug);
        }

        /// <summary>
        /// Tests that compiled code will be decompiled to given results using specified compiler parameters.
        /// </summary>
        /// <param name="expectedCode">Expected decompiled output.</param>
        /// <param name="codeToTest">Code to compile</param>
        /// <param name="optimize">A value indicating whether use optimization settings for the compiler.</param>
        /// <param name="useDebug">A value indicating whether use debug settings for the compiler.</param>
        internal static void TestCode(string expectedCode, string codeToTest, bool optimize, bool useDebug)
        {
            AssemblyDefinition assembly = Compile(codeToTest, optimize, useDebug);
            AstBuilder decompiler = new AstBuilder(new DecompilerContext(assembly.MainModule));
            decompiler.AddAssembly(assembly);
            new Helpers.RemoveCompilerAttribute().Run(decompiler.SyntaxTree);
            StringWriter output = new StringWriter();
            decompiler.GenerateCode(new PlainTextOutput(output));
            var decompiledOutput = output.ToString();
            CodeAssert.AreEqual(expectedCode, decompiledOutput, optimize ? "Optimized code failed" : "Not optimized code failed");
        }

        /// <summary>
        /// Compile code with given parameters.
        /// </summary>
        /// <param name="code">Code to compile.</param>
        /// <param name="optimize">A value indicating whether use optimization settings for the compiler.</param>
        /// <param name="useDebug">A value indicating whether use debug settings for the compiler.</param>
        /// <returns>Compiled assembly.</returns>
        private static AssemblyDefinition Compile(string code, bool optimize, bool useDebug)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v4.0" } });
            CompilerParameters options = new CompilerParameters();
            options.CompilerOptions = "/unsafe /o" + (optimize ? "+" : "-") + (useDebug ? " /debug" : "");
            options.ReferencedAssemblies.Add("System.Core.dll");
            options.ReferencedAssemblies.Add("System.dll");
            options.ReferencedAssemblies.Add("System.Management.dll");
            CompilerResults results = provider.CompileAssemblyFromSource(options, code);
            try
            {
                if (results.Errors.Count > 0)
                {
                    StringBuilder b = new StringBuilder("Compiler error:");
                    foreach (var error in results.Errors)
                    {
                        b.AppendLine(error.ToString());
                    }
                    throw new Exception(b.ToString());
                }
                return AssemblyDefinition.ReadAssembly(results.PathToAssembly);
            }
            finally
            {
                File.Delete(results.PathToAssembly);
                results.TempFiles.Delete();
            }
        }
    }
}
