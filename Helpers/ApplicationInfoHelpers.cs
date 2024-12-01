using System.Text;
using Silk.NET.OpenXR;

namespace Edrakon.Helpers;


public static class ApplicationInfoHelpers
{   public const int XR_APP_NAME_LENGTH = 128;
    public const int XR_ENGINE_NAME_LENGTH = 128;
    
    
    
    public static unsafe void SetApplicationName(this in ApplicationInfo info, string name)
    {

        fixed (byte* ptr = info.ApplicationName)
        {
            Span<byte> appNameField = new(ptr, XR_APP_NAME_LENGTH);
            Span<byte> appName = Encoding.UTF8.GetBytes(name);

            appName.CopyTo(appNameField);
        }
    }


    public static unsafe void SetEngineName(this in ApplicationInfo info, string name)
    {

        fixed (byte* ptr = info.EngineName)
        {
            Span<byte> appNameField = new(ptr, XR_ENGINE_NAME_LENGTH);
            Span<byte> appName = Encoding.UTF8.GetBytes(name);

            appName.CopyTo(appNameField);
        }
    }
}