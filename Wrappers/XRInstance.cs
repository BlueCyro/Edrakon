using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Edrakon.Helpers;
using Edrakon.Structs;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.OpenXR;

namespace Edrakon.Wrappers;


public class XRInstance : IDisposable
{

    public readonly XR XR;
    public readonly XRSystem System;


    public string RuntimeName => props.GetRuntimeName();
    public Version RuntimeVersion => (Version64)props.RuntimeVersion;
    public long Time
    {
        get
        {
            [DllImport("libc")]
            [SuppressGCTransition]
            static extern Timespec clock_gettime(ClockType clockId, out Timespec time);

            clock_gettime(ClockType.Monotonic, out Timespec time);
            timespecToTime.Call(instance, in time, out long xrTime);
            return xrTime;
        }
    }


    internal readonly Instance instance;
    internal readonly InstanceProperties props;


    private readonly PtrFuncTyped<XrConvertTimespecTimeToTimeKHR> timespecToTime;
    public XRInstance(XR xr, Instance inst)
    {
        XR = xr;
        instance = inst;


        props.Type = StructureType.InstanceProperties;
        xr.GetInstanceProperties(instance, ref props).ThrowIfNotSuccess();
        timespecToTime = XRPfnHelpers.GetXRFunction<XrConvertTimespecTimeToTimeKHR>(xr, inst, "xrConvertTimespecTimeToTimeKHR");

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
        ref byte funcNameBytes = ref MemoryMarshal.GetReference(funcNameStr.AsSpan());
        XR.GetInstanceProcAddr(instance, ref funcNameBytes, ref pfn).ThrowIfNotSuccess($"Could not get XR function: '{funcName}'.");
        return new(pfn);
    }


    public XRActionSet CreateActionSet(string name, string localizedName, uint priority = 0)
    {
        return new(XR, this, name, localizedName, priority);
    }


    public unsafe void SuggestInteractionProfileBindings(string profile, XRActionSuggestedBinding[] bindings)
    {
        InteractionProfileSuggestedBinding info = XRStructHelper.Get<InteractionProfileSuggestedBinding>();


        ActionSuggestedBinding* actionBindings = stackalloc ActionSuggestedBinding[bindings.Length];

        ulong path = StringToPath(profile);
        for (int i = 0; i < bindings.Length; i++)
        {
            actionBindings[i].Action = bindings[i].Action.Action;
            actionBindings[i].Binding = StringToPath(bindings[i].Binding);
        }
        info.InteractionProfile = path;
        info.CountSuggestedBindings = (uint)bindings.Length;
        info.SuggestedBindings = actionBindings;
        info.InteractionProfile = path;

        XR.SuggestInteractionProfileBinding(instance, ref info).ThrowIfNotSuccess();
    }


    public unsafe ulong StringToPath(string str)
    {
        ulong path = 0;
        Span<byte> strBytes = stackalloc byte[str.Utf8Count()];
        str.AsUtf8(strBytes);
        XR.StringToPath(instance, strBytes, &path);
        return path;
    }


    public void Dispose()
    {
        GC.SuppressFinalize(this);

        XR.DestroyInstance(instance);
        timespecToTime.Dispose();
    }
}




public delegate Result XrConvertTimespecTimeToTimeKHR(Instance instance, in Timespec timespec, out long time);