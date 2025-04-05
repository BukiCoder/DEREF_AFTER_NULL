using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Deref_after_null_3
{
    internal class Walker : CSharpSyntaxWalker
    {
        public LocalContext Context;
        public SyntaxNodeAnalysisContext AnalysisContext;
        public SemanticModel Semantic;
        public DiagnosticDescriptor Rule;

        private bool analyzeConditions = false;

        void Report(Location location, string name, SyntaxNodeAnalysisContext context)
        {
            var diagnostic = Diagnostic.Create(Rule, location, name);
            context.ReportDiagnostic(diagnostic);
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
                return true;
            }
            return false;
        }

        #region tree_methods
        StatesContainer SelectComparedAlwaysWhen(StatesContainer states, SimpleCondition condition)
        {
            if (condition.Valid)
            {
                var res = new StatesContainer(true);
                if (condition.Always.HasNull)
                {
                    res.states[1, 1] = new HashSet<Guid>(states.states[1, 1]);
                    res.states[1, 0] = new HashSet<Guid>(states.states[1, 0]);
                }
                if (condition.Always.HasNotNull)
                {
                    res.states[0, 0] = new HashSet<Guid>(states.states[0, 0]);
                    res.states[0, 1] = new HashSet<Guid>(states.states[0, 1]);

                }
                return res;
            }
            else
            {
                return new StatesContainer(true);
            }
        }

        StatesContainer SelectComparedUnknown(StatesContainer states, SimpleCondition condition)
        {
            if (condition.Valid && (!condition.Always.HasNull && !condition.Always.HasNotNull))
            {
                return new StatesContainer(states);
            }
            else
            {
                return new StatesContainer(true);
            }
        }

        StatesContainer SelectOther(StatesContainer states, SimpleCondition condition)
        {
            if (condition == null || !condition.Valid)
            {
                return new StatesContainer(states);
            }
            else
            {
                return new StatesContainer(true);
            }
        }

        StatesContainer SelectComparedOnlyWhen(StatesContainer states, SimpleCondition condition)
        {
            if (condition.Valid)
            {
                if ((!condition.Only.HasNull && !condition.Only.HasNotNull))
                {
                    return new StatesContainer(states);
                }
                else if (condition.Only.HasNull && condition.Only.HasNotNull)
                {
                    return new StatesContainer(true);
                }
                else
                {
                    var res = new StatesContainer(true);
                    if (condition.Only.HasNull)
                    {
                        res.states[0, 0].Clear();
                        res.states[0, 1].Clear();
                        res.states[1, 0] = new HashSet<Guid>(states.states[1, 0]);
                        res.states[1, 1] = new HashSet<Guid>(states.states[1, 1]);
                    }
                    else
                    {
                        res.states[1, 0].Clear();
                        res.states[1, 1].Clear();
                        res.states[0, 1] = new HashSet<Guid>(states.states[0, 1]);
                        res.states[0, 0] = new HashSet<Guid>(states.states[0, 0]);
                    }
                    return res;
                }
            }
            else
            {
                return new StatesContainer(true);
            }
        }
        StatesContainer Union(StatesContainer s1, StatesContainer s2)
        {
            s1.states[0, 1].UnionWith(s2.states[0, 1]);
            s1.states[0, 0].UnionWith(s2.states[0, 0]);
            s1.states[1, 0].UnionWith(s2.states[1, 0]);
            s1.states[1, 1].UnionWith(s2.states[1, 1]);
            return s1;
        }
        StatesContainer SetCompared(StatesContainer s1, StatesContainer s2)
        {
            foreach (var item in s2.states[1, 0])
            {
                if (s1.states[1, 0].Contains(item))
                {
                    s1.states[1, 0].Remove(item);
                    s1.states[1, 1].Add(item);
                }
            }
            foreach (var item in s2.states[0, 0])
            {
                if (s1.states[0, 0].Contains(item))
                {
                    s1.states[0, 0].Remove(item);
                    s1.states[0, 1].Add(item);
                }
            }
            return s1;
        }
        StatesContainer SelectComparedNotAlways(StatesContainer states, SimpleCondition condition)
        {
            if (condition.Valid)
            {
                var res = new StatesContainer(true);
                if (!condition.Always.HasNull)
                {
                    res.states[1, 1] = new HashSet<Guid>(states.states[1, 1]);
                    res.states[1, 0] = new HashSet<Guid>(states.states[1, 0]);
                }
                if (!condition.Always.HasNotNull)
                {
                    res.states[0, 0] = new HashSet<Guid>(states.states[0, 0]);
                    res.states[0, 1] = new HashSet<Guid>(states.states[0, 1]);

                }
                return res;
            }
            else
            {
                return new StatesContainer(states);
            }

        }


        #endregion

        #region reset
        public override void VisitReturnStatement(ReturnStatementSyntax node)
        {
            base.VisitReturnStatement(node);
            Context.Reset();
        }
        public override void VisitGotoStatement(GotoStatementSyntax node)
        {
            base.VisitGotoStatement(node);
            Context.Reset();
        }
        public override void VisitThrowStatement(ThrowStatementSyntax node)
        {
            base.VisitThrowStatement(node);
            Context.Reset();
        }
        public override void VisitContinueStatement(ContinueStatementSyntax node)
        {
            base.VisitContinueStatement(node);
            Context.Reset();
        }
        public override void VisitBreakStatement(BreakStatementSyntax node)
        {
            base.VisitBreakStatement(node);
            Context.outContexts.Add((SymbolTree<StatesContainer>)Context.Symbols.Clone());
            Context.Reset();
        }
        public override void VisitYieldStatement(YieldStatementSyntax node)
        {
            base.VisitYieldStatement(node);
            Context.Reset();
        }
        #endregion

        #region avoid
        public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            var context_ = Context;
            Context = new LocalContext();
            try { base.VisitParenthesizedLambdaExpression(node); }
            catch (Exception) { }
            finally { Context = context_; }
        }

        public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            var context_ = Context;
            Context = new LocalContext();
            try { base.VisitConditionalExpression(node); }
            catch (Exception) { }
            finally { Context = context_; }
        }

        public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
        {
            var context_ = Context;
            Context = new LocalContext();
            try { base.VisitLocalFunctionStatement(node); }
            catch (Exception) { }
            finally { Context = context_; }
        }
        #endregion  

        #region dereference

        void Access(SyntaxNode node)
        {
            var symbolInfo = Context.GetAdd(node, Semantic, true);
            if (symbolInfo != null)
            {
                if ((symbolInfo.val.states[1, 1].Count > 0))
                {
                    Report(node.GetLocation(), node.GetText().ToString().Trim(), AnalysisContext);
                }
            }
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            Access(node.Expression);
            base.VisitMemberAccessExpression(node);
        }

        public override void VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {
            Access(node.Expression);
            base.VisitElementAccessExpression(node);
        }

        public override void VisitCastExpression(CastExpressionSyntax node)
        {
            Access(node.Expression);
            base.VisitCastExpression(node);
        }
        #endregion       

        #region loop
        void VisitLoop(SyntaxNode condition, SyntaxNode body)
        {

            if (IsGood(condition))
            {
                if (condition != null)
                {
                    var conditionAnalysis = Condition.GetCondition(condition, Semantic);
                    var elseConditionAnalysis = (!conditionAnalysis);
                    var statement = SymbolTree<StatesContainer>.Select(Context.Symbols, conditionAnalysis.All, SelectComparedAlwaysWhen);
                    var @else = SymbolTree<StatesContainer>.Select(Context.Symbols, elseConditionAnalysis.All, SelectComparedAlwaysWhen);
                    var statementAlways = SymbolTree<StatesContainer>.Select(Context.Symbols, conditionAnalysis.All, SelectComparedAlwaysWhen);
                    var elseAlways = SymbolTree<StatesContainer>.Select(Context.Symbols, elseConditionAnalysis.All, SelectComparedAlwaysWhen);
                    SymbolTree<StatesContainer>.Transform(statement, statementAlways, Union);
                    SymbolTree<StatesContainer>.Transform(@else, elseAlways, Union);
                    var comparedAll = SymbolTree<StatesContainer>.Select(Context.Symbols, conditionAnalysis.All, SelectComparedUnknown);
                    SymbolTree<StatesContainer>.Transform(comparedAll, SymbolTree<StatesContainer>.Select(Context.Symbols, elseConditionAnalysis.All, SelectComparedUnknown), Union);
                    var comparedAllClone = (SymbolTree<StatesContainer>)comparedAll.Clone();
                    var other = SymbolTree<StatesContainer>.Select(Context.Symbols, conditionAnalysis.All, SelectOther);
                    var otherClone = (SymbolTree<StatesContainer>)other.Clone();
                    var statementCompared = SymbolTree<StatesContainer>.Select(comparedAll, conditionAnalysis.All, SelectComparedOnlyWhen);
                    var elseCompared = SymbolTree<StatesContainer>.Select(comparedAllClone, elseConditionAnalysis.All, SelectComparedOnlyWhen);
                    SymbolTree<StatesContainer>.Transform(statement, statementCompared, Union);
                    SymbolTree<StatesContainer>.Transform(@else, elseCompared, Union);
                    SymbolTree<StatesContainer>.Transform(statement, other, Union);
                    SymbolTree<StatesContainer>.Transform(@else, otherClone, Union);

                    LocalContext statement_c = new LocalContext() { Symbols = statement };
                    LocalContext else_c = new LocalContext() { Symbols = @else };

                    var context_ = Context;
                    Context = statement_c;
                    bool exception = false;
                    try
                    {
                        base.Visit(body);
                    }
                    catch (Exception)
                    {
                        exception = true;
                    }
                    finally
                    {
                        Context = context_;
                    }
                    if (!exception)
                    {
                        SymbolTree<StatesContainer>.Transform(statementAlways, comparedAll, Union);
                        SymbolTree<StatesContainer>.Transform(elseAlways, comparedAllClone, Union);
                        SymbolTree<StatesContainer>.Transform(statement_c.Symbols, statementAlways, SetCompared);
                        SymbolTree<StatesContainer>.Transform(else_c.Symbols, elseAlways, SetCompared);
                        SymbolTree<StatesContainer>.Transform(statement_c.Symbols, else_c.Symbols, Union);
                        foreach (var item in statement_c.outContexts)
                        {
                            SymbolTree<StatesContainer>.Transform(statement_c.Symbols, item, Union);
                        }
                        Context.Symbols = SymbolTree<StatesContainer>.Select(statement_c.Symbols, conditionAnalysis.All, SelectComparedNotAlways);
                    }


                }
            }
            else
            {

                Context.Reset();              
                base.Visit(body);
                Context.Reset();
            }

        }
        public override void VisitDoStatement(DoStatementSyntax node)
        {
            base.Visit(node.Statement);
            if (IsGood(node.Condition))
            {
                if (node.Condition != null)
                {
                    var conditionAnalysis = Condition.GetCondition(node.Condition, Semantic);
                    var elseConditionAnalysis = (!conditionAnalysis);
                    var statement = SymbolTree<StatesContainer>.Select(Context.Symbols, conditionAnalysis.All, SelectComparedAlwaysWhen);
                    var statementAlways = SymbolTree<StatesContainer>.Select(Context.Symbols, conditionAnalysis.All, SelectComparedAlwaysWhen);
                    var comparedAll = SymbolTree<StatesContainer>.Select(Context.Symbols, conditionAnalysis.All, SelectComparedUnknown);
                    SymbolTree<StatesContainer>.Transform(comparedAll, SymbolTree<StatesContainer>.Select(Context.Symbols, elseConditionAnalysis.All, SelectComparedUnknown), Union);
                    SymbolTree<StatesContainer>.Transform(statementAlways, comparedAll, Union);
                    SymbolTree<StatesContainer>.Transform(Context.Symbols, statementAlways, SetCompared);
                    Context.Symbols = SymbolTree<StatesContainer>.Select(Context.Symbols, conditionAnalysis.All, SelectComparedNotAlways);

                }
            }
            else
            {
                Context.Reset();
            }
        }
        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            Access(node.Expression);
            base.VisitForEachStatement(node);
        }
        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            VisitLoop(node.Condition, node.Statement);
        }
        public override void VisitForStatement(ForStatementSyntax node)
        {
            VisitLoop(node.Condition, node.Statement);
        }
        #endregion

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (!IsGood(node))
            {
                Context.Reset();
            }
            else if (analyzeConditions || (!node.IsKind(SyntaxKind.LogicalAndExpression) && !node.IsKind(SyntaxKind.LogicalOrExpression)))
            {
                base.VisitBinaryExpression(node);
            }
        }
        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            base.VisitAssignmentExpression(node);
            Assign(node.Left, node.Right);
        }
        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var invocationSymbol = Semantic.GetSymbolInfo(node).Symbol;

            bool knownMethod = invocationSymbol != null && invocationSymbol.Kind == SymbolKind.Method;
            if (invocationSymbol?.Name == "Exit" && invocationSymbol?.ContainingSymbol?.Name == "Environment")
            {
                Context.Reset();
                return;
            }

            base.VisitInvocationExpression(node);

            if (knownMethod)
            {
                if (invocationSymbol.Name.ToLower().Contains("set") || invocationSymbol.Name.ToLower().Contains("assign") || invocationSymbol.Name.ToLower().Contains("init") || invocationSymbol.Name.ToLower().Contains("fill"))
                {
                    if (node.Expression.ChildNodes().Count() > 0)
                    {
                        var symbolInfo = Context.GetAdd(node.Expression.ChildNodes().First(), Semantic, true);
                        if (symbolInfo != null)
                        {
                            symbolInfo.val = new StatesContainer();
                            symbolInfo.Members = new Dictionary<ISymbol, SymbolTree<StatesContainer>>(SymbolEqualityComparer.Default);
                        }
                    }

                    var assignedSymbols = Context.Symbols.Members.Where(x => invocationSymbol.Name.ToLower().Contains(x.Key.Name.ToLower()));
                    foreach (var item in assignedSymbols)
                    {
                        item.Value.Members = new Dictionary<ISymbol, SymbolTree<StatesContainer>>(SymbolEqualityComparer.Default);
                        item.Value.val = new StatesContainer();
                    }
                }
            }
            foreach (var arg in node.ArgumentList.Arguments)
            {
                if (!arg.RefKindKeyword.IsKind(SyntaxKind.None))
                {
                    var name = Context.GetAdd(arg.Expression, Semantic, true);
                    if (name != null)
                    {
                        name.Members = new Dictionary<ISymbol, SymbolTree<StatesContainer>>(SymbolEqualityComparer.Default);
                        name.val = new StatesContainer();
                    }
                }
            }
        }

        void AssignSymbol(SymbolTree<StatesContainer> var, SymbolTree<StatesContainer> oth)
        {

            var.val.states = new HashSet<Guid>[2, 2]
            {
                { new HashSet<Guid>(oth.val.states[0, 0]), new HashSet<Guid>(oth.val.states[0, 1]) },
                { new HashSet<Guid>(oth.val.states[1, 0]), new HashSet<Guid>(oth.val.states[1, 1]) }
            };
            var.Members = new Dictionary<ISymbol, SymbolTree<StatesContainer>>(SymbolEqualityComparer.Default);
        }
        void AssignUnknown(SymbolTree<StatesContainer> var, SyntaxNode val)
        {

            var.val.states = new HashSet<Guid>[2, 2]
            {
                 { new HashSet<Guid>() { Guid.NewGuid() }, new HashSet<Guid>() },
                 { new HashSet<Guid>(), new HashSet<Guid>()}
            };
            if (val.IsKind(SyntaxKind.ElementAccessExpression) || val.IsKind(SyntaxKind.SimpleMemberAccessExpression))
            {
                var.val.states[1, 0].Add(Guid.NewGuid());
            }
            var.Members = new Dictionary<ISymbol, SymbolTree<StatesContainer>>(SymbolEqualityComparer.Default);
        }


        void Assign(SyntaxNode var, SyntaxNode val)
        {
            var varSymbol = Context.GetAdd(var, Semantic, true);
            if (varSymbol == null)
            {
                return;
            }
            if (val.IsKind(SyntaxKind.NullLiteralExpression))
            {
                int c = varSymbol.val.states[0, 1].Count + varSymbol.val.states[1, 1].Count;
                varSymbol.val.states[0, 0].Clear();
                varSymbol.val.states[1, 1].Clear();
                varSymbol.val.states[0, 1].Clear();
                varSymbol.val.states[1, 0].Clear();
                varSymbol.val.states[1, c > 0 ? 1 : 0].Add(Guid.NewGuid());
                varSymbol.Members = new Dictionary<ISymbol, SymbolTree<StatesContainer>>(SymbolEqualityComparer.Default);

            }
            else if (val.IsKind(SyntaxKind.AsExpression))
            {
                BinaryExpressionSyntax casted = ((BinaryExpressionSyntax)val);

                var symbolInfo = Context.GetAdd(casted.Left, Semantic, true);
                if (symbolInfo != null)
                {
                    AssignSymbol(varSymbol, symbolInfo);
                }
                else
                {
                    AssignUnknown(varSymbol, casted.Left);
                }

            }
            else if (val.IsKind(SyntaxKind.ObjectCreationExpression))
            {
                var tmp = new HashSet<Guid>();
                if (varSymbol.val.states[0, 1].Count + varSymbol.val.states[1, 1].Count > 0)
                {
                    tmp.Add(Guid.NewGuid());
                }
                varSymbol.val.states = new HashSet<Guid>[2, 2]
                {
                   { new HashSet<Guid>() { Guid.NewGuid() }, tmp },
                   { new HashSet<Guid>(), new HashSet<Guid>()}
                };
                Context.GetAdd(var, Semantic, true).Members = new Dictionary<ISymbol, SymbolTree<StatesContainer>>(SymbolEqualityComparer.Default);
            }
            else if (Context.GetAdd(val, Semantic, true) != null)
            {
                var symbolInfo = Context.GetAdd(val, Semantic, true);
                AssignSymbol(varSymbol, symbolInfo);
            }
            else
            {
                AssignUnknown(varSymbol, var);
            }
        }

        public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            base.VisitVariableDeclaration(node);

            foreach (var item in node.Variables)
            {
                if (item.Initializer == null || item.Initializer.Value == null)
                {
                    continue;
                }
                Assign(item, item.Initializer.Value);
            }
        }

        public override void VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
        {
            base.VisitConditionalAccessExpression(node);
            var symbolInfo = Context.GetAdd(node.Expression, Semantic, true);

            if (symbolInfo != null)
            {
                if (symbolInfo.val.states[1, 0].Count > 0)
                {
                    symbolInfo.val.states[1, 1].UnionWith(new HashSet<Guid>(symbolInfo.val.states[1, 0]));
                    symbolInfo.val.states[1, 0].Clear();

                }
            }
        }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            if (IsGood(node.Condition))//no assignments, methods do not change members
            {
                var conditionAnalysis = Condition.GetCondition(node.Condition, Semantic);

                if (!conditionAnalysis.WithNull || node.Condition.IsKind(SyntaxKind.EqualsExpression) || node.Condition.IsKind(SyntaxKind.NotEqualsExpression))
                {
                    if (node.Condition.DescendantNodesAndSelf().All(x => !x.IsKind(SyntaxKind.ConditionalAccessExpression)))
                    {
                        analyzeConditions = true;
                        Visit(node.Condition);
                        analyzeConditions = false;
                    }
                }
                var elseConditionAnalysis = !conditionAnalysis;
                var statement = SymbolTree<StatesContainer>.Select(Context.Symbols, conditionAnalysis.All, SelectComparedAlwaysWhen);
                var @else = SymbolTree<StatesContainer>.Select(Context.Symbols, elseConditionAnalysis.All, SelectComparedAlwaysWhen);
                var statement_always = (SymbolTree<StatesContainer>)statement.Clone();
                var else_always = (SymbolTree<StatesContainer>)@else.Clone();
                SymbolTree<StatesContainer>.Transform(statement, statement_always, Union);
                SymbolTree<StatesContainer>.Transform(@else, else_always, Union);
                var comparedAll = SymbolTree<StatesContainer>.Select(Context.Symbols, conditionAnalysis.All, SelectComparedUnknown);
                SymbolTree<StatesContainer>.Transform(comparedAll, SymbolTree<StatesContainer>.Select(Context.Symbols, (!conditionAnalysis).All, SelectComparedUnknown), Union);
                var comparedAllClone = (SymbolTree<StatesContainer>)comparedAll.Clone();
                SymbolTree<StatesContainer>.TransformBySelected(statement, comparedAll, conditionAnalysis.All, Union, SelectComparedOnlyWhen);
                SymbolTree<StatesContainer>.TransformBySelected(@else, comparedAllClone, elseConditionAnalysis.All, Union, SelectComparedOnlyWhen);
                var other = SymbolTree<StatesContainer>.Select(Context.Symbols, conditionAnalysis.All, SelectOther);
                var otherClone = (SymbolTree<StatesContainer>)other.Clone();
                SymbolTree<StatesContainer>.Transform(statement, other, Union);
                SymbolTree<StatesContainer>.Transform(@else, otherClone, Union);


                LocalContext statement_c = new LocalContext() { Symbols = statement };
                LocalContext else_c = new LocalContext() { Symbols = @else };
                SymbolTree<StatesContainer>.Transform(statement_always, comparedAll, Union);
                SymbolTree<StatesContainer>.Transform(else_always, comparedAllClone, Union);

                var context_ = Context;
                bool visited = false;
                try
                {
                    if (!statement_c.Symbols.Any((StatesContainer s) => { return s.states[0, 1].Count + s.states[0, 0].Count + s.states[1, 0].Count + s.states[1, 1].Count == 0; }))
                    {
                        Context = statement_c;

                        base.Visit(node.Statement);
                        visited = true;
                    }

                    if (node.Else != null)
                    {
                        if (!else_c.Symbols.Any((StatesContainer s) => { return s.states[0, 1].Count + s.states[0, 0].Count + s.states[1, 0].Count + s.states[1, 1].Count == 0; }))
                        {
                            Context = else_c;
                            base.Visit(node.Else);
                            visited = true;
                        }
                    }
                }
                catch (Exception)
                {
                    visited = false;
                }
                finally
                {
                    if (visited)
                    {
                        Context = context_;
                        SymbolTree<StatesContainer>.Transform(statement_c.Symbols, statement_always, SetCompared);
                        SymbolTree<StatesContainer>.Transform(else_c.Symbols, else_always, SetCompared);
                        SymbolTree<StatesContainer>.Transform(statement_c.Symbols, else_c.Symbols, Union);
                        Context.outContexts.AddRange(else_c.outContexts);
                        Context.outContexts.AddRange(statement_c.outContexts);
                        Context.Symbols = statement_c.Symbols;
                    }
                }


            }
            else
            {
                try
                {
                    Context.Reset();
                    Visit(node.Statement);
                    if (node.Else != null)
                    {
                        Context.Reset();
                        Visit(node.Else);
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    Context.Reset();
                }

            }
        }
    }
}
