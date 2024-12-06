using System.Diagnostics.CodeAnalysis;
using Silk.NET.Core;
using Silk.NET.OpenXR;

namespace Edrakon.Wrappers;

public class XRFaceTrackingProperties : XRPropertyBase<SystemFaceTrackingProperties2FB>
{

    [AllowNull]
    public XR XR { get; private set; }


    public bool SupportsVisualFaceTracking => (Bool32)properties.SupportsVisualFaceTracking;
    public bool SupportsAudioFaceTracking => (Bool32)properties.SupportsAudioFaceTracking;

    public override void Instantiate(XR xr, XRInstance instance)
    {
        XR = xr;
        base.Instantiate(xr, instance);
    }
}