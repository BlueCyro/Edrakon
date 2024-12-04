using System.Numerics;
using Edrakon.Helpers;
using Edrakon.Structs;
using Silk.NET.OpenXR;

namespace Edrakon.Wrappers;

public class XRSession : IDisposable
{
    public readonly XR XR;
    public readonly XRInstance Instance;
    internal readonly Session session;


    public XRSession(XR xr, XRInstance instance, XRSystem system)
    {
        XR = xr;
        Instance = instance;
        SessionCreateInfo seshInfo = XRStructHelper<SessionCreateInfo>.Get();
        seshInfo.SystemId = system.SysID;
        xr.CreateSession(instance.instance, ref seshInfo, ref session);
    }

    public void Begin(ViewConfigurationType viewConfigurationType = ViewConfigurationType.None)
    {
        SessionBeginInfo beginInfo = XRStructHelper<SessionBeginInfo>.Get();
        beginInfo.PrimaryViewConfigurationType = viewConfigurationType;

        XR.BeginSession(session, ref beginInfo);
    }


    public void End() => XR.EndSession(session);

    public unsafe FrameState WaitFrame()
    {
        FrameState frameState = XRStructHelper<FrameState>.Get();

        // FrameWaitInfo is ALWAYS null currently. This is for extensibility purposes.
        XR.WaitFrame(session, (FrameWaitInfo*)null, ref frameState).ThrowIfNotSuccess();
        return frameState;
    }


    public PtrFuncTyped<T> GetXRFunction<T>(string funcName) where T : Delegate => XRPfnHelpers.GetXRFunction<T>(XR, Instance.instance, funcName);



    public XRSpace CreateReferenceSpace(Vector3 position = default, Quaternion orientation = default, ReferenceSpaceType spaceType = ReferenceSpaceType.Local)
    {
        if (orientation == default)
            orientation = Quaternion.Identity;
        
        return new(XR, this, position, orientation, spaceType);
    }



    public unsafe void AttachActionSets(params Span<XRActionSet> actionSets)
    {
        SessionActionSetsAttachInfo attachInfo = XRStructHelper<SessionActionSetsAttachInfo>.Get();

        Span<ActionSet> sets = stackalloc ActionSet[actionSets.Length];

        for (int i = 0; i < actionSets.Length; i++)
            sets[i] = actionSets[i].actionSet;


        fixed (ActionSet* setsPtr = sets)
        {
            attachInfo.ActionSets = setsPtr;
            attachInfo.CountActionSets = (uint)actionSets.Length;
            XR.AttachSessionActionSets(session, ref attachInfo);
        }
    }

    public ActionStatePose GetActionStatePose(XRAction<PosefAction> action, string? subPath = null)
    {
        ActionStateGetInfo getInfo = XRStructHelper<ActionStateGetInfo>.Get();

        unsafe
        {
            if (subPath != null)
            {
                StackString256 subPathBytes = new(subPath);
                getInfo.SubactionPath = (ulong)&subPathBytes;
            }
        }

        ActionStatePose pose = XRStructHelper<ActionStatePose>.Get();
        XR.GetActionStatePose(session, ref getInfo, ref pose).ThrowIfNotSuccess($"Could not get action state for '{action.Name}'");

        return pose;
    }



    public void Dispose()
    {
        GC.SuppressFinalize(this);

        XR.DestroySession(session);
    }
}
