using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace OpenXRStructTypeGenerator;


[Generator]
#pragma warning disable RS1036 // Specify analyzer banned API enforcement setting
public class StructTypeGenerator : IIncrementalGenerator
#pragma warning restore RS1036
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // context.RegisterPostInitializationOutput(ctx => ctx.AddSource("XRHelpers.g.cs", SourceText.From(Helpers.CreateLookups(), Encoding.UTF8)));

        context.SyntaxProvider.CreateSyntaxProvider(predicate: (c, _) => true, transform: (ctx, _) => true);

        var synProvider = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: Helpers.Filter,
            transform: (ctx, _) => Helpers.Transform(ctx))
        .Where(m => m is not null)
        .Collect();

        context.RegisterSourceOutput(synProvider, Helpers.Execute);
    }
}
