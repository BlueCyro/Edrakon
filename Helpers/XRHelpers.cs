using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.OpenXR;

namespace Edrakon.Helpers;


public static partial class XRHelpers
{
    public static unsafe Result GetExtensionCount(this XR xr, out uint count)
    {
        Unsafe.SkipInit(out count);
        var result = xr.EnumerateInstanceExtensionProperties((byte*)null, 0, ref count, (ExtensionProperties*)null);
        return result;
    }

    public static unsafe Result GetExtensions(this XR xr, ref uint count, Span<ExtensionProperties> props) => xr.EnumerateInstanceExtensionProperties((byte*)null, ref count, props);

    public static partial T GetPropertyStruct<T>() where T : unmanaged;

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