using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Deref_aftet_null_2
{
    internal class Walker : CSharpSyntaxWalker
    {
        public LocalContext Context;
        public SyntaxNodeAnalysisContext AnalysisContext;
        public SemanticModel Semantic;
        public DiagnosticDescriptor Rule;
        enum Actions
        {
            Assign,
            DoSmthMembers,
            DoSmthAll,
            DontKnowContext
        }
        void Report(Location location, string name, SyntaxNodeAnalysisContext context)
        {
            var diagnostic = Diagnostic.Create(Rule, location, name);
            context.ReportDiagnostic(diagnostic);
        }
        void ResetGlobalVariables(LocalContext context)
        {
            foreach (var item in context.Symbols)
            {
                if (item.Key.Kind != SymbolKind.Local && item.Key.Kind != SymbolKind.Parameter)
                {
                    item.Value.Members = new Dictionary<ISymbol, SymbolInfo>(SymbolEqualityComparer.Default);
                    item.Value.minValue = ValueSuggestion.AssignedUnknown;
                    item.Value.maxValue = ValueSuggestion.AssignedUnknown;
                    item.Value.Default = ValueSuggestion.AssignedUnknown;
                }
            }
        }
        void ResetAllMembers()
        {
            foreach (var item in Context.Symbols)
            {
                item.Value.Members = new Dictionary<ISymbol, SymbolInfo>(SymbolEqualityComparer.Default);
                item.Value.Default = ValueSuggestion.AssignedUnknown;
            }
        }
        void ResetContext()
        {
            Context.Symbols = new Dictionary<ISymbol, SymbolInfo>(SymbolEqualityComparer.Default);
            Context.Default = ValueSuggestion.AssignedUnknown;
        }

        void WriteByType(ITypeSymbol type, LocalContext context, ValueSuggestion max)
        {
            void wrt(Dictionary<ISymbol, SymbolInfo> members)
            {
                foreach (var item in members)
                {
                    ITypeSymbol symbolType = null;
                    switch (item.Key.Kind)
                    {
                        case SymbolKind.Field:
                            symbolType = ((IFieldSymbol)item.Key).Type;
                            break;
                        case SymbolKind.Local:
                            symbolType = ((ILocalSymbol)item.Key).Type;
                            break;
                        case SymbolKind.Parameter:
                            symbolType = ((IParameterSymbol)item.Key).Type;
                            break;
                        case SymbolKind.Property:
                            symbolType = ((IPropertySymbol)item.Key).Type;
                            break;
                        default:
                            break;
                    }

                    if (SymbolEqualityComparer.Default.Equals(symbolType, type))
                    {
                        item.Value.maxValue = (ValueSuggestion)(Math.Max((int)max, (int)item.Value.maxValue));
                        item.Value.minValue = (ValueSuggestion)(Math.Max((int)max, (int)item.Value.minValue));
                        item.Value.Default = (ValueSuggestion)(Math.Max((int)max, (int)item.Value.Default));
                    }
                    else
                    {
                        item.Value.Default = (ValueSuggestion)(Math.Max((int)max, (int)item.Value.Default));
                    }
                    wrt(item.Value.Members);
                }
            }

            wrt(context.Symbols);
        }
        void TryWrite(SyntaxNode node, LocalContext context, Actions action, ValueSuggestion min = ValueSuggestion.AssignedUnknown, ValueSuggestion max = ValueSuggestion.AssignedUnknown, ValueSuggestion def = ValueSuggestion.AssignedUnknown)
        {
            var info = context.TryGetAdd(node, Semantic, true);
            if (info != null)
            {
                switch (action)
                {
                    case Actions.Assign:
                        info.minValue = (min == ValueSuggestion.AssignedWithNull || min == ValueSuggestion.ComparedWithNull) ? ValueSuggestion.AssignedWithNull : ValueSuggestion.AssignedUnknown;
                        info.maxValue = (max == ValueSuggestion.AssignedWithNull || max == ValueSuggestion.ComparedWithNull) ? ValueSuggestion.AssignedWithNull : ValueSuggestion.AssignedUnknown;
                        info.Members = new Dictionary<ISymbol, SymbolInfo>(SymbolEqualityComparer.Default);
                        info.Default = def;
                        break;
                    case Actions.DoSmthMembers:
                        ResetAllMembers();
                        break;
                    case Actions.DoSmthAll:
                        ResetAllMembers();
                        info.minValue = ValueSuggestion.AssignedUnknown;
                        info.maxValue = ValueSuggestion.AssignedUnknown;
                        info.Default = ValueSuggestion.AssignedUnknown;
                        break;
                    case Actions.DontKnowContext:
                        info.minValue = ValueSuggestion.Unknown;
                        info.maxValue = ValueSuggestion.Unknown;
                        break;
                    default:
                        break;
                }

            }
            else
            {
                var exprType = Semantic.GetTypeInfo(node).Type;

                if (exprType != null && !exprType.IsValueType)
                {
                    switch (action)
                    {
                        case Actions.Assign:
                            if (max >= ValueSuggestion.Unknown)
                            {
                                WriteByType(exprType, context, ValueSuggestion.AssignedUnknown);
                            }
                            break;
                        case Actions.DoSmthMembers:
                            ResetAllMembers();
                            break;
                        case Actions.DoSmthAll:
                            ResetAllMembers();
                            WriteByType(exprType, context, ValueSuggestion.AssignedUnknown);
                            break;
                        case Actions.DontKnowContext:
                            WriteByType(exprType, context, ValueSuggestion.Unknown);
                            break;
                        default:
                            break;
                    }

                }
            }
        }

        #region reset
        public override void VisitReturnStatement(ReturnStatementSyntax node)
        {
            base.VisitReturnStatement(node);
            ResetContext();
        }
        public override void VisitGotoStatement(GotoStatementSyntax node)
        {
            base.VisitGotoStatement(node);
            ResetContext();
        }
        public override void VisitThrowStatement(ThrowStatementSyntax node)
        {
            base.VisitThrowStatement(node);
            ResetContext();
        }
        public override void VisitContinueStatement(ContinueStatementSyntax node)
        {
            base.VisitContinueStatement(node);
            ResetContext();
        }
        public override void VisitBreakStatement(BreakStatementSyntax node)
        {
            base.VisitBreakStatement(node);
            ResetContext();
        }
        public override void VisitYieldStatement(YieldStatementSyntax node)
        {
            base.VisitYieldStatement(node);
            ResetContext();
        }
        #endregion

        #region avoid
        public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            var context_ = Context;
            Context = new LocalContext();
            base.VisitParenthesizedLambdaExpression(node);
            Context = context_;
        }

        public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            var context_ = Context;
            Context = new LocalContext();
            base.VisitConditionalExpression(node);
            Context = context_;
        }

        public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
        {
            var context_ = Context;
            Context = new LocalContext();
            base.VisitLocalFunctionStatement(node);
            Context = context_;
        }
        #endregion  

        #region dereference
        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            var symbolInfo = Context.TryGetAdd(node.Expression, Semantic, false);
            if (symbolInfo != null)
            {
                if (symbolInfo.minValue == ValueSuggestion.ComparedWithNull || symbolInfo.minValue == ValueSuggestion.AssignedWithNull)
                {
                    Report(node.Expression.GetLocation(), node.Expression.GetText().ToString().Trim(), AnalysisContext);
                }
            }
            base.VisitMemberAccessExpression(node);
        }

        public override void VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {
            var symbolInfo = Context.TryGetAdd(node.Expression, Semantic, false);
            if (symbolInfo != null)
            {
                if (symbolInfo.minValue == ValueSuggestion.ComparedWithNull || symbolInfo.minValue == ValueSuggestion.AssignedWithNull)
                {
                    Report(node.Expression.GetLocation(), node.Expression.GetText().ToString().Trim(), AnalysisContext);
                }
            }
            base.VisitElementAccessExpression(node);
        }
        #endregion
        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (!IsGood(node))
            {
                ResetContext();
            }
            else if (!node.IsKind(SyntaxKind.LogicalAndExpression) && !node.IsKind(SyntaxKind.LogicalOrExpression))
            {
                base.VisitBinaryExpression(node);
            }
        }
        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            base.VisitAssignmentExpression(node);
            if (node.Right.IsKind(SyntaxKind.NullLiteralExpression))
            {
                var left = Context.TryGetAdd(node.Left, Semantic, true);
                if (left != null && left.maxValue == ValueSuggestion.AssignedUnknown)
                {
                    Context.TryGetAdd(node.Left, Semantic, true).maxValue = ValueSuggestion.Unknown;
                }
            }
            else if (Context.TryGetAdd(node.Right, Semantic, true) != null)
            {
                var symbolInfo = Context.TryGetAdd(node.Right, Semantic, true);
                TryWrite(node.Left, Context, Actions.Assign, symbolInfo.minValue, symbolInfo.maxValue, symbolInfo.Default);
            }
            else
            {
                TryWrite(node.Left, Context, Actions.Assign);
            }
        }
        void VisitLoop(SyntaxNode condition, SyntaxNode body)
        {
            if (IsGood(condition))
            {
                if (condition != null)
                {
                    var comparedWithNull = condition.DescendantNodesAndSelf().OfType<BinaryExpressionSyntax>().Where(x => x.Left.IsKind(SyntaxKind.NullLiteralExpression) || x.Right.IsKind(SyntaxKind.NullLiteralExpression)).Select(x => x.Left.IsKind(SyntaxKind.NullLiteralExpression) ? x.Right : x.Left);
                    comparedWithNull = comparedWithNull.Union(condition.DescendantNodesAndSelf().OfType<IsPatternExpressionSyntax>().Where(x => x.Pattern.IsKind(SyntaxKind.NullLiteralExpression)).Select(x => x.Expression));

                    if (comparedWithNull.Count() == 0)
                    {
                        LocalContext statement_c = new LocalContext(Context);
                        var context_ = Context;
                        Context = statement_c;
                        base.Visit(body);
                        Context = context_;
                        Context.Merge(statement_c);
                        return;
                    }
                    else
                    {
                        LocalContext statement_c = new LocalContext(Context);
                        var context_ = Context;
                        Context = statement_c;
                        base.Visit(body);
                        Context = context_;
                        Context.Merge(statement_c);

                        foreach (var item in comparedWithNull)
                        {
                            var s2 = statement_c.TryGetAdd(item, Semantic, true);

                            if (s2 != null)
                            {
                                var s1 = Context.TryGetAdd(item, Semantic, true);
                                s1.minValue = ValueSuggestion.AssignedUnknown;
                            }
                        }

                    }
                }
                else
                {
                    var context_ = Context;
                    LocalContext statement_c = new LocalContext(Context);
                    Context = statement_c;
                    base.Visit(body);
                    Context = context_;
                    Context.Merge(statement_c);
                    return;
                }
            }
            else
            {
                ResetContext();
                base.Visit(body);
                ResetContext();
            }
        }
        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            VisitLoop(node.Condition, node.Statement);
        }
        public override void VisitForStatement(ForStatementSyntax node)
        {
            VisitLoop(node.Condition, node.Statement);
        }
        void MemberInvoked(SyntaxNode node, LocalContext context)
        {
            TryWrite(node, context, Actions.DoSmthMembers);
        }
        void ArgumentPushed(ArgumentSyntax node, LocalContext context)
        {
            TryWrite(node.Expression, context, node.RefKindKeyword.IsKind(SyntaxKind.None) ? Actions.DoSmthMembers : Actions.DoSmthAll);
        }
        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (node.Expression.GetText().ToString().Trim() == "Environment.Exit")
            {
                Context.Symbols = new Dictionary<ISymbol, SymbolInfo>(SymbolEqualityComparer.Default);
                return;
            }
            if (Context.TryGetAdd(node.Expression, Semantic, false) != null)
            {
                if (Context.TryGetAdd(node.Expression, Semantic, false).minValue == ValueSuggestion.ComparedWithNull || Context.TryGetAdd(node.Expression, Semantic, false).minValue == ValueSuggestion.AssignedWithNull)
                {
                    Report(node.Expression.GetLocation(), node.Expression.GetText().ToString().Trim(), AnalysisContext);
                }
            }

            base.VisitInvocationExpression(node);
            var invocationSymbol = Semantic.GetSymbolInfo(node);
            bool knownMethod = invocationSymbol.Symbol != null && invocationSymbol.Symbol.Kind == SymbolKind.Method && (((IMethodSymbol)invocationSymbol.Symbol).ContainingNamespace != null);

            if (!knownMethod || (knownMethod && (!((IMethodSymbol)invocationSymbol.Symbol).ContainingNamespace.Name.StartsWith("System"))))
            {
                MemberInvoked(node.Expression, Context);
                foreach (var item in node.ArgumentList.Arguments)
                {
                    ArgumentPushed(item, Context);
                }
                ResetGlobalVariables(Context);
            }
        }
        bool IsGood(SyntaxNode node)
        {
            if (node == null || node.IsKind(SyntaxKind.None))
            {
                return true;
            }
            if (node.DescendantNodes().All(x => !x.IsKind(SyntaxKind.SimpleAssignmentExpression))
            && node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().All(x => x.ArgumentList.Arguments.All(z => z.RefKindKeyword.Kind() == SyntaxKind.None)))
            {
                foreach (var item in node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>())
                {
                    if (Semantic.GetSymbolInfo(item).Symbol != null && Semantic.GetSymbolInfo(item).Symbol.Kind == SymbolKind.Method && (((IMethodSymbol)Semantic.GetSymbolInfo(item).Symbol).ContainingNamespace != null))
                    {
                        if ((!((IMethodSymbol)Semantic.GetSymbolInfo(item).Symbol).ContainingNamespace.Name.StartsWith("System")) && item.ArgumentList.Arguments.Count > 0)
                        {
                            return false;
                        }
                    }

                }
                return true;
            }
            return false;
        }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            if (IsGood(node.Condition))//no assignments, methods do not change members
            {
                LocalContext statement_c = new LocalContext(Context);
                LocalContext else_c = new LocalContext(Context);

                var comparedWithNull = node.Condition.DescendantNodesAndSelf().OfType<BinaryExpressionSyntax>().Where(x => x.Left.IsKind(SyntaxKind.NullLiteralExpression) || x.Right.IsKind(SyntaxKind.NullLiteralExpression)).Select(x => x.Left.IsKind(SyntaxKind.NullLiteralExpression) ? x.Right : x.Left);
                comparedWithNull = comparedWithNull.Union(node.Condition.DescendantNodesAndSelf().OfType<IsPatternExpressionSyntax>().Where(x => x.Pattern.IsKind(SyntaxKind.NullLiteralExpression)).Select(x => x.Expression));
                foreach (var item in comparedWithNull)
                {
                    TryWrite(item, statement_c, Actions.DontKnowContext);
                    TryWrite(item, else_c, Actions.DontKnowContext);
                }


                var context_ = Context;
                Context = statement_c;
                base.Visit(node.Statement);
                if (node.Else != null)
                {
                    Context = else_c;
                    base.Visit(node.Else);
                }
                Context = context_;

                if ((node.Condition.IsKind(SyntaxKind.EqualsExpression) || node.Condition.IsKind(SyntaxKind.NotEqualsExpression)) && ((BinaryExpressionSyntax)node.Condition).Right.IsKind(SyntaxKind.NullLiteralExpression))
                {
                    if (!node.Condition.IsKind(SyntaxKind.EqualsExpression))
                    {
                        (else_c, statement_c) = (statement_c, else_c);
                    }

                    Context.Merge(statement_c);
                    Context.Merge(else_c);
                    var left = ((BinaryExpressionSyntax)node.Condition).Left;
                    var stmnLeft = statement_c.TryGetAdd(left, Semantic, true);
                    //   stmnLeft.minValue = ValueSuggestion.ComparedWithNull;
                    if (stmnLeft != null)
                    {
                        if (stmnLeft.maxValue != ValueSuggestion.AssignedUnknown)
                        {
                            Context.TryGetAdd(left, Semantic, true).minValue = ValueSuggestion.ComparedWithNull;
                        }
                        else
                        {
                            Context.TryGetAdd(left, Semantic, true).minValue = stmnLeft.minValue;
                        }
                    }
                }
                else
                {
                    if (node.Else != null)
                    {
                        statement_c.Merge(else_c, false);
                    }
                    Context.Merge(statement_c);
                    foreach (var item in comparedWithNull)
                    {
                        var s2 = statement_c.TryGetAdd(item, Semantic, true);
                        if (s2 != null && s2.maxValue != ValueSuggestion.AssignedUnknown)
                        {
                            var s1 = Context.TryGetAdd(item, Semantic, true);
                            s1.minValue = ValueSuggestion.ComparedWithNull;
                        }
                        else if (s2 != null)
                        {
                            var s1 = Context.TryGetAdd(item, Semantic, true);
                            s1.minValue = ValueSuggestion.Unknown;
                        }
                    }
                }
            }
            else
            {
                foreach (var item in node.Condition.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>())
                {
                    var invocationSymbol = Semantic.GetSymbolInfo(item);
                    bool knownMethod = invocationSymbol.Symbol != null && invocationSymbol.Symbol.Kind == SymbolKind.Method && (((IMethodSymbol)invocationSymbol.Symbol).ContainingNamespace != null);

                    if (!knownMethod || (knownMethod && (!((IMethodSymbol)invocationSymbol.Symbol).ContainingNamespace.Name.StartsWith("System"))))
                    {
                        MemberInvoked(item.Expression, Context);
                        foreach (var arg in item.ArgumentList.Arguments)
                        {
                            ArgumentPushed(arg, Context);
                        }
                        ResetGlobalVariables(Context);
                    }
                }
                foreach (var item in node.Condition.DescendantNodesAndSelf().OfType<AssignmentExpressionSyntax>())
                {
                    TryWrite(item, Context, Actions.Assign);
                }

                LocalContext statement_c = new LocalContext(Context);
                LocalContext else_c = new LocalContext(Context);

                var comparedWithNull = node.Condition.DescendantNodesAndSelf().OfType<BinaryExpressionSyntax>().Where(x => x.Left.IsKind(SyntaxKind.NullLiteralExpression) || x.Right.IsKind(SyntaxKind.NullLiteralExpression)).Select(x => x.Left.IsKind(SyntaxKind.NullLiteralExpression) ? x.Right : x.Left);
                comparedWithNull = comparedWithNull.Union(node.Condition.DescendantNodesAndSelf().OfType<IsPatternExpressionSyntax>().Where(x => x.Pattern.IsKind(SyntaxKind.NullLiteralExpression)).Select(x => x.Expression));

                foreach (var item in comparedWithNull)
                {
                    TryWrite(item, statement_c, Actions.DontKnowContext);
                    TryWrite(item, else_c, Actions.DontKnowContext);
                }

                var context_ = Context;
                Context = statement_c;
                base.Visit(node.Statement);
                if (node.Else != null)
                {
                    Context = else_c;
                    base.Visit(node.Else);
                }
                Context = context_;
                if (node.Else != null)
                {
                    statement_c.Merge(else_c, false);
                }
                Context.Merge(statement_c);
            }
        }
    }
}
