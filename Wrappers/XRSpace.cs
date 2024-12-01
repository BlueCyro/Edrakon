using System.Numerics;
using Edrakon.Helpers;
using Silk.NET.OpenXR;

namespace Edrakon.Wrappers;

public class XRSpace
{
    public readonly XR XR;
    public readonly XRSession Session;
    public readonly Space Space;


    public XRSpace(XR xr, XRSession session, Vector3 position, Quaternion orientation, ReferenceSpaceType spaceType)
    {
        XR = xr;
        Session = session;

        ReferenceSpaceCreateInfo refSpaceInfo = XRHelpers.GetPropertyStruct<ReferenceSpaceCreateInfo>();

        Vector3f positionF = new(position.X, position.Y, position.Z);
        Quaternionf orientationF = new(orientation.X, orientation.Y, orientation.Z, orientation.W);

        refSpaceInfo.PoseInReferenceSpace = new(orientationF, positionF);
        refSpaceInfo.ReferenceSpaceType = spaceType;

        XR.CreateReferenceSpace(session.session, ref refSpaceInfo, ref Space).ThrowIfNotSuccess();
    }
    
}