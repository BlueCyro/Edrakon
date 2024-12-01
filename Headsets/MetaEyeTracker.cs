using Edrakon.Helpers;
using Edrakon.Wrappers;
using Silk.NET.Core;
using Silk.NET.OpenXR;

namespace Edrakon.Headsets;

public class MetaEyeTracker : IDisposable
{
    public readonly XRInstance Instance;
    public readonly XRSession Session;
    public readonly XRSpace Space;


    private readonly PtrFuncTyped<XrCreateEyeTrackerFB> xrCreateEyeTrackerFB;
    private readonly PtrFuncTyped<XrDestroyEyeTrackerFB> xrDestroyEyeTrackerFB;
    private readonly PtrFuncTyped<XrGetEyeGazesFB> xrGetEyeGazesFB;


    private readonly EyeTrackerFB eyeTracker;



    public MetaEyeTracker(XRInstance instance, XRSession session)
    {
        xrCreateEyeTrackerFB = instance.GetXRFunction<XrCreateEyeTrackerFB>(nameof(xrCreateEyeTrackerFB));
        xrDestroyEyeTrackerFB = instance.GetXRFunction<XrDestroyEyeTrackerFB>(nameof(xrDestroyEyeTrackerFB));
        xrGetEyeGazesFB = instance.GetXRFunction<XrGetEyeGazesFB>(nameof(xrGetEyeGazesFB));


        Instance = instance;
        Session = session;
        Space = session.CreateReferenceSpace();


        if (!instance.SupportsEyeTracking())
            throw new NotSupportedException($"Eye tracking for this device isn't supported on the current OpenXR runtime.");

        EyeTrackerCreateInfoFB eyeCreateInfo = XRHelpers.GetPropertyStruct<EyeTrackerCreateInfoFB>();
        xrCreateEyeTrackerFB.Call(session.session, ref eyeCreateInfo, ref eyeTracker).ThrowIfNotSuccess();
    }


    public void GetEyeGazes(out EyeGazeFB left, out EyeGazeFB right)
    {
        EyeGazesInfoFB eyeGazeInfo = XRHelpers.GetPropertyStruct<EyeGazesInfoFB>();
        eyeGazeInfo.BaseSpace = Space.Space;
        eyeGazeInfo.Time = Session.WaitFrame().PredictedDisplayTime;

        EyeGazesFB gazes = XRHelpers.GetPropertyStruct<EyeGazesFB>();
        xrGetEyeGazesFB.Call(eyeTracker, ref eyeGazeInfo, ref gazes).ThrowIfNotSuccess();

        
        left = gazes.Gaze.Element0;
        right = gazes.Gaze.Element1;
    }


    public void Dispose()
    {
        GC.SuppressFinalize(this);

        xrDestroyEyeTrackerFB.Call(eyeTracker);

        xrCreateEyeTrackerFB.Dispose();
        xrDestroyEyeTrackerFB.Dispose();
        xrGetEyeGazesFB.Dispose();
    }
}


public delegate Result XrCreateEyeTrackerFB(Session session, ref EyeTrackerCreateInfoFB createInfo, ref EyeTrackerFB eyeTracker);
public delegate Result XrDestroyEyeTrackerFB(EyeTrackerFB eyeTracker);
public delegate Result XrGetEyeGazesFB(EyeTrackerFB eyeTracker, ref EyeGazesInfoFB gazeInfo, ref EyeGazesFB eyeGazes);


public static class EyeTrackerHelpers
{
    public static bool SupportsEyeTracking(this XRInstance instance) => (Bool32)instance.GetRawProperty<SystemEyeTrackingPropertiesFB>().SupportsEyeTracking;
}