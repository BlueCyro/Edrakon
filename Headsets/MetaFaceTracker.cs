using System.Numerics;
using System.Reflection;
using Edrakon.Helpers;
using KoboldOSC.Messages;
using KoboldOSC.Structs;
using Edrakon.Wrappers;
using Silk.NET.Core;
using Silk.NET.OpenXR;

namespace Edrakon.Headsets;

public class MetaFaceTracker : IDisposable
{
    public readonly XRInstance Instance;
    public readonly XRSession Session;

    public FaceExpressionSet2FB ExpressionSet => FaceExpressionSet2FB.DefaultFB;
    public static readonly int ExpressionCount = Enum.GetValues<FaceExpression2FB>().Cast<FaceExpression2FB>().Distinct().Count();


    private PtrFuncTyped<XrCreateFaceTracker2FB> xrCreateFaceTracker2FB;
    private PtrFuncTyped<XrDestroyFaceTracker2FB> xrDestroyFaceTracker2FB;
    private PtrFuncTyped<XrGetFaceExpressionWeights2FB> xrGetFaceExpressionWeights2FB;
    

    private FaceTracker2FB tracker;
    private MetaFaceInfo faceInfo;
    private FaceExpressionInfo2FB expressionInfo = XRHelpers.GetPropertyStruct<FaceExpressionInfo2FB>();


    public MetaFaceTracker(XRInstance instance, XRSession session)
    {
        Instance = instance;
        Session = session;
    }


    public void Initialize()
    {
        xrCreateFaceTracker2FB = Instance.GetXRFunction<XrCreateFaceTracker2FB>(nameof(xrCreateFaceTracker2FB));
        xrDestroyFaceTracker2FB = Instance.GetXRFunction<XrDestroyFaceTracker2FB>(nameof(xrDestroyFaceTracker2FB));
        xrGetFaceExpressionWeights2FB = Instance.GetXRFunction<XrGetFaceExpressionWeights2FB>(nameof(xrGetFaceExpressionWeights2FB));



        FaceTrackerCreateInfo2FB faceTrackerInfo = XRHelpers.GetPropertyStruct<FaceTrackerCreateInfo2FB>();
        FaceTrackingDataSource2FB dataSource = FaceTrackingDataSource2FB.VisualFB;  // Only support visual right now.
        faceTrackerInfo.FaceExpressionSet = ExpressionSet;                          // This is ALWAYS DefaultFB currently.
        faceTrackerInfo.RequestedDataSourceCount = 1;
        unsafe { faceTrackerInfo.RequestedDataSources = &dataSource; }              // Pass in data source by ref (technically an array of 1 value)

        xrCreateFaceTracker2FB.Call(Session.session, ref faceTrackerInfo, ref tracker).ThrowIfNotSuccess();
    }


    public ref MetaFaceInfo GetBlendshapes()
    {
        // Init expression weights
        FaceExpressionWeights2FB expressionWeights = XRHelpers.GetPropertyStruct<FaceExpressionWeights2FB>();

        // Set the weight count and confidence count.
        expressionWeights.WeightCount = (uint)ExpressionCount;
        expressionWeights.ConfidenceCount = (int)FaceConfidence2FB.CountFB; // This is ALWAYS CountFB currently.

        unsafe
        {
            // Populate the weights field with a pointer to the span and get the face blendshapes.
            fixed (MetaFaceConfidences* confPtr = &faceInfo.FaceConfidences)
            fixed (MetaFaceParameters* weightsPtr = &faceInfo.FaceParameters)
            {
                expressionWeights.Weights = (float*)weightsPtr;
                expressionWeights.Confidences = (float*)confPtr;
                xrGetFaceExpressionWeights2FB.Call(tracker, ref expressionInfo, ref expressionWeights).ThrowIfNotSuccess("Failed to get expression weights.");
            }
        }

        // Blow up if it's not valid.
        // if (!(Bool32)expressionWeights.IsValid)
        //     throw new InvalidDataException($"Expression weights were not valid!!!");
        
        return ref faceInfo;
    }

    
    public void Dispose()
    {
        GC.SuppressFinalize(this);

        xrDestroyFaceTracker2FB.Call(tracker);

        xrCreateFaceTracker2FB.Dispose();
        xrDestroyFaceTracker2FB.Dispose();
        xrGetFaceExpressionWeights2FB.Dispose();
    }
}


public interface IParameterStruct
{
    public static abstract OSCInfo[] PathInfo { get; }
    public static abstract int BundleCount { get; }
}





public readonly struct OSCInfo(OSCString path, int bundleId = -1)
{
    public readonly int BundleId;
    public readonly OSCString path;
}


public static class MetaFaceTrackerHelpers
{
    public static void WriteVector(this KOscMessage msg, in Vector3 vec)
    {
        msg.WriteFloat(vec.X, vec.Y, vec.Z);
    }
}



public delegate Result XrCreateFaceTracker2FB(Session session, ref FaceTrackerCreateInfo2FB info, ref FaceTracker2FB faceTracker);
public delegate Result XrGetFaceExpressionWeights2FB(FaceTracker2FB faceTracker, ref FaceExpressionInfo2FB expressionInfo, ref FaceExpressionWeights2FB expressionWeights);
public delegate Result XrDestroyFaceTracker2FB(FaceTracker2FB faceTracker);