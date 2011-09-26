using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;
using ICSharpCode.NRefactory.PatternMatching;

namespace ICSharpCode.Decompiler.Ast.Transforms {
    class OperatorCastRemoval :  ContextTrackingVisitor<AstNode> {

        private class PrimTypeDetails {
            public readonly int Precision;
            public readonly bool Signed;

            public PrimTypeDetails(int precision, bool signed) {
                Precision = precision;
                Signed = signed;
            }
        }

        public OperatorCastRemoval(DecompilerContext context)
            : base(context) {
        }
        private static readonly PrimitiveType _int = new PrimitiveType("int");
        private static readonly PrimitiveType _uint = new PrimitiveType("uint");
        private static readonly PrimitiveType _long = new PrimitiveType("long");
        private static readonly PrimitiveType _ulong = new PrimitiveType("ulong");
        private static readonly PrimitiveType _short = new PrimitiveType("short");
        private static readonly PrimitiveType _ushort = new PrimitiveType("ushort");
        private static readonly PrimitiveType _byte = new PrimitiveType("byte");
        private static readonly PrimitiveType _sbyte = new PrimitiveType("sbyte");

        private static string NormalizeTypeName(string typeName) {
            switch (typeName) {
                case "System.Int32":
                    return "int";
                case "System.Int64":
                    return "long";
                case "System.UInt32":
                    return "uint";
                case "System.UInt64":
                    return "ulong";
                case "System.Int16":
                    return "short";
                case "System.UInt16":
                    return "ushort";
                case "System.Byte":
                    return "byte";
                case "System.SByte":
                    return "sbyte";
                case "System.Decimal":
                    return "decimal";
            }
            return typeName;
        }

        private static string DenormalizeTypeName(string typeName) {
            switch (typeName) {
                case "int":
                    return "System.Int32";
                case "long":
                    return "System.Int64";
                case "uint":
                    return "System.UInt32";
                case "ulong":
                    return "System.UInt64";
                case "short":
                    return "System.Int16";
                case "ushort":
                    return "System.UInt16";
                case "byte":
                    return "System.Byte";
                case "sbyte":
                    return "System.SByte";
                case "decimal":
                    return "System.Decimal";
            }
            return typeName;
        }
        private static Dictionary<string, PrimTypeDetails> primTypes = new Dictionary<string, PrimTypeDetails>()
            {
            {"int",new PrimTypeDetails(32,true)},
            {"uint",new PrimTypeDetails(32,false)},
            {"long",new PrimTypeDetails(64,true)},
            {"ulong",new PrimTypeDetails(64,false)},
            {"short",new PrimTypeDetails(16,true)},
            {"ushort",new PrimTypeDetails(16,false)},
            {"byte",new PrimTypeDetails(8,false)},
            {"sbyte",new PrimTypeDetails(8,true)},
            };

        private static readonly BinaryOperatorType[] promotableOperatorTypes = new[]{BinaryOperatorType.Subtract,BinaryOperatorType.Add,BinaryOperatorType.Divide,BinaryOperatorType.Multiply,BinaryOperatorType.Modulus,BinaryOperatorType.BitwiseAnd,BinaryOperatorType.BitwiseOr,BinaryOperatorType.ExclusiveOr, BinaryOperatorType.Equality, BinaryOperatorType.InEquality,BinaryOperatorType.GreaterThan,BinaryOperatorType.LessThan,BinaryOperatorType.LessThanOrEqual,BinaryOperatorType.GreaterThanOrEqual};

        private PrimTypeDetails GetSmallestType(PrimTypeDetails left, PrimTypeDetails right) {
            if (left.Precision == right.Precision && left.Signed == right.Signed)
                return left;
            
            return null;
        }

        private static string Promote(string leftType, string rightType) {
            string remaining = null;
            Func<string, bool> matchEither = match => {
                if (leftType == match)
                    remaining = rightType;
                else if (rightType == match)
                    remaining = leftType;
                else
                    return false;
                return true;
            };
            Func<string, string[], bool> matchBoth = (match, matches) => matchEither(match) && matches.Contains(remaining);
            if (matchEither("decimal"))
                return remaining == "double" || remaining == "float" ? null : "decimal";
            if (matchEither("double"))
                return "double";
            if (matchEither("float"))
                return "float";
            if (matchEither("ulong"))
                return "ulong";
            if (matchEither("long"))
                return "long";
            if (matchBoth("uint", new[] { "sbyte", "short", "int" }))
                return "long";
            if (matchEither("uint"))
                return "uint";
            return "int";
        }
        private static string Promote(string inType, UnaryOperatorType op) {
            switch (op) {
                    case UnaryOperatorType.Minus:
                    if(inType == "uint")
                        return "long";
                    else {
                        goto case UnaryOperatorType.BitNot;
                    }
                case UnaryOperatorType.BitNot:
                case UnaryOperatorType.Plus:
                    return new[] { "sbyte", "byte", "short", "char" }.Contains(inType) ? "int" : inType;
            }
            return inType;

        }

