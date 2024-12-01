using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.Core;
using Silk.NET.OpenXR;

namespace Edrakon;

// public static class XRHelpers
// {
//     public const int XR_APP_NAME_LENGTH = 128;
//     public const int XR_ENGINE_NAME_LENGTH = 128;


//     public static unsafe InstanceCreateInfo CreateInfo => new(StructureType.InstanceCreateInfo);


//     // public static unsafe Result CreateInstance(XR xr, string appName, string engineName, Span<string> extensions, ref Instance instance)
//     // {
//     //     byte** extStrings = stackalloc byte*[extensions.Length];

//     //     for (int i = 0; i < extensions.Length; i++)
//     //     {
//     //         byte* curStr = stackalloc byte[extensions[i].Length];
//     //         Span<byte> curStrBytes = new(curStr, extensions[i].Length);
//     //         Encoding.UTF8.GetBytes(extensions[i], curStrBytes);
//     //         extStrings[i] = curStr;
//     //     }

//     //     InstanceCreateInfo info = CreateInfo;
//     //     info.EnabledExtensionCount = (uint)extensions.Length;
//     //     info.EnabledExtensionNames = extStrings;
//     //     info.ApplicationInfo.SetApplicationName(appName);
//     //     info.ApplicationInfo.SetEngineName(engineName);
//     //     info.ApplicationInfo.ApplicationVersion = new Version32(0, 0, 1);
//     //     info.ApplicationInfo.ApiVersion = new Version64(1, 0, 0);

//     //     Result result = xr.CreateInstance(in info, ref instance);
//     //     return result;
//     // }



//     public static void EnumerateApiLayerProperties(this Span<ApiLayerProperties> properties, XR xr)
//     {
//         for (int i = 0; i < properties.Length; i++)
//             properties[i].Type = StructureType.ApiLayerProperties;
        
//         uint count = (uint)properties.Length;
//         xr.EnumerateApiLayerProperties(ref count, properties);
//     }



//     public static unsafe string GetLayerName(this in ApiLayerProperties props)
//     {
//         fixed (byte* ptr = props.LayerName)
//         {
//             return Marshal.PtrToStringAnsi((nint)ptr)!;
//         }
//     }



//     public static unsafe string GetDescription(this in ApiLayerProperties props)
//     {
//         fixed (byte* ptr = props.Description)
//         {
//             return Marshal.PtrToStringAnsi((nint)ptr)!;
//         }
//     }






//     public static unsafe Result GetSystem(this Instance inst, XR xr, ref SystemGetInfo info, ref ulong systemID) => xr.GetSystem(inst, ref info, ref systemID);


//     public static unsafe string GetSystemName(this in SystemProperties props)
//     {
//         fixed (byte* ptr = props.SystemName)
//         {
//             return Marshal.PtrToStringAnsi((nint)ptr)!;
//         }
//     }



//     public static void ThrowIfNotSuccess(this Result result, string? message = null)
//     {
//         if (result != Result.Success)
//             throw new XRException($"Result failed: {result}{(message != null ? $", Message: {message}" : "")}");
//     }



//     public static void CheckExtensions(this IReadOnlyDictionary<string, ExtensionProperties> extensions, Span<string> extQueries)
//     {
//         for (int i = 0; i < extQueries.Length; i++)
//         {
//             if (!extensions.ContainsKey(extQueries[i]))
//                 throw new NotSupportedException($"The '{extQueries[i]}' OpenXR extension appears to be unsupported.");
//         }
//     }
// }