﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team
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

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DiffLib;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.Decompiler.Tests.Helpers;
using Microsoft.CSharp;
using Mono.Cecil;
using NUnit.Framework;

namespace ICSharpCode.Decompiler.Tests
{
	[TestFixture]
	public class TestRunner
	{
		[Test]
		public void Async()
		{
			TestFile(@"..\..\Tests\Async.cs");
		}
		
		[Test, Ignore("disambiguating overloads is not yet implemented")]
		public void CallOverloadedMethod()
		{
			TestFile(@"..\..\Tests\CallOverloadedMethod.cs");
		}
		
		[Test, Category("Checked & unchecked")]
		public void CheckedUnchecked()
		{
			TestFile(@"..\..\Tests\CheckedUnchecked.cs");
		}

        [Test, Category("Checked")]
        public void CheckedInArrayCreationArgument()
        {
            var codeToTest = @"
using System;

public class CheckedUnchecked
{
	public void CheckedInArrayCreationArgument(int a, int b)
	{
		Console.WriteLine(new int[checked(a + b)]);
	}
}
";
            var expectedCode = @"
using System;

public class CheckedUnchecked
{
	public void CheckedInArrayCreationArgument(int a, int b)
	{
		Console.WriteLine(checked(new int[a + b]));
	}
}
";
            TestCode(expectedCode, codeToTest);
        }

        [Test, Category("Checked")]
        public void ForWithCheckedInitializerAndUncheckedIterator()
        {
            var codeToTest = @"
using System;

public class CheckedUnchecked
{
	public void ForWithCheckedInitializerAndUncheckedIterator(int n)
	{
		checked
		{
			int i = n;
			for (i -= 10; i < n; i = unchecked(i + 1))
			{
				n--;
			}
		}
	}
}
";
            var expectedCode = @"
using System;

public class CheckedUnchecked
{
	public void ForWithCheckedInitializerAndUncheckedIterator(int n)
	{
		int i = n;
		checked
		{
			for (i -= 10; i < n; i = unchecked(i + 1))
			{
				n--;
			}
		}
	}
}
";
            TestCode(expectedCode, codeToTest);
        }
		
		[Test, Ignore("Missing cast on null")]
		public void DelegateConstruction()
		{
			TestFile(@"..\..\Tests\DelegateConstruction.cs");
		}
		
		[Test, Ignore("Not yet implemented")]
		public void ExpressionTrees()
		{
			TestFile(@"..\..\Tests\ExpressionTrees.cs");
		}
		
		[Test]
		public void ExceptionHandling()
		{
			TestFile(@"..\..\Tests\ExceptionHandling.cs", optimize: false);
		}
		
		[Test]
		public void Generics()
		{
			TestFile(@"..\..\Tests\Generics.cs");
		}
		
		[Test]
		public void CustomShortCircuitOperators()
		{
			TestFile(@"..\..\Tests\CustomShortCircuitOperators.cs");
		}
		
		[Test]
		public void ControlFlowWithDebug()
		{
			TestFile(@"..\..\Tests\ControlFlow.cs", optimize: false, useDebug: true);
		}
		
		[Test]
		public void DoubleConstants()
		{
			TestFile(@"..\..\Tests\DoubleConstants.cs");
		}
		
		[Test]
		public void IncrementDecrement()
		{
			TestFile(@"..\..\Tests\IncrementDecrement.cs");
		}
		
		[Test]
		public void InitializerTests()
		{
			TestFile(@"..\..\Tests\InitializerTests.cs");
		}

		[Test]
		public void LiftedOperators()
		{
			TestFile(@"..\..\Tests\LiftedOperators.cs");
		}
		
		[Test, Category("Loops")]
		public void Loops()
		{
			TestFile(@"..\..\Tests\Loops.cs");
		}

        [Test, Category("Loops")]
        public void ForEachOverArray()
        {
            var codeToTest = @"
using System;

public class Loops
{
    public void ForEachOverArray(string[] array)
    {
        foreach (string text in array)
        {
            text.ToLower();
        }
    }
}
";
            var expectedCode = @"
using System;

public class Loops
{
	public void ForEachOverArray(string[] array)
    {
        for (int i = 0; i < array.Length; i++)
		{
            string text = array[i];
			text.ToLower();
		}
    }
}
";
            TestCode(expectedCode, codeToTest);
        }
		
