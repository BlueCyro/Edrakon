using System.Runtime.CompilerServices;
using Silk.NET.OpenXR;

namespace Edrakon.Helpers;

public static partial class XRStructHelper
{
    public static T Get<T>() where T : unmanaged => throw new NotImplementedException("This method was probably supposed to be intercepted...");
}