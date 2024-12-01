using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Edrakon.Helpers;
using Edrakon.Structs;
using Edrakon.Wrapper;
using Silk.NET.Core;
using Silk.NET.OpenXR;

namespace Edrakon.Wrappers;


public class XRInstance : IDisposable
{
    public readonly XR XR;
    public readonly XRSystem System;


    public string RuntimeName => props.GetRuntimeName();
    public Version RuntimeVersion => (Version64)props.RuntimeVersion;


    internal readonly Instance instance;
    internal readonly InstanceProperties props;


    public XRInstance(XR xr, Instance inst)
    {
        XR = xr;
        instance = inst;


        props.Type = StructureType.InstanceProperties;
        xr.GetInstanceProperties(instance, ref props).ThrowIfNotSuccess();

        System = new(xr, this);
    }


    public XRSession CreateSession() => new(XR, this, System);


    public T GetProperty<T>() where T : IXRPropertyBase, new()
    {
        T prop = new();
        prop.Instantiate(XR, this);
        return prop;
    }


    internal unsafe T GetRawProperty<T>() where T : unmanaged
    {
        SystemProperties sysProps = new();
        T props = new();

        sysProps.Next = &props;
        XR.GetSystemProperties(instance, System.SysID, ref sysProps);
        return props;
    }



    public unsafe PtrFuncTyped<T> GetXRFunction<T>(string funcName) where T : Delegate
    {
        StackString128 funcNameStr = new(funcName);
        PfnVoidFunction pfn = new();
        XR.GetInstanceProcAddr(instance, ref funcNameStr.Bytes, ref pfn).ThrowIfNotSuccess($"Could not get XR function: '{funcName}'.");
        return new(pfn);
    }


    public void Dispose()
    {
        GC.SuppressFinalize(this);

        XR.DestroyInstance(instance);
    }
}