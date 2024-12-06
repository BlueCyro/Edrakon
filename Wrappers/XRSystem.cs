using System.Diagnostics.CodeAnalysis;
using Edrakon.Helpers;
using Silk.NET.Core;
using Silk.NET.OpenXR;

namespace Edrakon.Wrappers;

public class XRSystem
{
    [AllowNull]
    public XR XR { get; private set; }
    public ulong SysID => sysID;

    public FormFactor FormFactor => sysInfo.FormFactor;


    public bool OrientationTracking => (Bool32)sysProps.TrackingProperties.OrientationTracking;
    public bool PositionTracking => (Bool32)sysProps.TrackingProperties.PositionTracking;
    public uint MaxSwapchainWidth => sysProps.GraphicsProperties.MaxSwapchainImageWidth;
    public uint MaxSwapchainHeight => sysProps.GraphicsProperties.MaxSwapchainImageHeight;


    internal SystemGetInfo sysInfo = XRStructHelper.Get<SystemGetInfo>();
    internal SystemProperties sysProps = XRStructHelper.Get<SystemProperties>();
    internal ulong sysID;
    internal readonly XRInstance instance;


    public XRSystem(XR xr, XRInstance inst)
    {
        XR = xr;
        instance = inst;
        sysInfo.FormFactor = FormFactor.HeadMountedDisplay; // TODO: Find a better way to define this
        xr.GetSystem(inst.instance, ref sysInfo, ref sysID).ThrowIfNotSuccess($"Could not get system info upon instantiation of: {nameof(XRSystem)}");
        xr.GetSystemProperties(inst.instance, sysID, ref sysProps).ThrowIfNotSuccess();
    }



    public T GetProperty<T>() where T : IXRPropertyBase, new()
    {
        T prop = new();
        prop.Instantiate(XR, instance);
        return prop;
    }
}

