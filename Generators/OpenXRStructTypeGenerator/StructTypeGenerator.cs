using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Silk.NET.OpenXR;

namespace OpenXRStructTypeGenerator;


[Generator]
#pragma warning disable RS1036 // Specify analyzer banned API enforcement setting
public class StructTypeGenerator : IIncrementalGenerator
#pragma warning restore RS1036
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("XRHelpers.g.cs", SourceText.From(Helpers.CreateLookups(), Encoding.UTF8)));

        // context.SyntaxProvider.CreateSyntaxProvider(predicate: (c, _) => true, transform: (ctx, _) => true);

        // var synProvider = context.SyntaxProvider.CreateSyntaxProvider(
        //     predicate: Helpers.Filter,
        //     transform: (ctx, _) => Helpers.Transform(ctx))
        // .Where(m => m is not null);

        // context.RegisterSourceOutput(synProvider, Helpers.Execute);
    }
}


public static class Helpers
{
    public static ImmutableDictionary<string, StructureType> StructureLookups = Execute().ToImmutableDictionary();

    public static bool Filter(SyntaxNode node, CancellationToken token)
    {
        return node is InvocationExpressionSyntax;
    }


    public static void Execute(SourceProductionContext context, StructureType? type)
    {
        context.AddSource($"Test.{type}.g.cs", SourceText.From("namespace Edrakon.Helpers;\npublic static partial class Test { public static partial T GetMyFunnyStruct<T>() where T : unmanaged => throw new NotImplementedException(); }", Encoding.UTF8));
    }

    public static StructureType? Transform(GeneratorSyntaxContext context)
    {
        SemanticModel model = context.SemanticModel;
        InvocationExpressionSyntax invocation = (InvocationExpressionSyntax)context.Node;

        var info = model.GetSymbolInfo(invocation);

        var symbol = info.Symbol as IMethodSymbol;

        if (symbol == null)
            return null;

        if (symbol.Name == "GetMyFunnyStruct")
            return StructureLookups[symbol.ReturnType.Name];
        
        return null;
    }


    public static string CreateLookups()
    {
        return $@"
namespace Edrakon.Helpers;

#pragma warning disable CS0618 // Disable obsolete warning
public static partial class XRStructHelper<T> where T : unmanaged
{{
    private static readonly T propStruct;

    static XRStructHelper()
    {{
        propStruct = new();
        switch (propStruct)
        {{
{string.Join("\n\n", StructureLookups.Select(lookup =>
{
    string typeName = $"global::Silk.NET.OpenXR.{lookup.Key}";
    string label = $"new{lookup.Key}";
    string structureType = $"global::Silk.NET.OpenXR.StructureType.{lookup.Value}";

    return $@"
            case {typeName}:
                global::System.Runtime.CompilerServices.Unsafe.As<T, global::Silk.NET.OpenXR.StructureType>(ref propStruct) = {structureType};
                break;
";}))}
            default:
                throw new NotImplementedException($""Could not get unimplemented property struct: {{typeof(T)}}"");

        }}
    }}
    public static partial T Get() => propStruct;
}}
#pragma warning restore CS0618 // Restore obsolete warning
";
    }


    public static Dictionary<string, StructureType> Execute()
    {
        Type[] xrTypes;
        Dictionary<string, StructureType> dict = [];
        try
        {
            xrTypes = typeof(SystemProperties).Assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            xrTypes = ex.Types.Where(type => type != null).Cast<Type>().ToArray();
        }

        for (int i = xrTypes.Length - 1; i > 0; i--)
        {
            Type curType = xrTypes[i];

            // Only structs are desired
            if (!curType.IsValueType)
                continue;

            ConstructorInfo[] constructors = curType.GetConstructors();

            // No constructors? Continue to next type.
            if (constructors.Length == 0)
                continue;

            // Iterate all constructors
            for (int j = 0; j < constructors.Length; j++)
            {
                ConstructorInfo curInfo = constructors[j];
                ParameterInfo[] parameterInfos = curInfo.GetParameters();

                // No parameters? Continue to next constructor.
                if (parameterInfos.Length == 0)
                    break;

                // Get the first parameter in the current constructor
                ParameterInfo firstParam = parameterInfos[0];
                
                if (firstParam.IsOptional && firstParam.HasDefaultValue && firstParam.ParameterType == typeof(StructureType?))
                {
                    object? defaultValue = firstParam.DefaultValue;
                    StructureType defaultType = defaultValue == null ? StructureType.Unknown : (StructureType)defaultValue;
                    
                    if (!firstParam.ParameterType.GetMember(Enum.GetName(typeof(StructureType), defaultType)).Any(member => member.GetCustomAttributes().Any(attr => attr is ObsoleteAttribute)))
                        dict.Add(curType.Name, defaultType);
                    // Console.WriteLine($"Found structure '{curType}' with default structure type of: {defaultType}");

                    break;
                }
            }
        }

        return dict;
    }
}
