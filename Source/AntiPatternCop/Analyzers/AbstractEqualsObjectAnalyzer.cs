using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace AntiPatternCop.Analyzers
{
    public abstract class AbstractEqualsObjectAnalyzer : DiagnosticAnalyzer
    {
        public const string MessageId = "APC0001";

        internal static DiagnosticDescriptor Descriptor
            = new DiagnosticDescriptor(MessageId,
                "Don't use Equals(object)",
                "Use strongly typed equality on object.",
                "AntiPatternCop.CodeQuality",
                DiagnosticSeverity.Warning,
                true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {
                var objectType = compilationContext.Compilation.GetSpecialType(SpecialType.System_Object);
                if (objectType is null)
                    return;

                var equalsMethods = objectType.GetMembers(nameof(Equals)).OfType<IMethodSymbol>();
                var instanceEqualsMethod = equalsMethods.FirstOrDefault(m => m is { DeclaredAccessibility: Accessibility.Public, IsStatic: false, Parameters: { Length: 1 } });
                var staticEqualsMethod = equalsMethods.FirstOrDefault(m => m is { DeclaredAccessibility: Accessibility.Public, IsStatic: true, Parameters: { Length: 2 } });
                if (instanceEqualsMethod is null || staticEqualsMethod is null)
                    return;

                compilationContext.RegisterOperationAction(c => AnalyzeInvocation(c, instanceEqualsMethod, staticEqualsMethod), OperationKind.Invocation);
            });
        }

        protected abstract SyntaxNode GetMethodName(IInvocationOperation invocation);

        private void AnalyzeInvocation(OperationAnalysisContext context, IMethodSymbol instanceEqualsMethod, IMethodSymbol staticEqualsMethod)
        {
            var invocation = (IInvocationOperation)context.Operation;

            if ((invocation.TargetMethod.OverriddenMethod ?? invocation.TargetMethod).Equals(instanceEqualsMethod)
                || invocation.TargetMethod.Equals(staticEqualsMethod))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(Descriptor,
                    GetMethodName(invocation).GetLocation(),
                    additionalLocations: new[] { invocation.Syntax.GetLocation() }));
            }
        }
    }
}
