using System.Runtime.InteropServices;
using Silk.NET.OpenXR;

namespace Edrakon.Helpers;

public static class ExtensionPropertiesHelpers
{
    public static unsafe string GetExtensionName(this in ExtensionProperties props)
    {
        fixed (byte* ptr = props.ExtensionName)
            return Marshal.PtrToStringAnsi((nint)ptr)!;
    }
}