        private static bool IsSafeCast(string fromType, string toType) {

            if(!primTypes.ContainsKey(fromType) || !primTypes.ContainsKey(toType))
                return false;
            var fromDet = primTypes[fromType];
            var toDet = primTypes[toType];
            return !toDet.Signed
                ? !fromDet.Signed && fromDet.Precision <= toDet.Precision
                : (fromDet.Signed ? fromDet.Precision <= toDet.Precision : fromDet.Precision < toDet.Precision);
        }

        private void UncastExpr(Expression suspiciousCast, Expression other) {
            var castExpression = suspiciousCast as CastExpression;
            if (castExpression == null) {
                return;
            }
            var castType = castExpression.Type as PrimitiveType;
            var exprType = TryGetExpressionType(castExpression.Expression);
            if (castType == null || exprType == null) {
                return;
            }
            var rType = TryGetExpressionType(other);
            if (rType == null) {
                return;
            }
            var rTypeName = NormalizeTypeName(rType);
            var exprTypeName = NormalizeTypeName(exprType);
            var castTypeName = castType.Keyword;

            var promoted = Promote(rTypeName, exprTypeName);
            var promoted2 = Promote(rTypeName, castTypeName);
            if (promoted == promoted2 && IsSafeCast(exprTypeName,castTypeName)) {
                var tr = FromPrimName(promoted);
                if (tr != null)
                    suspiciousCast.Parent.AddAnnotation(new TypeInformation(tr));
                suspiciousCast.ReplaceWith(castExpression.Expression);
            }
        }

        private static string TryGetExpressionType(Expression exp) {
            var ti = exp.Annotation<TypeInformation>();
            if (ti != null && ti.InferredType != null)
                return ti.InferredType.FullName;
            var ilv = exp.Annotation<ILVariable>();
            if (ilv != null && ilv.Type != null)
                return ilv.Type.FullName;
            return null;
        }

        private TypeReference FromPrimName(string typeName) {
            var ts = context.CurrentModule.TypeSystem;
            switch (typeName) {
                case "byte":
                    return ts.Byte;
                case "sbyte":
                    return ts.SByte;
                case "short":
                    return ts.Int16;
                case "ushort":
                    return ts.UInt16;
                case "long":
                    return ts.Int64;
                case "ulong":
                    return ts.UInt64;
                case "int":
                    return ts.Int32;
                case "uint":
                    return ts.UInt32;
                case "double":
                    return ts.Double;
                case "float":
                    return ts.Single;
                case "char":
                    return ts.Char;
                    default:
                    return null;
            }
        }
        public override AstNode VisitUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression, object data) {
            var ret = base.VisitUnaryOperatorExpression(unaryOperatorExpression, data);
            var castExpr = unaryOperatorExpression.Expression as CastExpression;
            if (castExpr == null)
                return ret;
            var castType = castExpr.Type as PrimitiveType;
            var exprType = TryGetExpressionType(castExpr.Expression);
            if(castType == null)
                return ret;
            var p = Promote(castType.Keyword, unaryOperatorExpression.Operator);
            var p2 = Promote(exprType, unaryOperatorExpression.Operator);
            if (p == p2 && IsSafeCast(p, p2))
                castExpr.ReplaceWith(castExpr.Expression);
            return ret;
        }

        public override AstNode VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression, object data) {
            var ret = base.VisitBinaryOperatorExpression(binaryOperatorExpression, data);
            if (promotableOperatorTypes.Contains(binaryOperatorExpression.Operator)) {
                UncastExpr(binaryOperatorExpression.Left, binaryOperatorExpression.Right);
                UncastExpr(binaryOperatorExpression.Right, binaryOperatorExpression.Left);
            }
            return ret;
        }
        public override AstNode VisitReturnStatement(ReturnStatement returnStatement, object data) {
            base.VisitReturnStatement(returnStatement, data);
            var retType = context.CurrentMethod.MethodReturnType.ReturnType;
            var castExp = returnStatement.Expression as CastExpression;
            if (castExp == null || !(castExp.Type is PrimitiveType))
                return null;
            var castType = (PrimitiveType)castExp.Type;
            var expType = TryGetExpressionType(castExp.Expression);
            if (IsImplicitCast(NormalizeTypeName(expType), castType.Keyword) && IsImplicitCast(castType.Keyword, NormalizeTypeName(retType.FullName)))
                returnStatement.Expression = castExp.Expression;
            return null;
        }

        private static readonly string[] baseTypes = new[]
        { "sbyte", "byte", "short", "ushort", "int", "uint", "long", "ulong", "float", "double", "decimal","char" };
        private static bool IsImplicitCast(string fromType, string toType) {
            if(fromType == toType)
                return true;
            if(!baseTypes.Contains(fromType) || !baseTypes.Contains(toType))
                return false;
            PrimTypeDetails fromDet, toDet;
            if(primTypes.TryGetValue(fromType,out fromDet) && primTypes.TryGetValue(toType, out toDet)) {
                return (!fromDet.Signed || toDet.Signed)
                    && fromDet.Precision <= (toDet.Signed ? toDet.Precision : 1 + toDet.Precision);
            }

            if (toType == "decimal")
                return fromType != "float" && fromType != "double";

            return toType == "double";
        }
    }
}
