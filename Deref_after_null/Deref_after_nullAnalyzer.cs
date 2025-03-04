using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Threading;

namespace Deref_after_null
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class Deref_after_nullAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DEREF_AFTER_NULL";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Usage";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        }

        private string GetNameIfNullExpr(BinaryExpressionSyntax expr)
        {
            var kinds = new SyntaxKind[] { SyntaxKind.IdentifierName, SyntaxKind.SimpleMemberAccessExpression, SyntaxKind.ElementAccessExpression };
            if (expr.Kind() == SyntaxKind.EqualsExpression || expr.Kind() == SyntaxKind.NotEqualsExpression)
            {
                if (kinds.Contains(expr.Left.Kind()) && expr.Right.Kind() == SyntaxKind.NullLiteralExpression)
                {
                    return expr.Left.GetText().ToString().Trim();
                }

                if (kinds.Contains(expr.Right.Kind()) && expr.Left.Kind() == SyntaxKind.NullLiteralExpression)
                {
                    return expr.Left.GetText().ToString().Trim();
                }
            }
            return null;
        }

        private enum Condition
        {
            IsNull,
            InNotNull,
            Unknown
        }

        Tuple<Dictionary<string, bool>, string[]> TestCondition(SyntaxNode condition, LocalContext context)
        {
            SyntaxKind[] kinds = { SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression, SyntaxKind.GreaterThanEqualsToken, SyntaxKind.GreaterThanExpression, SyntaxKind.LessThanEqualsToken, SyntaxKind.LessThanExpression, SyntaxKind.SimpleMemberAccessExpression, SyntaxKind.InvocationExpression };
            var expressions = condition.DescendantNodesAndSelf().OfType<BinaryExpressionSyntax>().Where(x => kinds.Contains(x.Kind())).ToArray();
            if (expressions.Length == 0)
            {
                return null;
            }

            string[] names = new string[expressions.Length];
            var conditionText = condition.GetText().ToString();
            int jj = 0;
            for (int i = 0; i < expressions.Length; i++)
            {
                var name = GetNameIfNullExpr(expressions[i]);
                if (name != null)
                {
                    var info = context.GetInfo(name);
                    if (info != null)
                    {
                        var pp = expressions[i].Kind() == SyntaxKind.EqualsExpression ? true : false;
                        if (!info.val.n)
                        {
                            conditionText = conditionText.Replace(expressions[i].GetText().ToString().Trim(), (!pp).ToString());
                        }
                        else if (!info.val.nn)
                        {
                            conditionText = conditionText.Replace(expressions[i].GetText().ToString().Trim(), pp.ToString());
                        }
                        else
                        {
                            conditionText = conditionText.Replace(expressions[i].GetText().ToString().Trim(), (pp ? "" : "!") + "{" + i + "}");
                            names[jj] = name;
                            jj++;
                        }
                    }
                }
                else
                {
                    conditionText = conditionText.Replace(expressions[i].GetText().ToString().Trim(), "{" + i + "}");
                    jj++;
                }
            }

            conditionText = conditionText.Replace("&&", " and ").Replace("||", " or ").Replace("!", " not ").Replace("^", " xor ");
            Dictionary<string, bool> results = new Dictionary<string, bool>();
            if (jj == 0)
            {
                var d = new Dictionary<string, bool>();
                d.Add("def", Convert.ToBoolean((new DataTable()).Compute(conditionText.ToLower(), String.Empty)));
                return Tuple.Create<Dictionary<string, bool>, string[]>(d, new string[0]);
            }

            var table = new DataTable();
            for (int j = 0; j < (int)Math.Pow(2, jj); j++)
            {
                var bin = Convert.ToString(j, 2);
                bin = new string('0', jj - bin.Length) + bin;
                var truefalse = bin.Select(x => (x == '1').ToString()).ToArray();
                string txt = String.Format(conditionText, truefalse).ToLower();
                var res = Convert.ToBoolean(table.Compute(txt.ToLower(), String.Empty));
                results.Add(bin, res);
            }

            return Tuple.Create<Dictionary<string, bool>, string[]>(results, names);
        }
        private Dictionary<string, Condition> OnlyWhen(Tuple<Dictionary<string, bool>, string[]> tests)
        {

            Dictionary<string, Condition> results = new Dictionary<string, Condition>();
            for (int j = 0; j < tests.Item2.Length; j++)
            {
                var name = tests.Item2[j];
                if (name != null)
                {
                    if (tests.Item1.Where(x => x.Value).All(x => x.Key[j] == '1'))
                    {
                        results.Add(name, Condition.IsNull);
                    }
                    else if (tests.Item1.Where(x => x.Value).All(x => x.Key[j] == '0'))
                    {
                        results.Add(name, Condition.InNotNull);
                    }
                    else
                    {
                        results.Add(name, Condition.Unknown);
                    }
                }
            }
            return results;
        }

        private Dictionary<string, Condition> AlwaysTrue(Tuple<Dictionary<string, bool>, string[]> tests)
        {

            Dictionary<string, Condition> results = new Dictionary<string, Condition>();
            for (int j = 0; j < tests.Item2.Length; j++)
            {
                var name = tests.Item2[j];
                if (name != null)
                {
                    if (tests.Item1.Where(x => x.Key[j] == '1').All(x => x.Value))
                    {
                        results.Add(name, Condition.IsNull);
                    }
                    else if (tests.Item1.Where(x => x.Key[j] == '0').All(x => x.Value))
                    {
                        results.Add(name, Condition.InNotNull);
                    }
                    else
                    {
                        results.Add(name, Condition.Unknown);
                    }
                }
            }
            return results;
        }


        void Report(Location loc, string name, SyntaxNodeAnalysisContext context1)
        {
            var diagnostic = Diagnostic.Create(Rule, loc, name);
            context1.ReportDiagnostic(diagnostic);
        }

        void ReportNode(LocalContext context, SyntaxNode node, SyntaxNodeAnalysisContext analysisContext)
        {
            var acceses = node.DescendantNodesAndSelf().OfType<MemberAccessExpressionSyntax>();
            foreach (var acc in acceses)
            {
                string s = acc.Expression.GetText().ToString().Trim();
                var reference = context.GetInfo(s);
                if (reference != null && (reference.val.n))
                {
                    Report(acc.Expression.GetLocation(), s, analysisContext);
                }
            }
            var acceses1 = node.DescendantNodesAndSelf().OfType<ElementAccessExpressionSyntax>();
            foreach (var acc in acceses1)
            {
                string s = acc.Expression.GetText().ToString().Trim();
                var reference = context.GetInfo(s);
                if (reference != null && (reference.val.n))
                {
                    Report(acc.Expression.GetLocation(), s, analysisContext);
                }
            }
        }

        void AddUniqNames(LocalContext to, LocalContext from)
        {
            var namesFrom = from.AllNames(from.variables);
            var namesTo = to.AllNames(to.variables);
            foreach (var v in namesFrom)
            {
                if (!from.GetInfo(v).isLocal)
                {
                    if (!namesTo.Contains(v))
                    {
                        to.Add(v, new ValueSuggestion(from.GetInfo(v).val));
                    }
                }
            }
        }

        void AddConditionContext(LocalContext context, Dictionary<string, Condition> conditions)
        {
            foreach (var exp in conditions)
            {
                if (exp.Value == Condition.IsNull)
                {
                    context.GetInfo(exp.Key).val.n = true;
                    context.GetInfo(exp.Key).val.nn = false;
                }
                else if (exp.Value == Condition.InNotNull)
                {
                    context.GetInfo(exp.Key).val.nn = true;
                    context.GetInfo(exp.Key).val.n = false;
                }
            }
        }
        private LocalContext AnalyzeBody(SyntaxNode body, LocalContext context, SyntaxNodeAnalysisContext analysisContext)
        {

            foreach (var item in body.ChildNodes())
            {
                if (item.IsKind(SyntaxKind.Block))
                {
                    AnalyzeBody(item, context, analysisContext);
                    continue;
                }
                else if (item.IsKind(SyntaxKind.IfStatement) || item.IsKind(SyntaxKind.WhileStatement))
                {

                    var condtion = item.IsKind(SyntaxKind.IfStatement) ? ((IfStatementSyntax)item).Condition : ((WhileStatementSyntax)item).Condition;
                    var statement = item.IsKind(SyntaxKind.IfStatement) ? ((IfStatementSyntax)item).Statement : ((WhileStatementSyntax)item).Statement;
                    var exprs = condtion.DescendantNodesAndSelf().OfType<BinaryExpressionSyntax>().Where(x => x.Kind() == SyntaxKind.EqualsExpression || x.Kind() == SyntaxKind.NotEqualsExpression);

                    foreach (var exp in exprs)
                    {
                        var name = GetNameIfNullExpr(exp);
                        nameInfo info;
                        if (name != null)
                        {

                            if ((info = context.GetInfo(name)) != null)
                            {
                                if (info.val.beyond)
                                {
                                    info.val.beyond = false;
                                    info.val.n = true;
                                }
                            }
                            else
                            {
                                context.Add(name, new ValueSuggestion() { n = true, nn = true, beyond = false });
                            }
                        }
                    }

                    Dictionary<string, Condition> onlyWhen = null;

                    ReportNode(context, condtion, analysisContext);

                    LocalContext ifBody = new LocalContext(context);
                    LocalContext elseBody = new LocalContext(context);

                    var tests = TestCondition(condtion, context);
                    if (tests != null)
                    {
                        onlyWhen = OnlyWhen(tests);

                        if (onlyWhen != null)
                        {
                            AddConditionContext(ifBody, onlyWhen);
                        }
                    }

                    ifBody = AnalyzeBody(statement, ifBody, analysisContext);
                    if ((statement.ChildNodes().Any(x => x.Kind() == SyntaxKind.ReturnStatement)) || (statement.Kind() == SyntaxKind.ReturnStatement))
                    {
                        ifBody.SetToLock();
                    }

                    if (tests != null)
                    {
                        onlyWhen = AlwaysTrue(tests);
                        if (onlyWhen != null)
                        {
                            for (int i = 0; i < onlyWhen.Keys.Count; i++)
                            {
                                var cond = onlyWhen.Keys.ElementAt(i);
                                if (onlyWhen[cond] == Condition.InNotNull)
                                {
                                    onlyWhen[cond] = Condition.IsNull;
                                }
                                else if (onlyWhen[cond] == Condition.IsNull)
                                {
                                    onlyWhen[cond] = Condition.InNotNull;
                                }

                            }
                            AddConditionContext(elseBody, onlyWhen);
                        }
                    }

                    if (item.IsKind(SyntaxKind.IfStatement) && ((IfStatementSyntax)item).Else != null)
                    {
                        elseBody = AnalyzeBody(((IfStatementSyntax)item).Else, elseBody, analysisContext);
                    }
                    else if (item.IsKind(SyntaxKind.WhileStatement))
                    {
                        ifBody.SetToLock();
                        elseBody.SetToLock();
                    }

                    foreach (var name in context.AllNames(context.variables))
                    {
                        if (tests != null && tests.Item2.Contains(name))
                        {
                            var ind = tests.Item2.ToList().IndexOf(name);
                            var info = context.GetInfo(name);
                            var isnull = tests.Item1.Where(x => x.Key[ind] == '1');
                            if (isnull.Count() > 0)
                            {
                                if (isnull.All(x => x.Value))
                                {
                                    info.val.n = false;
                                    if (ifBody != null)
                                    {
                                        info.val.n |= ifBody.GetInfo(name).val.n;
                                        info.val.nn |= ifBody.GetInfo(name).val.nn;
                                    }

                                }
                                else if (isnull.All(x => !x.Value))
                                {
                                    info.val.n = false;
                                    if (elseBody != null)
                                    {
                                        info.val.n |= elseBody.GetInfo(name).val.n;
                                        info.val.nn |= elseBody.GetInfo(name).val.nn;
                                    }
                                }
                                else
                                {
                                    info.val.n = elseBody.GetInfo(name).val.n | ifBody.GetInfo(name).val.n;
                                    info.val.nn |= elseBody.GetInfo(name).val.nn | ifBody.GetInfo(name).val.nn;
                                }
                            }

                            var isntnull = tests.Item1.Where(x => x.Key[ind] == '0');
                            if (isntnull.Count() > 0)
                            {
                                if (isntnull.All(x => x.Value))
                                {
                                    info.val.n |= ifBody.GetInfo(name).val.n;
                                    info.val.nn = ifBody.GetInfo(name).val.nn;
                                }
                                else if (isntnull.All(x => !x.Value))
                                {
                                    info.val.n |= elseBody.GetInfo(name).val.n;
                                    info.val.nn = elseBody.GetInfo(name).val.nn;
                                }
                                else
                                {
                                    info.val.n |= elseBody.GetInfo(name).val.n | ifBody.GetInfo(name).val.n;
                                    info.val.nn = elseBody.GetInfo(name).val.nn | ifBody.GetInfo(name).val.nn;
                                }
                            }

                        }
                        else
                        {
                            var info = context.GetInfo(name);
                            if (tests != null && tests.Item1.Any(x => x.Value))
                            {
                                info.val.n |= ifBody.GetInfo(name).val.n;
                                info.val.nn |= ifBody.GetInfo(name).val.nn;
                            }
                            if (tests != null && tests.Item1.Any(x => !x.Value))
                            {
                                info.val.n |= elseBody.GetInfo(name).val.n;
                                info.val.nn |= elseBody.GetInfo(name).val.nn;
                            }
                        }
                    }

                    if (tests != null && tests.Item1.Any(x => x.Value))
                    {
                        AddUniqNames(context, ifBody);
                    }

                    if (tests != null && tests.Item1.Any(x => !x.Value))
                    {
                        AddUniqNames(context, elseBody);
                    }
                    if (tests == null)
                    {
                        foreach (var v in context.AllNames(context.variables))
                        {

                            context.GetInfo(v).val.n |= elseBody.GetInfo(v).val.n | ifBody.GetInfo(v).val.n;
                            context.GetInfo(v).val.nn |= elseBody.GetInfo(v).val.nn | ifBody.GetInfo(v).val.nn;
                        }

                        AddUniqNames(context, ifBody);
                        AddUniqNames(context, elseBody);
                    }

                    continue;
                }

                ReportNode(context, item, analysisContext);
                if (item.IsKind(SyntaxKind.LocalDeclarationStatement))
                {
                    string name = item.DescendantNodesAndSelf().OfType<VariableDeclaratorSyntax>().First().Identifier.Text;
                    context.Add(name, new ValueSuggestion() { n = false, nn = true, beyond = true });
                }
                if (item.IsKind(SyntaxKind.ExpressionStatement))
                {
                    var assigments = item.DescendantNodesAndSelf().Where(x => x.IsKind(SyntaxKind.SimpleAssignmentExpression));
                    if (assigments.Count() == 1)
                    {

                        var n = assigments.Single();
                        var leftName = ((AssignmentExpressionSyntax)n).Left.GetText().ToString().Trim();
                        if (((AssignmentExpressionSyntax)n).Right.Kind() == SyntaxKind.ObjectCreationExpression)
                        {
                            context.GetInfo(leftName).val.n = false;
                            context.GetInfo(leftName).val.nn = true;
                        }
                        else if (((AssignmentExpressionSyntax)n).Right.Kind() == SyntaxKind.NullLiteralExpression)
                        {
                            context.GetInfo(leftName).val.n = true;
                            context.GetInfo(leftName).val.nn = false;
                        }
                        else
                        {
                            var txt = ((AssignmentExpressionSyntax)n).Right.GetText().ToString().Trim();
                            if (context.GetInfo(txt) != null)
                            {
                                context.SetNameInfo(leftName, context.GetInfo(txt));
                            }
                        }
                    }

                }
            }
            return context;
        }

        private void AnalyzeMethod(SyntaxNodeAnalysisContext analysisContext)
        {

            MethodDeclarationSyntax method = (MethodDeclarationSyntax)analysisContext.Node;

            LocalContext context = new LocalContext();
            var parameters = method.ChildNodes().OfType<ParameterListSyntax>().Single().ChildNodes().OfType<ParameterSyntax>();
            foreach (var p in parameters)
            {
                context.Add(p.ChildTokens().Single(x => x.Kind() == SyntaxKind.IdentifierToken).ValueText, new ValueSuggestion() { n = false, nn = true, beyond = true });
            }
            AnalyzeBody(method.Body, context, analysisContext);
        }
    }
}
