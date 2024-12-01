using System.Numerics;
using Edrakon.Helpers;
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
        SessionCreateInfo seshInfo = XRHelpers.GetPropertyStruct<SessionCreateInfo>();
        seshInfo.SystemId = system.SysID;
        xr.CreateSession(instance.instance, ref seshInfo, ref session);
    }

    public void Begin(ViewConfigurationType viewConfigurationType = ViewConfigurationType.None)
    {
        SessionBeginInfo beginInfo = XRHelpers.GetPropertyStruct<SessionBeginInfo>();
        beginInfo.PrimaryViewConfigurationType = viewConfigurationType;

        XR.BeginSession(session, ref beginInfo);
    }


    public void End() => XR.EndSession(session);

    public unsafe FrameState WaitFrame()
    {
        FrameState frameState = XRHelpers.GetPropertyStruct<FrameState>();

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

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        XR.DestroySession(session);
    }
}
