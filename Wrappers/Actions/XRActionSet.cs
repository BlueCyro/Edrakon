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

        ActionSetCreateInfo info = XRStructHelper<ActionSetCreateInfo>.Get();


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



    public XRAction<T> CreateAction<T>(string name, string localizedName, string[]? subActionpaths = null)
        where T : unmanaged, IXRAction
    {
        return new(XR, this, name, localizedName, subActionpaths);
    }
}
