// Copyright (c) AlphaSierraPapa for the SharpDevelop Team
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

using NUnit.Framework;

namespace ICSharpCode.Decompiler.Tests
{
	[TestFixture]
	public class TestRunner
	{
		[Test]
		public void Async()
		{
            CodeTest.TestFile(@"..\..\Tests\Async.cs");
		}
		
		[Test]
		public void CallOverloadedMethod()
		{
            CodeTest.TestFile(@"..\..\Tests\CallOverloadedMethod.cs");
		}
		
		[Test, Category("Checked & unchecked")]
		public void CheckedUnchecked()
		{
            CodeTest.TestFile(@"..\..\Tests\CheckedUnchecked.cs");
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
            CodeTest.TestCode(expectedCode, codeToTest);
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
            CodeTest.TestCode(expectedCode, codeToTest);
        }

        [Test, Category("DelegateConstruction")]
		public void DelegateConstruction()
		{
			CodeTest.TestFile(@"..\..\Tests\DelegateConstruction.cs");
		}

        [Test, Category("DelegateConstruction")]
        public void DelegateConstructionInstanceMembersCaptureOfThis()
        {
            var codeToTest = @"
using System;

public class DelegateConstruction
{
	public Action CaptureOfThis()
	{
		return delegate 
        {
			this.CaptureOfThis();
		};
	}
}
";
            CodeTest.TestCode(codeToTest, codeToTest);
        }

        [Test, Category("DelegateConstruction")]
        public void DelegateConstructionInstanceMembersCaptureOfThisAndParameter()
        {
            var codeToTest = @"
using System;

public class DelegateConstruction
{
	public Action CaptureOfThisAndParameter(int a)
	{
		return delegate 
        {
			this.CaptureOfThisAndParameter(a);
		};
	}
}
";
            CodeTest.TestCode(codeToTest, codeToTest);
        }

        [Test, Category("DelegateConstruction")]
        public void DelegateConstructionInstanceMembersCaptureOfThisAndParameterInForEach()
        {
            var codeToTest = @"
using System;
using System.Linq;

public class DelegateConstruction
{
	public Action CaptureOfThisAndParameter(int a)
	{
		return delegate 
        {
			this.CaptureOfThisAndParameter(a);
		};
	}

	public Action CaptureOfThisAndParameterInForEach(int a)
	{
		foreach (int item in Enumerable.Empty<int>()) 
        {
			if (item > 0)
            {
				return delegate 
                {
					this.CaptureOfThisAndParameter(item + a);
				};
			}
		}
		return null;
	}
}
";
            CodeTest.TestCode(codeToTest, codeToTest);
        }

        [Test, Category("DelegateConstruction")]
        public void DelegateConstructionInstanceMembersCaptureOfThisAndParameterInForEachWithItemCopy()
        {
            var codeToTest = @"
using System;
using System.Linq;

public class DelegateConstruction
{
	public Action CaptureOfThisAndParameter(int a)
	{
		return delegate 
        {
			this.CaptureOfThisAndParameter(a);
		};
	}

	public Action CaptureOfThisAndParameterInForEachWithItemCopy(int a)
	{
		foreach (int item in Enumerable.Empty<int>()) 
        {
			int copyOfItem = item;
			if (item > 0) 
            {
				return delegate 
                {
					this.CaptureOfThisAndParameter(item + a + copyOfItem);
				};
			}
		}
		return null;
	}
}
";
            CodeTest.TestCode(codeToTest, codeToTest);
        }

        [Test, Category("DelegateConstruction")]
        public void DelegateConstructionInstanceMembersLambdaInForLoop()
        {
            var codeToTest = @"
using System;

public class DelegateConstruction
{
	public void LambdaInForLoop()
	{
		for (int i = 0; i < 100000; i++) 
        {
			this.Bar(() => this.Foo());
		}
	}
		
	public int Foo()
	{
		return 0;
	}
		
	public void Bar(Func<int> f)
	{
	}
}
";
            CodeTest.TestCode(codeToTest, codeToTest);
        }

        [Test, Category("DelegateConstruction")]
        public void DelegateConstructionMethodBoundOnNull()
        {
            var codeToTest = @"
using System;

public static class DelegateConstruction
{
	public static void Test(this string a)
	{
	}

    public static Action ExtensionMethodBoundOnNull()
	{
		return new Action(((string)null).Test);
	}

	public static object InstanceMethodOnNull()
	{
		return new Func<string>(((string)null).ToUpper);
	}
}
";
            CodeTest.TestCode(codeToTest, codeToTest);
        }
        [Test, Category("DelegateConstruction")]
        public void DelegateConstructionTypeInference()
        {
            var codeToTest = @"
using System;

public static class DelegateConstruction
{
	public static Func<int, Func<int, int>> CurriedAddition(int a)
	{
		return b => c => a + b + c;
	}
	
	public static Func<int, Func<int, Func<int, int>>> CurriedAddition2(int a)
	{
		return b => c => d => a + b + c + d;
	}
}
";
            CodeTest.TestCode(codeToTest, codeToTest);
        }

		[Test, Ignore("Not yet implemented")]
		public void ExpressionTrees()
		{
            CodeTest.TestFile(@"..\..\Tests\ExpressionTrees.cs");
		}
		
		[Test]
		public void ExceptionHandling()
		{
            CodeTest.TestFile(@"..\..\Tests\ExceptionHandling.cs", optimize: false);
		}
		
