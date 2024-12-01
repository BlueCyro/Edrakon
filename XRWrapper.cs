using System.Runtime.CompilerServices;
using Edrakon.Helpers;
using Silk.NET.OpenXR;

namespace Edrakon;

public class XRWrapper : IDisposable
{
    public XRWrapper()
    {
        xr = XR.GetApi();
    }

    public XRWrapper(XR xr)
    {
        this.xr = xr;
    }

    // public IReadOnlyDictionary<string, ExtensionProperties> AllExtensions
    // {
    //     get
    //     {
    //         Dictionary<string, ExtensionProperties> extDict = [];

    //         xr.GetExtensionCount(out uint extCount).ThrowIfNotSuccess();
    //         Span<ExtensionProperties> extProps = stackalloc ExtensionProperties[(int)extCount];
    //         extProps.InitExts();
    //         xr.GetExtensions(ref extCount, extProps).ThrowIfNotSuccess();

    //         for (int i = 0; i < extCount; i++)
    //             extDict.Add(extProps[i].GetExtensionName(), extProps[i]);
            
    //         return extDict;
    //     }
    // }


    public ulong SystemID => sysID;


    private readonly XR xr;
    internal Instance instance;
    internal SystemGetInfo sysInfo;
    internal ulong sysID;


    public void CreateInstance(string appName, string engineName, Span<string> extensions)
    {
        // XRHelpers.CreateInstance(xr, appName, engineName, extensions, ref instance).ThrowIfNotSuccess();
        // sysInfo.Type = StructureType.SystemGetInfo;
        // sysInfo.FormFactor = FormFactor.HeadMountedDisplay;
        // instance.GetSystem(xr, ref sysInfo, ref sysID).ThrowIfNotSuccess();
    }


    public void GetInstanceProperties(out InstanceProperties props)
    {
        if (instance.Handle == 0)
            throw new InvalidOperationException($"Can't query instance properties before this wrapper has an instance! (Did you call '{nameof(CreateInstance)}'?)");

        Unsafe.SkipInit(out props);
        props.Type = StructureType.InstanceProperties;
        xr.GetInstanceProperties(instance, ref Unsafe.AsRef(in props)).ThrowIfNotSuccess();
    }


    public unsafe void GetProperties<T>(out T propertyStruct, StructureType type) where T : unmanaged
    {
        if (instance.Handle == 0)
            throw new InvalidOperationException($"Can't query System properties before this wrapper has an instance! (Did you call '{nameof(CreateInstance)}'?)");
        
        Unsafe.SkipInit(out SystemProperties props);
        Unsafe.SkipInit(out propertyStruct);
        Unsafe.As<T, StructureType>(ref Unsafe.AsRef(in propertyStruct)) = type;

        fixed (T* propPtr = &propertyStruct)
        {
            props.Type = StructureType.SystemProperties;
            props.Next = propPtr;
        }
        xr.GetSystemProperties(instance, sysID, ref props).ThrowIfNotSuccess();
        // Console.WriteLine((nint)props.Next);
    }


    public unsafe void GetSystemProperties(out SystemProperties props)
    {
        if (instance.Handle == 0)
            throw new InvalidOperationException($"Can't query System properties before this wrapper has an instance! (Did you call '{nameof(CreateInstance)}'?)");

        props = new();
        props.Type = StructureType.SystemProperties;
        xr.GetSystemProperties(instance, sysID, ref props).ThrowIfNotSuccess();
    }


    public void Dispose()
    {
        GC.SuppressFinalize(this);

        if (instance.Handle > 0)
            xr.DestroyInstance(instance);

    }
}
