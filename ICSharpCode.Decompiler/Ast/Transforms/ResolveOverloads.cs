using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.PatternMatching;
using Mono.Cecil;

namespace ICSharpCode.Decompiler.Ast.Transforms
{
    class ResolveOverloads : IAstTransform
    {
		readonly DecompilerContext context;

        public ResolveOverloads(DecompilerContext context)
		{
			this.context = context;
		}

        public void Run(AstNode compilationUnit)
        {
            foreach (InvocationExpression invocation in compilationUnit.Descendants.OfType<InvocationExpression>())
            {
                MemberReferenceExpression mre = invocation.Target as MemberReferenceExpression;
                MethodReference methodReference = invocation.Annotation<MethodReference>();
                if (mre != null && methodReference != null && invocation.Arguments.Any())
                {
                    if (methodReference.Parameters.Count == 0)
                    {
                        continue;
                    }

                    var typeDefinition = methodReference.DeclaringType.Resolve();
                    if (typeDefinition == null)
                    {
                        continue;
                    }

                    MethodDefinition methodDefinition = methodReference.Resolve();
                    if (methodDefinition == null)
                    {
                        continue;
                    }

                    var overloads = typeDefinition.Methods.Where(_ => _.Name == methodReference.Name && _.Parameters.Count == invocation.Arguments.Count).ToList();
                    
                    int initialCount = int.MaxValue;
                    while (overloads.Count < initialCount && overloads.Count > 1)
                    {
                        initialCount = overloads.Count;

                        // Do attempt to reduce the count of overloads by iteration 
                        // through all arguments and try to kick overloads 
                        // which does not have match
                        int i = -1;
                        foreach (var invocationArgument in invocation.Arguments)
                        {
                            i++;
                            overloads = this.ReducePrimitiveExpressionCalls(i, GetExpressionType(invocationArgument), overloads);
                        }
                    }

                    if (overloads.Count > 1)
                    {
                        int i = -1;
                        foreach (var invocationArgument in invocation.Arguments)
                        {
                            i++;
                            var targetTypeAst = AstBuilder.ConvertType(methodDefinition.Parameters[i].ParameterType);
                            invocationArgument.ReplaceWith(new CastExpression(targetTypeAst, invocationArgument.Clone()));
                        }
                    }
                }
            }
        }

        private List<MethodDefinition> ReducePrimitiveExpressionCalls(int argumentIndex, TypeReference value, List<MethodDefinition> overloads)
        {
            foreach (var overloadedMethod in overloads.ToList())
            {
                var parameter = overloadedMethod.Parameters[argumentIndex];
                if (!IsParameterMatch(parameter.ParameterType, value))
                {
                    overloads.Remove(overloadedMethod);
                    continue;
                }
            }

            return overloads;
        }

        private List<MethodDefinition> ReducePrimitiveExpressionCalls(int argumentIndex, object value, List<MethodDefinition> overloads)
        {
            foreach (var overloadedMethod in overloads.ToList())
            {
                var parameter = overloadedMethod.Parameters[argumentIndex];
                if (!IsParameterMatch(parameter.ParameterType, value))
                {
                    overloads.Remove(overloadedMethod);
                    continue;
                }
            }

            return overloads;
        }

        private TypeReference GetExpressionType(Expression expression)
        {
            var primitiveExpression = expression as PrimitiveExpression;
            if (primitiveExpression != null)
            {
                if (primitiveExpression.Value == null)
                {
                    return null;
                }

                var type = primitiveExpression.Value.GetType();
                TypeReference typeReference = new TypeReference(type.Namespace, type.Name, this.context.CurrentModule, this.context.CurrentModule, type.IsValueType);
                return typeReference;
            }

            var arrayCreateExpression = expression as ArrayCreateExpression;
            if (arrayCreateExpression != null)
            {
                return null;
            }

            return null;
        }

        private bool IsParameterMatch(TypeReference typeReference, object value)
        {
            if (!typeReference.IsValueType && value == null)
            {
                return true;
            }

            var valueType = value.GetType();
            return IsAssignableFrom(typeReference, valueType);
        }

        private bool IsParameterMatch(TypeReference typeReference, TypeReference typeToMatch)
        {
            if (typeToMatch == null)
            {
                return true;
            }

            return IsAssignableFrom(typeReference, typeToMatch);
        }

        private static bool IsAssignableFrom(TypeReference typeReference, TypeReference testTypeReference)
        {
            var testType = testTypeReference.Resolve();
            while (testType != null && testType.BaseType != null)
            {
                var result = typeReference.FullName == testType.FullName;
                if (result)
                {
                    return result;
                }

                // Check for implicit conversion operators
                testType = testType.BaseType.Resolve();
            }

            return typeReference.FullName == testType.FullName;
        }

        private static bool IsAssignableFrom(TypeReference typeReference, Type testType)
        {
            while (testType.BaseType != null)
            {
                var result = typeReference.FullName == testType.FullName;
                if (result)
                {
                    return result;
                }

                // Check for implicit conversion operators
                testType = testType.BaseType;
            }

            return typeReference.FullName == testType.FullName;
        }
    }
}