		[Test]
		public void Generics()
		{
            CodeTest.TestFile(@"..\..\Tests\Generics.cs");
		}
		
		[Test]
		public void CustomShortCircuitOperators()
		{
            CodeTest.TestFile(@"..\..\Tests\CustomShortCircuitOperators.cs");
		}
		
		[Test]
		public void ControlFlowWithDebug()
		{
            CodeTest.TestFile(@"..\..\Tests\ControlFlow.cs", optimize: false, useDebug: true);
		}
		
		[Test]
		public void DoubleConstants()
		{
            CodeTest.TestFile(@"..\..\Tests\DoubleConstants.cs");
		}
		
		[Test]
		public void IncrementDecrement()
		{
            CodeTest.TestFile(@"..\..\Tests\IncrementDecrement.cs");
		}
		
		[Test]
		public void InitializerTests()
		{
            CodeTest.TestFile(@"..\..\Tests\InitializerTests.cs");
		}

		[Test]
		public void LiftedOperators()
		{
            CodeTest.TestFile(@"..\..\Tests\LiftedOperators.cs");
		}
		
		[Test, Category("Loops")]
		public void Loops()
		{
            CodeTest.TestFile(@"..\..\Tests\Loops.cs");
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
            CodeTest.TestCode(expectedCode, codeToTest);
        }

        [Test, Category("Loops")]
        public void ForEachOverOldTypedCollection()
        {
            var codeToTest = @"
using System;
using System.CodeDom;

public class Loops
{
    public void ForEachOverOldTypedCollection(CodeAttributeArgumentCollection collection)
    {
        foreach (CodeAttributeArgument codeAttributeArgument in collection)
        {
            codeAttributeArgument.Name.ToLower();
        }
    }
}
";
            var expectedCode = @"
using System;
using System.CodeDom;

public class Loops
{
	public void ForEachOverOldTypedCollection(CodeAttributeArgumentCollection collection)
    {
        foreach (CodeAttributeArgument codeAttributeArgument in collection)
        {
            codeAttributeArgument.Name.ToLower();
        }
    }
}
";
            CodeTest.TestCode(expectedCode, codeToTest);
        }

        [Test, Category("Loops")]
        public void ForEachTypedCollectionWithCast()
        {
            var codeToTest = @"using System;
using System.Management;
public class Loops
{
    public static string ForEachTypedCollectionWithCast(string query, string field)
    {
	    ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(""root\\CIMV2"", query);
		foreach (ManagementObject managementObject in managementObjectSearcher.Get())
		{
			string text = Convert.ToString(managementObject[field]);
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
		}
        return null;
    }
}
";
            var expectedCode = @"
using System;
using System.Management;

public class Loops
{
    public static string ForEachTypedCollectionWithCast(string query, string field)
    {
	    ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(""root\\CIMV2"", query);
		foreach (ManagementObject managementObject in managementObjectSearcher.Get())
		{
			string text = Convert.ToString(managementObject[field]);
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
		}
		return null;
    }
}
";
            CodeTest.TestCode(expectedCode, codeToTest, true, false);
            var expectedCodeUnoptimized = @"
using System;
using System.Management;

public class Loops
{
    public static string ForEachTypedCollectionWithCast(string query, string field)
    {
	    ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(""root\\CIMV2"", query);
        string result;
		foreach (ManagementObject managementObject in managementObjectSearcher.Get())
		{
			string value = Convert.ToString(managementObject[field]);
			if (!string.IsNullOrEmpty(value))
			{
                result = value;
				return result;
			}
		}
        result = null;
		return result;
    }
}
";
            CodeTest.TestCode(expectedCodeUnoptimized, codeToTest, false, false);
        }
		
		[Test]
		public void MultidimensionalArray()
		{
            CodeTest.TestFile(@"..\..\Tests\MultidimensionalArray.cs");
		}
		
		[Test]
		public void PInvoke()
		{
            CodeTest.TestFile(@"..\..\Tests\PInvoke.cs");
		}
		
		[Test]
		public void PropertiesAndEvents()
		{
            CodeTest.TestFile(@"..\..\Tests\PropertiesAndEvents.cs");
		}
		
		[Test]
		public void QueryExpressions()
		{
            CodeTest.TestFile(@"..\..\Tests\QueryExpressions.cs");
		}

        [Test, Category("Switch tests"), Ignore("switch transform doesn't recreate the exact original switch")]
		public void Switch()
		{
            CodeTest.TestFile(@"..\..\Tests\Switch.cs");
		}
		
		[Test]
		public void UndocumentedExpressions()
		{
            CodeTest.TestFile(@"..\..\Tests\UndocumentedExpressions.cs");
		}
		
		[Test]
		public void UnsafeCode()
		{
            CodeTest.TestFile(@"..\..\Tests\UnsafeCode.cs");
		}
		
		[Test]
		public void ValueTypes()
		{
            CodeTest.TestFile(@"..\..\Tests\ValueTypes.cs");
		}
		
		[Test, Category("Yield")]
		public void YieldReturn()
		{
            CodeTest.TestFile(@"..\..\Tests\YieldReturn.cs");
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
            CodeTest.TestCode(expectedCode, codeToTest);
        }
		
		[Test]
		public void TypeAnalysis()
		{
            CodeTest.TestFile(@"..\..\Tests\TypeAnalysisTests.cs");
		}
	}
}
