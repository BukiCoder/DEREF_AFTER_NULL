using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Deref_after_null_3
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class Deref_after_null_3Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "Deref_after_null";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "_";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        private readonly SyntaxKind[] nodesNeededToAnalyze = new SyntaxKind[]
        {
            SyntaxKind.MethodDeclaration,
            SyntaxKind.ConstructorDeclaration,
            SyntaxKind.DestructorDeclaration,
            SyntaxKind.OperatorDeclaration,
            SyntaxKind.IndexerDeclaration,
            SyntaxKind.ConversionOperatorDeclaration,
            SyntaxKind.AddAccessorDeclaration,
            SyntaxKind.GetAccessorDeclaration,
            SyntaxKind.InitAccessorDeclaration,
            SyntaxKind.RemoveAccessorDeclaration,
            SyntaxKind.SetAccessorDeclaration,
            SyntaxKind.UnknownAccessorDeclaration
        };
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            foreach (var kind in nodesNeededToAnalyze)
            {
                context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, kind);
            }
        }

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            Walker walker = new Walker();
            walker.AnalysisContext = context;
            walker.Context = new LocalContext();
            walker.Rule = Rule;
            walker.Semantic = context.SemanticModel;
            walker.Visit(context.Node);
        }
    }
}
