using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.VisualBasic.Testing;

namespace Deref_after_null_3.Test
{
    public static partial class VisualBasicAnalyzerVerifier<TAnalyzer>
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        public class Test : VisualBasicAnalyzerTest<TAnalyzer, MSTestVerifier>
        {
            public Test()
            {
            }
        }
    }
}
