using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.Core;
using Silk.NET.OpenXR;

namespace Edrakon;

public static class XRPfnHelpers
{
    public static unsafe PtrFuncTyped<T> GetXRFunction<T>(XR xr, Instance inst, string funcName) where T : Delegate
    {
        Span<byte> funcNameBytes = stackalloc byte[funcName.Length];
        Encoding.UTF8.GetBytes(funcName, funcNameBytes);
        PfnVoidFunction pfn = new();
        xr.GetInstanceProcAddr(inst, in MemoryMarshal.GetReference(funcNameBytes), ref pfn);
        return new(pfn);
    }
}



public readonly unsafe struct PtrFuncTyped<T>(PfnVoidFunction pfn) : IDisposable
    where T : Delegate
{
    public T Call { get; } = Marshal.GetDelegateForFunctionPointer<T>((nint)pfn.Handle);

    public void Dispose()
    {
        pfn.Dispose();
    }
}
