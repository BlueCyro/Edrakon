using System.Numerics;
using Edrakon.Helpers;
using Edrakon.Structs;
using Silk.NET.OpenXR;
using Action = Silk.NET.OpenXR.Action;

namespace Edrakon.Wrappers;

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

        ActionCreateInfo info = XRStructHelper<ActionCreateInfo>.Get();


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
