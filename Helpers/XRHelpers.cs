using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.Core.Native;
using Silk.NET.OpenXR;

namespace Edrakon.Helpers;

public static class XRHelpers
{
    internal static readonly ImmutableDictionary<Type, StructureType> StructureTypeLookup;

    static XRHelpers()
    {
        Type[] xrTypes;
        Dictionary<Type, StructureType> dict = [];
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

                    // Console.WriteLine($"Found structure '{curType}' with default structure type of: {defaultType}");
                    dict.Add(curType, defaultType);

                    break;
                }
            }
        }

        StructureTypeLookup = dict.ToImmutableDictionary();
    }


    public static unsafe Result GetExtensionCount(this XR xr, out uint count)
    {
        Unsafe.SkipInit(out count);
        var result = xr.EnumerateInstanceExtensionProperties((byte*)null, 0, ref count, (ExtensionProperties*)null);
        return result;
    }


    public static unsafe Result GetExtensions(this XR xr, ref uint count, Span<ExtensionProperties> props) => xr.EnumerateInstanceExtensionProperties((byte*)null, ref count, props);



    public static T GetPropertyStruct<T>() where T : unmanaged
    {
        T prop = new();

        if (StructureTypeLookup.TryGetValue(typeof(T), out StructureType type))
            Unsafe.As<T, StructureType>(ref prop) = type;
        else
            throw new NotSupportedException($"Unsupported property structure: {typeof(T)}");

        return prop;
    }



    public static void ThrowIfNotSuccess(this Result result, string? message = null)
    {
        if (result != Result.Success)
            throw new XRException($"Result failed: {result}{(message != null ? $", Message: {message}" : "")}");
    }



    public static unsafe string GetRuntimeName(this in InstanceProperties props)
    {
        fixed (byte* ptr = props.RuntimeName)
            return Marshal.PtrToStringAnsi((nint)ptr)!;
    }


    public static int Utf8Count(this string str) => Encoding.UTF8.GetByteCount(str) + 1;


    public static int AsUtf8(this string str, Span<byte> bytes) => Encoding.UTF8.GetBytes(str, bytes);


    
}