		[Test]
		public void MultidimensionalArray()
		{
			TestFile(@"..\..\Tests\MultidimensionalArray.cs");
		}
		
		[Test]
		public void PInvoke()
		{
			TestFile(@"..\..\Tests\PInvoke.cs");
		}
		
		[Test]
		public void PropertiesAndEvents()
		{
			TestFile(@"..\..\Tests\PropertiesAndEvents.cs");
		}
		
		[Test]
		public void QueryExpressions()
		{
			TestFile(@"..\..\Tests\QueryExpressions.cs");
		}

        [Test, Category("Switch tests"), Ignore("switch transform doesn't recreate the exact original switch")]
		public void Switch()
		{
			TestFile(@"..\..\Tests\Switch.cs");
		}
		
		[Test]
		public void UndocumentedExpressions()
		{
			TestFile(@"..\..\Tests\UndocumentedExpressions.cs");
		}
		
		[Test]
		public void UnsafeCode()
		{
			TestFile(@"..\..\Tests\UnsafeCode.cs");
		}
		
		[Test]
		public void ValueTypes()
		{
			TestFile(@"..\..\Tests\ValueTypes.cs");
		}
		
		[Test, Category("Yield")]
		public void YieldReturn()
		{
			TestFile(@"..\..\Tests\YieldReturn.cs");
		}

        [Test, Category("Yield")]
        public void YieldReturnWithAnonymousMethods1()
        {
            var codeToTest = @"
using System;
using System.Collections.Generic;

public class Yield
{
    public static IEnumerable<Func<string>> YieldReturnWithAnonymousMethods1(IEnumerable<string> input)
	{
		foreach (string current in input)
		{
			yield return () => current;
		}
	}
}
";
            var expectedCode = @"
using System;
using System.Collections.Generic;

public class Yield
{
	public static IEnumerable<Func<string>> YieldReturnWithAnonymousMethods1(IEnumerable<string> input)
	{
		foreach (string current in input)
		{
			yield return () => current;
		}
	}
}
";
            TestCode(expectedCode, codeToTest);
        }
		
		[Test]
		public void TypeAnalysis()
		{
			TestFile(@"..\..\Tests\TypeAnalysisTests.cs");
		}
		
		static void TestFile(string fileName, bool useDebug = false)
		{
			TestFile(fileName, false, useDebug);
			TestFile(fileName, true, useDebug);
		}

        static void TestCode(string expectedCode, string codeToTest, bool useDebug = false)
        {
            TestCode(expectedCode, codeToTest, false, useDebug);
            TestCode(expectedCode, codeToTest, true, useDebug);
        }

		static void TestFile(string fileName, bool optimize, bool useDebug = false)
		{
			string code = File.ReadAllText(fileName);
            TestCode(code, code, optimize, useDebug);
		}

        private static void TestCode(string expectedCode, string codeToTest, bool optimize, bool useDebug)
        {
            AssemblyDefinition assembly = Compile(codeToTest, optimize, useDebug);
            AstBuilder decompiler = new AstBuilder(new DecompilerContext(assembly.MainModule));
            decompiler.AddAssembly(assembly);
            new Helpers.RemoveCompilerAttribute().Run(decompiler.SyntaxTree);
            StringWriter output = new StringWriter();
            decompiler.GenerateCode(new PlainTextOutput(output));
            var decompiledOutput = output.ToString();
            CodeAssert.AreEqual(expectedCode, decompiledOutput);
        }

		static AssemblyDefinition Compile(string code, bool optimize, bool useDebug)
		{
			CSharpCodeProvider provider = new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v4.0" } });
			CompilerParameters options = new CompilerParameters();
			options.CompilerOptions = "/unsafe /o" + (optimize ? "+" : "-") + (useDebug ? " /debug": "");
			options.ReferencedAssemblies.Add("System.Core.dll");
			CompilerResults results = provider.CompileAssemblyFromSource(options, code);
			try {
				if (results.Errors.Count > 0) {
					StringBuilder b = new StringBuilder("Compiler error:");
					foreach (var error in results.Errors) {
						b.AppendLine(error.ToString());
					}
					throw new Exception(b.ToString());
				}
				return AssemblyDefinition.ReadAssembly(results.PathToAssembly);
			} finally {
				File.Delete(results.PathToAssembly);
				results.TempFiles.Delete();
			}
		}
	}
}
