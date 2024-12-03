using Edrakon.Helpers;
using Edrakon.Structs;
using Silk.NET.Core;
using Silk.NET.OpenXR;


namespace Edrakon.Wrappers;

public class SimpleXRWrapper
{
    internal readonly XR xr = XR.GetApi();

    
    /// <summary>
    /// Gets the number of extensions available in the current OpenXR runtime.
    /// </summary>
    public uint ExtensionCount
    {
        get
        {
            xr.GetExtensionCount(out uint count).ThrowIfNotSuccess("Failed to get extension count in simple XR wrapper.");
            return count;
        }
    }



    /// <summary>
    /// Returns a set of all extensions available in the current OpenXR runtime.
    /// </summary>
    public IReadOnlySet<string> AllExtensions
    {
        get
        {
            HashSet<string> extensions = [];

            uint count = ExtensionCount;
            Span<ExtensionProperties> extProps = stackalloc ExtensionProperties[(int)count];
            for (int i = 0; i < count; i++)
                extProps[i] = XRHelpers.GetPropertyStruct<ExtensionProperties>();

            xr.GetExtensions(ref count, extProps).ThrowIfNotSuccess("Failed to retrieve available OpenXR extensions.");

            for (int i = 0; i < count; i++)
                extensions.Add(extProps[i].GetExtensionName());

            return extensions;
        }
    }


    /// <summary>
    /// Checks if an extension exists in the current OpenXR runtime.
    /// </summary>
    /// <param name="ext">The extension ID to check</param>
    /// <returns>True if the extension exists, otherwise false.</returns>
    public bool ExtensionExists(string ext) => AllExtensions.Contains(ext);



    /// <summary>
    /// Creates an OpenXR instance which can be used to query information about the system and to create other XR classes.
    /// </summary>
    /// <param name="apiVersion">The version of the OpenXR API the application supports</param>
    /// <param name="appName">The name of the application</param>
    /// <param name="appVersion">The version of the application</param>
    /// <param name="engineName">The optional name of the application's engine</param>
    /// <param name="engineVersion">The optional version of the application's engine</param>
    /// <param name="extensions">A variable amount of extensions the application supports</param>
    /// <returns>An OpenXR instance.</returns>
    public unsafe XRInstance CreateInstance(Version apiVersion, string appName, Version appVersion, string? engineName = null, Version? engineVersion = null, params Span<string> extensions)
    {
        // Allocate some stack strings for each extension name
        StackString128* stackStrings = stackalloc StackString128[extensions.Length];

        // Allocate an equivalent amount of pointers to stack strings
        StackString128** stringPointers = stackalloc StackString128*[extensions.Length];

        // Copy each extension ID string to a stack string
        // string and store its pointer in the pointer array.
        for (int i = 0; i < extensions.Length; i++)
        {
            stackStrings[i] = new(extensions[i]);
            stringPointers[i] = stackStrings + i;
        }

        // Define app info
        ApplicationInfo appInfo = new()
        {
            ApplicationVersion = (Version32)appVersion,
            EngineVersion = engineVersion != null ? (Version32)engineVersion : 0,
            ApiVersion = (Version64)apiVersion
        };
        appInfo.SetApplicationName(appName);
        appInfo.SetEngineName(engineName ?? "");

        // Define instance info
        InstanceCreateInfo createInfo = new()
        {
            Type = StructureType.InstanceCreateInfo,
            EnabledExtensionCount = (uint)extensions.Length,
            EnabledExtensionNames = (byte**)stringPointers, // Cast the string pointer array to a byte pointer array
            ApplicationInfo = appInfo,
        };


        Instance instance = new();
        xr.CreateInstance(in createInfo, ref instance).ThrowIfNotSuccess("Failed to initialize OpenXR instance.");
        XRInstance instanceWrapper = new(xr, instance);

        return instanceWrapper;
    }
}