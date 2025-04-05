using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Deref_after_null_3
{
    internal class Condition
    {
        public SemanticModel semantic;
        public SymbolTree<SimpleCondition> All;
        public bool WithNull;

        public Condition()
        {
            All = new SymbolTree<SimpleCondition>();
            WithNull = false;
        }
        public Condition Clone()
        {
            var clone = new Condition();
            clone.WithNull = WithNull;
            clone.semantic = semantic;
            clone.All = (SymbolTree<SimpleCondition>)All.Clone();
            return clone;
        }
        //(symbol, IsConditional)
        public static List<(ISymbol, bool)> DecomposeName(SyntaxNode name, SemanticModel semantic)
        {
            if (name.IsKind(SyntaxKind.VariableDeclarator))
            {
                var symbol = semantic.GetDeclaredSymbol(((VariableDeclaratorSyntax)name));
                if (symbol != null)
                {
                    return new List<(ISymbol, bool)>() { (symbol, false) };
                }
            }
            else if (name.IsKind(SyntaxKind.IdentifierName))
            {
                var symbol = semantic.GetSymbolInfo(name).Symbol;
                if (symbol != null)
                {
                    return new List<(ISymbol, bool)>() { (symbol, false) };
                }
            }
            else if (name.IsKind(SyntaxKind.MemberBindingExpression))
            {
                return DecomposeName(((MemberBindingExpressionSyntax)name).Name, semantic);
            }
            else if (name.IsKind(SyntaxKind.SimpleMemberAccessExpression) || name.IsKind(SyntaxKind.ConditionalAccessExpression))
            {
                if (name.DescendantNodes().All(x => x.IsKind(SyntaxKind.SimpleMemberAccessExpression) || x.IsKind(SyntaxKind.IdentifierName) || x.IsKind(SyntaxKind.ThisExpression) || x.IsKind(SyntaxKind.ConditionalAccessExpression) || x.IsKind(SyntaxKind.MemberBindingExpression)))
                {
                    List<(ISymbol, bool)> symbols = new List<(ISymbol, bool)>();
                    if (name.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                    {
                        var left = DecomposeName(((MemberAccessExpressionSyntax)name).Expression, semantic);
                        if (left == null)
                        {
                            return null;
                        }
                        var right = DecomposeName((((MemberAccessExpressionSyntax)name).Name), semantic);
                        if (right == null)
                        {
                            return null;
                        }
                        left.AddRange(right);
                        return left;
                    }
                    else if (name.IsKind(SyntaxKind.ConditionalAccessExpression))
                    {
                        var left = DecomposeName(((ConditionalAccessExpressionSyntax)name).Expression, semantic);
                        left[left.Count - 1] = (left[left.Count - 1].Item1, true);
                        if (left == null)
                        {
                            return null;
                        }
                        var right = DecomposeName((((ConditionalAccessExpressionSyntax)name).WhenNotNull), semantic);
                        if (right == null)
                        {
                            return null;
                        }
                        left.AddRange(right);
                        return left;

                    }
                }
                else
                {
                    return null;
                }
            }

            return null;

        }

        static SyntaxNode IsComparisonWithNull(SyntaxNode condition, out bool IsNull)
        {
            if (condition.IsKind(SyntaxKind.IsPatternExpression))
            {
                var casted = (IsPatternExpressionSyntax)condition;
                if (casted.Pattern.IsKind(SyntaxKind.ConstantPattern) && ((ConstantPatternSyntax)casted.Pattern).Expression.IsKind(SyntaxKind.NullLiteralExpression))
                {
                    IsNull = true;
                    return casted.Expression;
                }
                else if (casted.Pattern.IsKind(SyntaxKind.NotPattern) && ((UnaryPatternSyntax)casted.Pattern).Pattern.IsKind(SyntaxKind.ConstantPattern) && ((ConstantPatternSyntax)((UnaryPatternSyntax)casted.Pattern).Pattern).Expression.IsKind(SyntaxKind.NullLiteralExpression))
                {
                    IsNull = false;
                    return casted.Expression;
                }
                else
                {
                    IsNull = false;
                    return null;
                }
            }
            else if (condition.IsKind(SyntaxKind.EqualsExpression) || condition.IsKind(SyntaxKind.NotEqualsExpression))
            {

                var casted = (BinaryExpressionSyntax)condition;
                if (casted.Right.IsKind(SyntaxKind.NullLiteralExpression))
                {
                    IsNull = condition.IsKind(SyntaxKind.EqualsExpression);
                    return casted.Left;
                }
                else
                {
                    IsNull = false;
                    return null;
                }


            }
            else
            {
                IsNull = false;
                return null;
            }
        }

        public static Condition AnalyzeComparison(SyntaxNode condition, SemanticModel semantic)
        {
            Condition c = new Condition();
            bool isNull;
            var node = IsComparisonWithNull(condition, out isNull);


            if (node != null)
            {
                c.WithNull = true;
                var name = DecomposeName(node, semantic);
                if (name != null)
                {
                    List<(ISymbol, bool)> allNames = new List<(ISymbol, bool)>();
                    foreach (var item in name)
                    {
                        allNames.Add(item);
                        if (item.Item2)
                        {
                            SimpleCondition bCondition1 = new SimpleCondition()
                            {
                                Only = new NullAndNotNullContainer() { HasNotNull = !isNull, HasNull = false },
                                Always = new NullAndNotNullContainer() { HasNotNull = false, HasNull = isNull },
                                Valid = true
                            };
                            c.All.GetAdd(allNames, 0, true).val = bCondition1;
                        }

                    }
                    SimpleCondition bCondition = new SimpleCondition()
                    {
                        Only = new NullAndNotNullContainer() { HasNotNull = !isNull, HasNull = isNull },
                        Always = new NullAndNotNullContainer() { HasNotNull = !isNull, HasNull = isNull },
                        Valid = true
                    };
                    c.All.GetAdd(name, 0, true).val = bCondition;

                }
            }


            return c;
        }
        public static Condition GetCondition(SyntaxNode condition, SemanticModel semantic)
        {
            if (condition.IsKind(SyntaxKind.LogicalAndExpression))
            {
                return GetCondition(((BinaryExpressionSyntax)condition).Left, semantic) & GetCondition(((BinaryExpressionSyntax)condition).Right, semantic);
            }
            else if (condition.IsKind(SyntaxKind.LogicalOrExpression))
            {
                return GetCondition(((BinaryExpressionSyntax)condition).Left, semantic) | GetCondition(((BinaryExpressionSyntax)condition).Right, semantic);
            }
            else if ((condition.IsKind(SyntaxKind.LogicalNotExpression)))
            {
                return !GetCondition(((PrefixUnaryExpressionSyntax)condition).Operand, semantic);

            }
            else if (condition.IsKind(SyntaxKind.ParenthesizedExpression))
            {
                return GetCondition(((ParenthesizedExpressionSyntax)condition).Expression, semantic);
            }

            else
            {
                return AnalyzeComparison(condition, semantic);
            }

        }

        public static Condition operator |(Condition a, Condition b)
        {
            SymbolTree<SimpleCondition>.Transform(a.All, b.All, SimpleCondition.MergeOr);
            a.WithNull |= b.WithNull;
            return a;
        }

        public static Condition operator !(Condition a)
        {
            var t = a.Clone();
            SymbolTree<SimpleCondition>.Transform(t.All, t.All, SimpleCondition.MergeNot);
            return t;
        }
        public static Condition operator &(Condition a, Condition b)
        {
            SymbolTree<SimpleCondition>.Transform(a.All, b.All, SimpleCondition.MergeAnd);
            a.WithNull |= b.WithNull;
            return a;
        }
    }
}
