using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Edrakon.Headsets;
using KoboldOSC;
using Edrakon.Wrappers;
using Silk.NET.OpenXR;

namespace Edrakon;


public class Program
{
    public const string EYE_GAZE_EXTENSION = "XR_EXT_eye_gaze_interaction";
    public const string HEADLESS_EXTENSION = "XR_MND_headless";
    public const string FB_FACE_EXTENSION  = "XR_FB_face_tracking2";


    static string[] DesiredExtensions = [EYE_GAZE_EXTENSION, HEADLESS_EXTENSION, FB_FACE_EXTENSION];


    public static void Main(string[] args)
    {
        // Spex helped with this. Give him a friendly meow.

        // KOscMessage msg1 = new("/test/path/msg1");
        // msg1.WriteFloat(1.2345f);
        // msg1.WriteTime(new OSCTimeTag(DateTime.Now));
        // msg1.WriteString("This is a test string to see if my pattern works :D");
        // msg1.WriteInt(3);
        // msg1.WriteVector(new(6f, 2f, 1f));

        // Span<byte> msgSpan = stackalloc byte[msg1.ByteLength];

        // msg1.Serialize(msgSpan);

        // File.WriteAllBytes("OSCTEST.osc", msgSpan);

        SimpleXRWrapper wrapper = new();
        var extensions = wrapper.AllExtensions;
        // Console.WriteLine("Currently supported extensions: ");
        // foreach (string ext in extensions)
        //     Console.WriteLine($"    {ext}");

        Console.WriteLine("\n----------------\n");


        Console.WriteLine($"Application-required extensions: ");
        foreach (string ext in DesiredExtensions)
            Console.WriteLine($"    {ext}");

        Console.WriteLine("\n----------------\n");

        List<string> unsupported = [];
        foreach (string ext in DesiredExtensions)
        {
            if (!extensions.Contains(ext))
                unsupported.Add(ext);
        }

        if (unsupported.Count > 0)
        {
            Console.WriteLine($"Current runtime doesn't support the following extensions:");
            foreach (string ext in unsupported)
                Console.WriteLine($"    {ext}");
            
            return;
        }
        else
            Console.WriteLine("-------- All required extensions are supported! --------");
        
        XRInstance inst = wrapper.CreateInstance(new Version(1, 0, 0), "Edrakon XR -> OSC bridge", new Version(0, 0, 1), extensions: DesiredExtensions);

        Console.WriteLine($"Runtime name: {inst.RuntimeName}");
        Console.WriteLine($"Runtime version: {inst.RuntimeVersion}");

        XRSystem sys = inst.System;
        Console.WriteLine("------------------");
        Console.WriteLine($"Supports orientation: {sys.OrientationTracking}");
        Console.WriteLine($"Supports position: {sys.PositionTracking}");
        Console.WriteLine($"Max swapchain width: {sys.MaxSwapchainWidth}");
        Console.WriteLine($"Max swapchain height: {sys.MaxSwapchainHeight}");
        Console.WriteLine("------------------");

        XRFaceTrackingProperties faceProps = sys.GetProperty<XRFaceTrackingProperties>();

        Console.WriteLine($"Supports visual face tracking: {faceProps.SupportsVisualFaceTracking}");
        Console.WriteLine($"Supports audio face tracking: {faceProps.SupportsAudioFaceTracking}");
        Console.WriteLine("------------------");

        XRSession session = inst.CreateSession();
        session.Begin();

        MetaFaceTracker faceTracker = new(inst, session);


        ref MetaFaceParameters faceParams = ref faceTracker.GetBlendshapes();
        Span<float> faceExpressions = MemoryMarshal.Cast<MetaFaceParameters, float>(new Span<MetaFaceParameters>(ref faceParams));

        // Console.WriteLine($"Expressions:");
        // for (int i = 0; i < faceExpressions.Length; i++)
        // {
        //     FaceExpression2FB curExpression = (FaceExpression2FB)i;
        //     Console.WriteLine($"{curExpression}    {faceExpressions[i]}");
        // }

        IPEndPoint ep = new(IPAddress.Any, 8000);
        using (Socket cl = new(ep.AddressFamily, SocketType.Dgram, ProtocolType.Udp))
        {
            cl.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            
            KOscBundle[] tempBundles = [new()];
            // while (!Console.KeyAvailable)
            // {
                faceParams = ref faceTracker.GetBlendshapes();
                faceParams.Serialize(tempBundles);
                KOscBundle tempBundle = tempBundles[0];

                byte[] tempBytes = ArrayPool<byte>.Shared.Rent(tempBundle.ByteLength);
                Array.Clear(tempBytes);

                tempBundle.Serialize(tempBytes);
                File.WriteAllBytes("OSCTEST.osc", tempBytes[..tempBundle.ByteLength]);

                cl.SendTo(tempBytes[..tempBundle.ByteLength], ep);
                ArrayPool<byte>.Shared.Return(tempBytes);
            // }


            // for (int i = 0; i < tempBundle.ByteLength; i++)
            // {
            //     char byteChar = (char)tempBytes[i];
            //     Console.Write($"({(char.IsControl(byteChar) ? ' ' : byteChar)}) {tempBytes[i]:x2}  ");
            // }
            // Console.WriteLine();
            // Console.WriteLine("-------------------------");
            
        }
    }
}



public static class TransformHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ToNumeric(this Vector3f other) => Unsafe.As<Vector3f, Vector3>(ref other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3f ToSilk(this Vector3 other) => Unsafe.As<Vector3, Vector3f>(ref other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion ToNumeric(this Quaternionf other) => Unsafe.As<Quaternionf, Quaternion>(ref other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternionf ToSilk(this Quaternion other) => Unsafe.As<Quaternion, Quaternionf>(ref other);
}



public readonly struct XrFaceTracker2FB
{
    public readonly ulong Handle;
}
