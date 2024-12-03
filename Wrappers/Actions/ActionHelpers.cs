using System.Runtime.CompilerServices;
using Edrakon.Structs;
using Action = Silk.NET.OpenXR.Action;

namespace Edrakon.Wrappers;

public static class ActionHelpers
{
    public static ref Action ToSilk<T>(this ref T typedAction) where T : unmanaged, IXRAction
    {
        return ref Unsafe.As<T, Action>(ref typedAction);
    }
}