using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Edrakon.Headsets;
using KoboldOSC.Messages;
using Edrakon.Wrappers;
using Silk.NET.OpenXR;
using Edrakon.Logging;
using System.Diagnostics.CodeAnalysis;
using KoboldOSC;

namespace Edrakon;


public class Program
{
    public const string EYE_GAZE_EXTENSION = "XR_EXT_eye_gaze_interaction";
    public const string HEADLESS_EXTENSION = "XR_MND_headless";
    public const string FB_FACE_EXTENSION  = "XR_FB_face_tracking2";

    [AllowNull]
    static QuestPro headset;

    [AllowNull]
    static Task inputTask;


    public static async Task Main(string[] args)
    {
        // Spex helped with this. Give him a friendly meow.


        // File.WriteAllBytes("../OSCTest.osc", dest);

        headset = new(args.Length > 0 ? int.Parse(args[0]) : 8000);
        

        headset.Initialize();

        #if !DEBUG
        inputTask = Task.Run(InputTask);
        #endif

        await headset.Run();
    }


    static void InputTask()
    {
        for (;;)
        {
            EdraLogger.Log("Input event", LogLevel.DEBUG);
            
            if (Console.ReadKey().Key == ConsoleKey.F)
            {
                headset.Dispose();
                break;
            }
        }
    }
}
