using System.Numerics;
using System.Runtime.CompilerServices;
using Edrakon.Helpers;
using Edrakon.Structs;
using Edrakon.Wrappers;
using Silk.NET.OpenXR;
using Action = Silk.NET.OpenXR.Action;

namespace Edrakon.Wrappers;
public class XRActionSet
{
    public readonly XR XR;
    public readonly XRInstance Instance;


    internal readonly ActionSet actionSet;

    public XRActionSet(XR xr, XRInstance inst, string name, string localizedName, uint priority)
    {
        XR = xr;
        Instance = inst;

        ActionSetCreateInfo info = XRHelpers.GetPropertyStruct<ActionSetCreateInfo>();


        unsafe
        {
            Span<byte> nameBytes = new(info.ActionSetName, 64);
            Span<byte> localizedNameBytes = new(info.LocalizedActionSetName, 128);
            name.AsUtf8(nameBytes);
            localizedName.AsUtf8(localizedNameBytes);
        }

        info.Priority = priority;

        XR.CreateActionSet(Instance.instance, ref info, ref actionSet);
    }



    public XRAction<T> CreateAction<T>(string name, string localizedName, string[]? subActionpaths = null) where T : unmanaged, IXRAction
    {
        return new(XR, this, name, localizedName, subActionpaths);
    }
}


public class XRAction<T> : IXRAction
    where T : unmanaged, IXRAction
{
    public static ActionType Type => T.Type;
    public Action Action => action.Action;

    public readonly string Name;
    public readonly string LocalizedName;

    public readonly XR XR;
    public readonly XRActionSet Set;

    internal readonly T action;

    public XRAction(XR xr, XRActionSet set, string name, string localizedName, string[]? subActionPaths = null)
    {
        XR = xr;
        Set = set;
        Name = name;
        LocalizedName = name;

        ActionCreateInfo info = XRHelpers.GetPropertyStruct<ActionCreateInfo>();


        unsafe
        {
            info.ActionType = Type;
            Span<byte> nameBytes = new(info.ActionName, 64);
            Span<byte> localizedNameBytes = new(info.LocalizedActionName, 128);
            name.AsUtf8(nameBytes);
            localizedName.AsUtf8(localizedNameBytes);

            if (subActionPaths != null)
            {
                ulong* paths = stackalloc ulong[subActionPaths.Length];


                for (int i = 0; i < subActionPaths.Length; i++)
                {
                    paths[i] = set.Instance.StringToPath(subActionPaths[i]);
                }

                info.SubactionPaths = paths;
                info.CountSubactionPaths = (uint)subActionPaths.Length;
            }
            
        }
        ref Action baseAction = ref action.ToSilk();

        XR.CreateAction(Set.actionSet, ref info, ref baseAction).ThrowIfNotSuccess();
    }




    public XRSpace CreateSpace(XRSession session, Vector3 position = default, Quaternion orientation = default, string? path = null)
    {
        if (orientation == default)
            orientation = Quaternion.Identity;
        
        return new XRSpace(XR, session, action.Action, position, orientation, path);
    }
}

public readonly struct XRActionSuggestedBinding(IXRAction action, string binding)
{
    public readonly IXRAction Action = action;
    public readonly string Binding = binding;
}


public readonly struct BooleanAction : IXRAction
{
    public static ActionType Type => ActionType.BooleanInput;
    public readonly Action Action { get; }
}

public readonly struct FloatAction : IXRAction
{
    public static ActionType Type => ActionType.FloatInput;
    public readonly Action Action { get; }
}

public readonly struct Vector2fAction : IXRAction
{
    public static ActionType Type => ActionType.Vector2fInput;
    public readonly Action Action { get; }
}

public readonly struct PosefAction : IXRAction
{
    public static ActionType Type => ActionType.PoseInput;
    public readonly Action Action { get; }
}

public readonly struct VibrationAction : IXRAction
{
    public static ActionType Type => ActionType.VibrationOutput;
    public readonly Action Action { get; }
}



public interface IXRAction
{
    static abstract ActionType Type { get; }
    Action Action { get; }
}


public static class ActionHelpers
{
    public static ref Action ToSilk<T>(this ref T typedAction) where T : unmanaged, IXRAction
    {
        return ref Unsafe.As<T, Action>(ref typedAction);
    }
}