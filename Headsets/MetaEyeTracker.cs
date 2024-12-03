using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Edrakon.Helpers;
using Edrakon.Logging;
using Edrakon.Wrappers;
using Silk.NET.Core;
using Silk.NET.OpenXR;

namespace Edrakon.Headsets;

public class MetaEyeTracker : IDisposable
{
    public Action<object?, int>? Logger;

    public const string EYE_INTERACTION_PROFILE = "/interaction_profiles/ext/eye_gaze_interaction";
    public const string EYE_ACTION_PATH         = "/user/eyes_ext/input/gaze_ext/pose";


    public readonly XRInstance Instance;
    public readonly XRSession Session;
    public readonly XRSpace Space;
    public readonly XRSpace ViewSpace;
    public readonly XRActionSet Actions;
    public readonly XRAction<PosefAction> EyeAction;



    private Posef lastGaze;

    public MetaEyeTracker(XRInstance instance, XRSession session)
    {
        // if (!instance.SupportsEyeTracking())
        //     throw new NotSupportedException($"Eye tracking for this device isn't supported on the current OpenXR runtime.");


        Instance = instance;
        Session = session;
        ViewSpace = session.CreateReferenceSpace(spaceType: ReferenceSpaceType.View);
        Actions = instance.CreateActionSet("edrakon", "Edrakon");
        EyeAction = Actions.CreateAction<PosefAction>("eye_gaze", "Eye Gaze");
        Space = EyeAction.CreateSpace(session);

        instance.SuggestInteractionProfileBindings(
            EYE_INTERACTION_PROFILE,
            [
                new(EyeAction, EYE_ACTION_PATH)
            ]);

        session.AttachActionSets(Actions);
    }


    public bool GetEyeGazes(out Posef eyes)
    {
        long time = Instance.Time;
        SpaceLocation spaceLocation = Space.LocateSpace(ViewSpace, time);
        SpaceLocationFlags flags = spaceLocation.LocationFlags;

        bool isValid = flags.HasFlag(SpaceLocationFlags.OrientationValidBit) && flags.HasFlag(SpaceLocationFlags.PositionValidBit);

        if (!isValid)
        {
            Log($"Eye transforms are not valid: {flags}", 1);
            eyes = lastGaze;
            return false;
        }

        // Log($"Time: {time}, Pose: (Position: {spaceLocation.Pose.Position.ToNumeric()}, Rotation: {spaceLocation.Pose.Orientation.ToNumeric()})");
        eyes = spaceLocation.Pose;
        lastGaze = eyes;
        return true;
    }

    private void Log(object? message = null, int logLevel = 0) => Logger?.Invoke(message, logLevel);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}


public delegate Result XrCreateEyeTrackerFB(Session session, ref EyeTrackerCreateInfoFB createInfo, ref EyeTrackerFB eyeTracker);
public delegate Result XrDestroyEyeTrackerFB(EyeTrackerFB eyeTracker);
public delegate Result XrGetEyeGazesFB(EyeTrackerFB eyeTracker, ref EyeGazesInfoFB gazeInfo, ref EyeGazesFB eyeGazes);


public static class EyeTrackerHelpers
{
    public static bool SupportsEyeTracking(this XRInstance instance) => (Bool32)instance.GetRawProperty<SystemEyeTrackingPropertiesFB>().SupportsEyeTracking;
}