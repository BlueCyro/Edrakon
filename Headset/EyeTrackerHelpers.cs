using Edrakon.Wrappers;
using Silk.NET.Core;
using Silk.NET.OpenXR;

namespace Edrakon.Headsets;

public static class EyeTrackerHelpers
{
    public static bool SupportsEyeTracking(this XRInstance instance) => (Bool32)instance.GetRawProperty<SystemEyeTrackingPropertiesFB>().SupportsEyeTracking;
}