using System.Numerics;
using System.Runtime.CompilerServices;
using Edrakon.Helpers;
using Edrakon.Structs;
using Silk.NET.OpenXR;

namespace Edrakon.Wrappers;

public class XRSpace : IDisposable
{
    public readonly XR XR;
    public readonly Space Space;


    public XRSpace(XR xr, XRSession session, Vector3 position, Quaternion orientation, ReferenceSpaceType spaceType)
    {
        XR = xr;

        ReferenceSpaceCreateInfo spaceInfo = XRStructHelper.Get<ReferenceSpaceCreateInfo>();

        Vector3f positionF = new(position.X, position.Y, position.Z);
        Quaternionf orientationF = new(orientation.X, orientation.Y, orientation.Z, orientation.W);

        spaceInfo.PoseInReferenceSpace = new(orientationF, positionF);
        spaceInfo.ReferenceSpaceType = spaceType;

        XR.CreateReferenceSpace(session.session, ref spaceInfo, ref Space).ThrowIfNotSuccess();
    }

    public XRSpace(XR xr, XRSession session, Silk.NET.OpenXR.Action action, Vector3 position, Quaternion orientation, string? path)
    {
        XR = xr;

        ActionSpaceCreateInfo spaceInfo = XRStructHelper.Get<ActionSpaceCreateInfo>();

        Vector3f positionF = new(position.X, position.Y, position.Z);
        Quaternionf orientationF = new(orientation.X, orientation.Y, orientation.Z, orientation.W);

        spaceInfo.PoseInActionSpace = new(orientationF, positionF);
        spaceInfo.Action = action;

        unsafe
        {
            if (path != null)
            {
                StackString256 pathBytes = new(path);
                spaceInfo.SubactionPath = (ulong)&pathBytes;
            }

            XR.CreateActionSpace(session.session, ref spaceInfo, ref Space).ThrowIfNotSuccess();
        }
    }


    public SpaceLocation LocateSpace(XRSpace other, long time)
    {
        SpaceLocation space = XRStructHelper.Get<SpaceLocation>();
        XR.LocateSpace(Space, other.Space, time, ref space).ThrowIfNotSuccess();
        return space;
    }


    public void Dispose()
    {
        GC.SuppressFinalize(this);
        XR.DestroySpace(Space);
    }
}