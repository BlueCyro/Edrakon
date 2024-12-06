// #define DEBUGLOGGING
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Silk.NET.OpenXR;

namespace OpenXRStructTypeGenerator;


#pragma warning disable RSEXPERIMENTAL002 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

public static class Helpers
{
#pragma warning disable RS1035 // Do not use APIs banned for analyzers

    #if DEBUGLOGGING
    public static StreamWriter? file;
    public const string logPath = "/home/cyro/Documents/Coding Stuff/Edrakon/Edrakon/Generators/LOG.txt";
    #endif
    static Helpers()
    {
        #if DEBUGLOGGING
        File.Delete(logPath);
        file = File.AppendText(logPath);
        #endif
    }
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
    public static FrozenDictionary<string, StructureType> StructureLookups = Execute().ToFrozenDictionary();

    public static bool Filter(SyntaxNode node, CancellationToken token)
    {
        return node is InvocationExpressionSyntax;
    }


    public static void Execute(SourceProductionContext context, ImmutableArray<(InterceptableLocation location, ITypeSymbol genericType)?> types)
    {
        var toNotNullable = types.Cast<(InterceptableLocation location, ITypeSymbol genericType)>();

        StringBuilder sb = new();

        sb.AppendLine(CreateFileInterceptorAttribute());
        sb.AppendLine(CreateGenericFallback(toNotNullable));

        context.AddSource($"XRStructHelper.gen.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    public static (InterceptableLocation location, ITypeSymbol genericType)? Transform(GeneratorSyntaxContext context)
    {
        SemanticModel model = context.SemanticModel;

        if (context.Node is not InvocationExpressionSyntax invocation)
            return null;

        var info = model.GetSymbolInfo(invocation);
        if (info.Symbol is not IMethodSymbol methodSymbol)
            return null;

        if (methodSymbol.Name != "Get")
            return null;

        var memberAccess = methodSymbol.ContainingType;
        if (memberAccess == null || !(memberAccess?.Name == "XRStructHelper"))
            return null;

        if (model.GetInterceptableLocation(invocation) is InterceptableLocation pos && methodSymbol.TypeArguments.FirstOrDefault() is ITypeSymbol typeArgs)
        {
            Helpers.WriteLine($"Invocation: {invocation}, Symbol: {methodSymbol.GetType()}, Name: {methodSymbol.Name}, Containing Type: {methodSymbol.ContainingType}");
            return (pos, typeArgs);
        }

        return null;
    }



    public static string CreateFileInterceptorAttribute()
    {
        return
$@"namespace System.Runtime.CompilerServices
{{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    file sealed class InterceptsLocationAttribute(int version, string data) : Attribute
    {{
        public readonly int Version = version;
        public readonly string Data = data;
    }}
}}
";
    }



    public static string CreateGenericFallback(IEnumerable<(InterceptableLocation location, ITypeSymbol genericType)> usages)
    {
        (InterceptableLocation location, ITypeSymbol genericType) = usages.First();
        var typeName = genericType.Name;
        string globalTypeName = $"global::{genericType.ContainingNamespace}.{typeName}";
        return
$@"namespace Edrakon.Helpers
{{
    public static partial class XRStructHelper
    {{
        #pragma warning disable CS0618 // Disable obsolete warning.

        [global::System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        {string.Join("\n        ", usages.Select(t => t.location.GetInterceptsLocationAttributeSyntax()))}
        public static T InterceptedGet<T>() where T : unmanaged
        {{
            T newStruct = new();

            switch (newStruct)
            {{
{string.Join("\n", StructureLookups.Select(propStruct =>
                {
                    return$@"
                case global::Silk.NET.OpenXR.{propStruct.Key} prop{propStruct.Key}:
                    prop{propStruct.Key}.Type = global::Silk.NET.OpenXR.StructureType.{propStruct.Value};
                    return global::System.Runtime.CompilerServices.Unsafe.As<global::Silk.NET.OpenXR.{propStruct.Key}, T>(ref prop{propStruct.Key});
                ";
                }))}
                default:
                    throw new global::System.NotImplementedException($""Cannot get property for unimplemented struct type: {{typeof(T)}}"");
            }}
        }}

        #pragma warning restore CS0618 // Restore obsolete warning.
    }}
}}";
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


    public static void WriteLine(string msg)
    {
        #if DEBUGLOGGING
        file?.WriteLine(msg);
        file?.Flush();
        #endif
    }
}

#pragma warning restore RSEXPERIMENTAL002 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.