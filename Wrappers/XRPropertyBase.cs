using System.Runtime.CompilerServices;
using Edrakon.Helpers;
using Edrakon.Wrappers;
using Silk.NET.OpenXR;

namespace Edrakon.Wrappers;


public abstract class XRPropertyBase<T> : IXRPropertyBase
    where T : unmanaged
{
    public StructureType StructureType => Unsafe.As<T, StructureType>(ref properties);
    protected unsafe T properties = XRStructHelper.Get<T>();
    protected SystemProperties sysProps;


    public virtual unsafe void Instantiate(XR xr, XRInstance instance)
    {
        sysProps.Type = StructureType.SystemProperties;
        fixed(void* ptr = &properties)
        {
            sysProps.Next = ptr;
            xr.GetSystemProperties(instance.instance, instance.System.SysID, ref sysProps).ThrowIfNotSuccess();
        }
        
    }
}


public interface IXRPropertyBase
{
    StructureType StructureType { get; }

    internal void Instantiate(XR xr, XRInstance instance);
}