using System.Diagnostics.CodeAnalysis;
using Edrakon;
using Edrakon.Logging;
using Edrakon.Wrappers;
using Silk.NET.OpenXR;
using KoboldOSC;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;
using System.Net;
using System.Numerics;
using KoboldOSC.Messages;
using Edrakon.Helpers;
using Edrakon.Structs;

namespace Edrakon.Headsets;


public class QuestPro(int port) : IDisposable
{
    public const string HEADSET_VERSION         = "0.0.1";
    public const string HEADSET_APP_NAME        = "Quest Pro face & eye tracking bridge via Edrakon";
    public const string HEADLESS_EXTENSION      = "XR_MND_headless";
    public const string TIMESPEC_EXTENSION      = "XR_KHR_convert_timespec_time";
    public const string EYE_GAZE_EXTENSION      = "XR_EXT_eye_gaze_interaction";
    public const string FB_FACE_EXTENSION       = "XR_FB_face_tracking2";
    public const float  UPDATES_PER_SECOND      = 1000f;
    public const float  UPDATES_DELAY           = 1f / UPDATES_PER_SECOND;
    private string[] requiredExtensions         = [EYE_GAZE_EXTENSION, HEADLESS_EXTENSION, FB_FACE_EXTENSION, TIMESPEC_EXTENSION];



    public Action<object?, int>? Logger;



    public readonly IPEndPoint      TempEndpoint = new(IPAddress.Loopback, port);
    public static readonly Version  TargetOpenXRVersion = new(1, 0, 0);
    public static readonly string   AppName = "Quest Pro bridge via Edrakon";
    public static readonly Version  AppVersion = new(0, 0, 1);



    [AllowNull]
    private XRSession session;

    [AllowNull]
    private XRInstance instance;

    [AllowNull]
    private MetaEyeTracker eyeTracker;
    
    [AllowNull]
    private MetaFaceTracker faceTracker;

    [AllowNull]
    private KOscSender sender;



    [AllowNull]
    private SimpleXRWrapper wrapper;

    [AllowNull]
    private Task runTask;



    [AllowNull]
    private CancellationTokenSource tokenSource;
    private CancellationToken token;
    private static readonly TimeSpan delay = TimeSpan.FromSeconds(UPDATES_DELAY);
    private bool disposed;



    public void Initialize()
    {
        ThrowIfDisposed();

        Log("Initializing Quest Pro headset...");

        wrapper = new();

        if (!HasRequiredExtensions(out var unsupported))
            throw new NotImplementedException($"Required extensions are missing from the current OpenXR runtime! Missing extensions: {string.Join(", ", unsupported)}");

        Log("All required OpenXR extensions are available!");


        instance = wrapper.CreateInstance(TargetOpenXRVersion, AppName, AppVersion, extensions: requiredExtensions);
        Log("Created OpenXR instance.");

        session = instance.CreateSession();
        session.Begin();
        Log("Created session.");

        faceTracker = new(instance, session);

        faceTracker.Initialize();
        Log("Initialized face tracker.");


        eyeTracker = new(instance, session);
        eyeTracker.Logger = (obj, level) => EdraLogger.Log(obj, (LogLevel)level);
        Log("Initialized eye tracker.");
        

        Log($"Opening sender to endpoint: {TempEndpoint}");
        sender = new(TempEndpoint);
        sender.Logger = (msg, level) => EdraLogger.Log(msg, (LogLevel)level);
        sender.Open();

        tokenSource = new();
        token = tokenSource.Token;
    }



    public async Task Run()
    {
        ThrowIfDisposed();
        Log("Starting Quest Pro face tracking bridge.");
        runTask = Task.Run(RunTask);
        await runTask;
    }



    private async Task RunTask()
    {
        ThrowIfDisposed();
        for (;;)
        {
            if (token.IsCancellationRequested)
                break;


            ref MetaFaceInfo faceParams = ref faceTracker.GetBlendshapes();

            Serialize(sender, ref faceParams);

            await Task.Delay(delay);
        }
    }



    private void Serialize(KOscSender sender, ref MetaFaceInfo faceInfo)
    {
        ThrowIfDisposed();

        #region Eye Look
        float leftX = faceInfo.FaceParameters.EyesLookLeftLFB - faceInfo.FaceParameters.EyesLookRightLFB;
        float leftY = faceInfo.FaceParameters.EyesLookUpLFB - faceInfo.FaceParameters.EyesLookDownLFB;
        float rightX = faceInfo.FaceParameters.EyesLookLeftRFB - faceInfo.FaceParameters.EyesLookRightRFB;
        float rightY = faceInfo.FaceParameters.EyesLookUpRFB + faceInfo.FaceParameters.EyesLookDownRFB;

        eyeTracker.GetEyeGazes(out Posef gazePose);

        Vector3 gazeVec = Vector3.Transform(-Vector3.UnitZ, gazePose.Orientation.ToNumeric());

        KOscMessage eyeTrackedGazePoint = new("/sl/eyeTrackedGazePoint");
        eyeTrackedGazePoint.WriteVector(gazeVec);
        KOscMessage leftEyeX = new("/avatar/parameters/LeftEyeX");
        leftEyeX.WriteFloat(leftX);
        KOscMessage leftEyeY = new("/avatar/parameters/LeftEyeY");
        leftEyeY.WriteFloat(leftY);
        KOscMessage rightEyeX = new("/avatar/parameters/RightEyeX");
        rightEyeX.WriteFloat(rightX);
        KOscMessage rightEyeY = new("/avatar/parameters/RightEyeY");
        rightEyeY.WriteFloat(rightY);
        KOscMessage centerVecFull = new("/tracking/eye/CenterVecFull");
        centerVecFull.WriteVector(gazePose.Position.ToNumeric());

        #endregion


        #region Eye lids

        KOscMessage rightEyeLid =  new("/avatar/parameters/RightEyeLid");
        rightEyeLid.WriteFloat(faceInfo.FaceParameters.EyesClosedRFB);
        KOscMessage rightEyeLidExpandedSqueeze =  new("/avatar/parameters/RightEyeLidExpandedSqueeze");
        rightEyeLidExpandedSqueeze.WriteFloat(faceInfo.FaceParameters.UpperLidRaiserRFB - faceInfo.FaceParameters.LidTightenerRFB);
        KOscMessage rightEyeSqueezeToggle =  new("/avatar/parameters/RightEyeSqueezeToggle");
        rightEyeSqueezeToggle.WriteFloat(0f);
        KOscMessage rightEyeWidenToggle =  new("/avatar/parameters/RightEyeWidenToggle");
        rightEyeWidenToggle.WriteInt(faceInfo.FaceParameters.UpperLidRaiserRFB > 0f ? 1 : 0);
        KOscMessage leftEyeLid =  new("/avatar/parameters/LeftEyeLid");
        leftEyeLid.WriteFloat(faceInfo.FaceParameters.EyesClosedLFB);
        KOscMessage leftEyeLidExpandedSqueeze =  new("/avatar/parameters/LeftEyeLidExpandedSqueeze");
        leftEyeLidExpandedSqueeze.WriteFloat(faceInfo.FaceParameters.UpperLidRaiserLFB - faceInfo.FaceParameters.LidTightenerLFB);
        KOscMessage leftEyeSqueezeToggle =  new("/avatar/parameters/LeftEyeSqueezeToggle");
        leftEyeSqueezeToggle.WriteFloat(0f);
        KOscMessage leftEyeWidenToggle =  new("/avatar/parameters/LeftEyeWidenToggle");
        leftEyeWidenToggle.WriteFloat(faceInfo.FaceParameters.UpperLidRaiserLFB > 0 ? 1 : 0);
        KOscMessage eyesClosedAmount =  new("/tracking/eye/EyesClosedAmount");
        eyesClosedAmount.WriteFloat(Math.Max(faceInfo.FaceParameters.EyesClosedLFB, faceInfo.FaceParameters.EyesClosedRFB));

        #endregion


        KOscBundle bundle1 = new(
            eyeTrackedGazePoint,
            leftEyeX,
            leftEyeY,
            rightEyeX,
            rightEyeY,
            centerVecFull,
            rightEyeLid,
            rightEyeLidExpandedSqueeze,
            rightEyeSqueezeToggle,
            rightEyeWidenToggle,
            leftEyeLid,
            leftEyeLidExpandedSqueeze,
            leftEyeSqueezeToggle,
            leftEyeWidenToggle,
            eyesClosedAmount);

        

        KOscMessage lowerFaceC = new("/sl/xrfb/facec/LowerFace");
        lowerFaceC.WriteFloat(faceInfo.FaceConfidences.Lower);
        KOscMessage upperFaceC = new("/sl/xrfb/facec/UpperFace");
        upperFaceC.WriteFloat(faceInfo.FaceConfidences.Upper);
        KOscMessage browLowererL = new("/sl/xrfb/facew/BrowLowererL");
        browLowererL.WriteFloat(faceInfo.FaceParameters.BrowLowererLFB);
        KOscMessage browLowererR = new("/sl/xrfb/facew/BrowLowererR");
        browLowererR.WriteFloat(faceInfo.FaceParameters.BrowLowererRFB);
        KOscMessage cheekPuffL = new("/sl/xrfb/facew/CheekPuffL");
        cheekPuffL.WriteFloat(faceInfo.FaceParameters.CheekPuffLFB);
        KOscMessage cheekPuffR = new("/sl/xrfb/facew/CheekPuffR");
        cheekPuffR.WriteFloat(faceInfo.FaceParameters.CheekPuffRFB);
        KOscMessage cheekRaiserL = new("/sl/xrfb/facew/CheekRaiserL");
        cheekRaiserL.WriteFloat(faceInfo.FaceParameters.CheekRaiserLFB);
        KOscMessage cheekRaiserR = new("/sl/xrfb/facew/CheekRaiserR");
        cheekRaiserR.WriteFloat(faceInfo.FaceParameters.CheekRaiserRFB);
        KOscMessage cheekSuckL = new("/sl/xrfb/facew/CheekSuckL");
        cheekSuckL.WriteFloat(faceInfo.FaceParameters.CheekSuckLFB);
        KOscMessage cheekSuckR = new("/sl/xrfb/facew/CheekSuckR");
        cheekSuckR.WriteFloat(faceInfo.FaceParameters.CheekSuckRFB);
        KOscMessage chinRaiserB = new("/sl/xrfb/facew/ChinRaiserB");
        chinRaiserB.WriteFloat(faceInfo.FaceParameters.ChinRaiserBFB);
        KOscMessage chinRaiserT = new("/sl/xrfb/facew/ChinRaiserT");
        chinRaiserT.WriteFloat(faceInfo.FaceParameters.ChinRaiserTFB);
        KOscMessage dimplerL = new("/sl/xrfb/facew/DimplerL");
        dimplerL.WriteFloat(faceInfo.FaceParameters.DimplerLFB);
        KOscMessage dimplerR = new("/sl/xrfb/facew/DimplerR");
        dimplerR.WriteFloat(faceInfo.FaceParameters.DimplerRFB);
        KOscMessage eyesClosedL = new("/sl/xrfb/facew/EyesClosedL");
        eyesClosedL.WriteFloat(faceInfo.FaceParameters.EyesClosedLFB);
        KOscMessage eyesClosedR = new("/sl/xrfb/facew/EyesClosedR");
        eyesClosedR.WriteFloat(faceInfo.FaceParameters.EyesClosedRFB);
        KOscMessage eyesLookDownL = new("/sl/xrfb/facew/EyesLookDownL");
        eyesLookDownL.WriteFloat(faceInfo.FaceParameters.EyesLookDownLFB);
        KOscMessage eyesLookDownR = new("/sl/xrfb/facew/EyesLookDownR");
        eyesLookDownR.WriteFloat(faceInfo.FaceParameters.EyesLookDownRFB);
        KOscMessage eyesLookLeftL = new("/sl/xrfb/facew/EyesLookLeftL");
        eyesLookLeftL.WriteFloat(faceInfo.FaceParameters.EyesLookLeftLFB);
        KOscMessage eyesLookLeftR = new("/sl/xrfb/facew/EyesLookLeftR");
        eyesLookLeftR.WriteFloat(faceInfo.FaceParameters.EyesLookLeftRFB);
        KOscMessage eyesLookRightL = new("/sl/xrfb/facew/EyesLookRightL");
        eyesLookRightL.WriteFloat(faceInfo.FaceParameters.EyesLookRightLFB);
        KOscMessage eyesLookRightR = new("/sl/xrfb/facew/EyesLookRightR");
        eyesLookRightR.WriteFloat(faceInfo.FaceParameters.EyesLookRightRFB);
        KOscMessage eyesLookUpL = new("/sl/xrfb/facew/EyesLookUpL");
        eyesLookUpL.WriteFloat(faceInfo.FaceParameters.EyesLookUpLFB);

        KOscBundle bundle2 = new(
            lowerFaceC,
            upperFaceC,
            browLowererL,
            browLowererR,
            cheekPuffL,
            cheekPuffR,
            cheekRaiserL,
            cheekRaiserR,
            cheekSuckL,
            cheekSuckR,
            chinRaiserB,
            chinRaiserT,
            dimplerL,
            dimplerR,
            eyesClosedL,
            eyesClosedR,
            eyesLookDownL,
            eyesLookDownR,
            eyesLookLeftL,
            eyesLookLeftR,
            eyesLookRightL,
            eyesLookRightR,
            eyesLookUpL);

        

        KOscMessage eyesLookUpR = new("/sl/xrfb/facew/EyesLookUpR");
        eyesLookUpR.WriteFloat(faceInfo.FaceParameters.EyesLookUpRFB);
        KOscMessage innerBrowRaiserL = new("/sl/xrfb/facew/InnerBrowRaiserL");
        innerBrowRaiserL.WriteFloat(faceInfo.FaceParameters.InnerBrowRaiserLFB);
        KOscMessage innerBrowRaiserR = new("/sl/xrfb/facew/InnerBrowRaiserR");
        innerBrowRaiserR.WriteFloat(faceInfo.FaceParameters.InnerBrowRaiserRFB);
        KOscMessage jawDrop = new("/sl/xrfb/facew/JawDrop");
        jawDrop.WriteFloat(faceInfo.FaceParameters.JawDropFB);
        KOscMessage jawSidewaysLeft = new("/sl/xrfb/facew/JawSidewaysLeft");
        jawSidewaysLeft.WriteFloat(faceInfo.FaceParameters.JawSidewaysLeftFB);
        KOscMessage jawSidewaysRight = new("/sl/xrfb/facew/JawSidewaysRight");
        jawSidewaysRight.WriteFloat(faceInfo.FaceParameters.JawSidewaysRightFB);
        KOscMessage jawThrust = new("/sl/xrfb/facew/JawThrust");
        jawThrust.WriteFloat(faceInfo.FaceParameters.JawThrustFB);
        KOscMessage lidTightenerL = new("/sl/xrfb/facew/LidTightenerL");
        lidTightenerL.WriteFloat(faceInfo.FaceParameters.LidTightenerLFB);
        KOscMessage lidTightenerR = new("/sl/xrfb/facew/LidTightenerR");
        lidTightenerR.WriteFloat(faceInfo.FaceParameters.LidTightenerRFB);
        KOscMessage lipCornerDepressorL = new("/sl/xrfb/facew/LipCornerDepressorL");
        lipCornerDepressorL.WriteFloat(faceInfo.FaceParameters.LipCornerDepressorLFB);
        KOscMessage lipCornerDepressorR = new("/sl/xrfb/facew/LipCornerDepressorR");
        lipCornerDepressorR.WriteFloat(faceInfo.FaceParameters.LipCornerDepressorRFB);
        KOscMessage lipCornerPullerL = new("/sl/xrfb/facew/LipCornerPullerL");
        lipCornerPullerL.WriteFloat(faceInfo.FaceParameters.LipCornerPullerLFB);
        KOscMessage lipCornerPullerR = new("/sl/xrfb/facew/LipCornerPullerR");
        lipCornerPullerR.WriteFloat(faceInfo.FaceParameters.LipCornerPullerRFB);
        KOscMessage lipFunnelerLB = new("/sl/xrfb/facew/LipFunnelerLB");
        lipFunnelerLB.WriteFloat(faceInfo.FaceParameters.LipFunnelerLBFB);
        KOscMessage lipFunnelerLT = new("/sl/xrfb/facew/LipFunnelerLT");
        lipFunnelerLT.WriteFloat(faceInfo.FaceParameters.LipFunnelerLTFB);
        KOscMessage lipFunnelerRB = new("/sl/xrfb/facew/LipFunnelerRB");
        lipFunnelerRB.WriteFloat(faceInfo.FaceParameters.LipFunnelerRBFB);
        KOscMessage lipFunnelerRT = new("/sl/xrfb/facew/LipFunnelerRT");
        lipFunnelerRT.WriteFloat(faceInfo.FaceParameters.LipFunnelerRTFB);
        KOscMessage lipPressorL = new("/sl/xrfb/facew/LipPressorL");
        lipPressorL.WriteFloat(faceInfo.FaceParameters.LipPressorLFB);
        KOscMessage lipPressorR = new("/sl/xrfb/facew/LipPressorR");
        lipPressorR.WriteFloat(faceInfo.FaceParameters.LipPressorRFB);
        KOscMessage lipPuckerL = new("/sl/xrfb/facew/LipPuckerL");
        lipPuckerL.WriteFloat(faceInfo.FaceParameters.LipPuckerLFB);

        KOscBundle bundle3 = new(
            eyesLookUpR,
            innerBrowRaiserL,
            innerBrowRaiserR,
            jawDrop,
            jawSidewaysLeft,
            jawSidewaysRight,
            jawThrust,
            lidTightenerL,
            lidTightenerR,
            lipCornerDepressorL,
            lipCornerDepressorR,
            lipCornerPullerL,
            lipCornerPullerR,
            lipFunnelerLB,
            lipFunnelerLT,
            lipFunnelerRB,
            lipFunnelerRT,
            lipPressorL,
            lipPressorR,
            lipPuckerL);



        KOscMessage lipPuckerR = new("/sl/xrfb/facew/LipPuckerR");
        lipPuckerR.WriteFloat(faceInfo.FaceParameters.LipPuckerRFB);
        KOscMessage lipStretcherL = new("/sl/xrfb/facew/LipStretcherL");
        lipStretcherL.WriteFloat(faceInfo.FaceParameters.LipStretcherLFB);
        KOscMessage lipStretcherR = new("/sl/xrfb/facew/LipStretcherR");
        lipStretcherR.WriteFloat(faceInfo.FaceParameters.LipStretcherRFB);
        KOscMessage lipSuckLB = new("/sl/xrfb/facew/LipSuckLB");
        lipSuckLB.WriteFloat(faceInfo.FaceParameters.LipSuckLBFB);
        KOscMessage lipSuckLT = new("/sl/xrfb/facew/LipSuckLT");
        lipSuckLT.WriteFloat(faceInfo.FaceParameters.LipSuckLTFB);
        KOscMessage lipSuckRB = new("/sl/xrfb/facew/LipSuckRB");
        lipSuckRB.WriteFloat(faceInfo.FaceParameters.LipSuckRBFB);
        KOscMessage lipSuckRT = new("/sl/xrfb/facew/LipSuckRT");
        lipSuckRT.WriteFloat(faceInfo.FaceParameters.LipSuckRTFB);
        KOscMessage lipTightenerL = new("/sl/xrfb/facew/LipTightenerL");
        lipTightenerL.WriteFloat(faceInfo.FaceParameters.LipTightenerLFB);
        KOscMessage lipTightenerR = new("/sl/xrfb/facew/LipTightenerR");
        lipTightenerR.WriteFloat(faceInfo.FaceParameters.LipTightenerRFB);
        KOscMessage lipsToward = new("/sl/xrfb/facew/LipsToward");
        lipsToward.WriteFloat(faceInfo.FaceParameters.LipsTowardFB);
        KOscMessage lowerLipDepressorL = new("/sl/xrfb/facew/LowerLipDepressorL");
        lowerLipDepressorL.WriteFloat(faceInfo.FaceParameters.LowerLipDepressorLFB);
        KOscMessage lowerLipDepressorR = new("/sl/xrfb/facew/LowerLipDepressorR");
        lowerLipDepressorR.WriteFloat(faceInfo.FaceParameters.LowerLipDepressorRFB);
        KOscMessage mouthLeft = new("/sl/xrfb/facew/MouthLeft");
        mouthLeft.WriteFloat(faceInfo.FaceParameters.MouthLeftFB);
        KOscMessage mouthRight = new("/sl/xrfb/facew/MouthRight");
        mouthRight.WriteFloat(faceInfo.FaceParameters.MouthRightFB);
        KOscMessage noseWrinklerL = new("/sl/xrfb/facew/NoseWrinklerL");
        noseWrinklerL.WriteFloat(faceInfo.FaceParameters.NoseWrinklerLFB);
        KOscMessage noseWrinklerR = new("/sl/xrfb/facew/NoseWrinklerR");
        noseWrinklerR.WriteFloat(faceInfo.FaceParameters.NoseWrinklerRFB);
        KOscMessage outerBrowRaiserL = new("/sl/xrfb/facew/OuterBrowRaiserL");
        outerBrowRaiserL.WriteFloat(faceInfo.FaceParameters.OuterBrowRaiserLFB);
        KOscMessage outerBrowRaiserR = new("/sl/xrfb/facew/OuterBrowRaiserR");
        outerBrowRaiserR.WriteFloat(faceInfo.FaceParameters.OuterBrowRaiserRFB);
        KOscMessage upperLidRaiserL = new("/sl/xrfb/facew/UpperLidRaiserL");
        upperLidRaiserL.WriteFloat(faceInfo.FaceParameters.UpperLidRaiserLFB);
        KOscMessage upperLidRaiserR = new("/sl/xrfb/facew/UpperLidRaiserR");
        upperLidRaiserR.WriteFloat(faceInfo.FaceParameters.UpperLidRaiserRFB);
        KOscMessage upperLipRaiserL = new("/sl/xrfb/facew/UpperLipRaiserL");
        upperLipRaiserL.WriteFloat(faceInfo.FaceParameters.UpperLipRaiserLFB);
        KOscMessage upperLipRaiserR = new("/sl/xrfb/facew/UpperLipRaiserR");
        upperLipRaiserR.WriteFloat(faceInfo.FaceParameters.UpperLipRaiserRFB);

        KOscBundle bundle4 = new(
            lipPuckerR,
            lipStretcherL,
            lipStretcherR,
            lipSuckLB,
            lipSuckLT,
            lipSuckRB,
            lipSuckRT,
            lipTightenerL,
            lipTightenerR,
            lipsToward,
            lowerLipDepressorL,
            lowerLipDepressorR,
            mouthLeft,
            mouthRight,
            noseWrinklerL,
            noseWrinklerR,
            outerBrowRaiserL,
            outerBrowRaiserR,
            upperLidRaiserL,
            upperLidRaiserR,
            upperLipRaiserL,
            upperLipRaiserR);
        
        


        KOscMessage tongueTipInterdental = new("/sl/xrfb/facew/TongueTipInterdental");
        tongueTipInterdental.WriteFloat(faceInfo.FaceParameters.TongueTipInterdentalFB);
        KOscMessage tongueTipAlveolar = new("/sl/xrfb/facew/TongueTipAlveolar");
        tongueTipAlveolar.WriteFloat(faceInfo.FaceParameters.TongueTipAlveolarFB);
        KOscMessage frontDorsalPalate = new("/sl/xrfb/facew/FrontDorsalPalate");
        frontDorsalPalate.WriteFloat(faceInfo.FaceParameters.TongueFrontDorsalPalateFB);
        KOscMessage midDorsalPalate = new("/sl/xrfb/facew/MidDorsalPalate");
        midDorsalPalate.WriteFloat(faceInfo.FaceParameters.TongueMidDorsalPalateFB);
        KOscMessage backDorsalVelar = new("/sl/xrfb/facew/BackDorsalVelar");
        backDorsalVelar.WriteFloat(faceInfo.FaceParameters.TongueBackDorsalVelarFB);
        KOscMessage tongueOut = new("/sl/xrfb/facew/TongueOut");
        tongueOut.WriteFloat(faceInfo.FaceParameters.TongueOutFB);
        KOscMessage tongueRetreat = new("/sl/xrfb/facew/TongueRetreat");
        tongueRetreat.WriteFloat(faceInfo.FaceParameters.TongueRetreatFB);


        KOscBundle bundle5 = new(
            tongueTipInterdental,
            tongueTipAlveolar,
            frontDorsalPalate,
            midDorsalPalate,
            backDorsalVelar,
            tongueOut,
            tongueRetreat);

        sender.Send(bundle1, bundle2, bundle3, bundle4, bundle5);
        // bundle.Dispose();
    }


    public void Stop()
    {
        Log("Shutting down Quest Pro face tracking bridge.");
        tokenSource.Cancel();
        runTask.Wait();
        Log("Quest Pro face tracking bridge shutdown complete!");
    }


    private bool HasRequiredExtensions([NotNullWhen(false)] out List<string>? unsupported)
    {
        unsupported = [];
        var allExts = wrapper.AllExtensions;
        foreach (string ext in requiredExtensions)
        {
            if (!allExts.Contains(ext))
                unsupported.Add(ext);
        }
        if (unsupported.Count > 0)
            return false;

        unsupported = null;
        return true;
    }


    public void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(disposed, this);


    private void Log(object? message = null, int logLevel = 0) => Logger?.Invoke(message, logLevel);


    public void Dispose()
    {
        if (disposed)
            return;

        Log("Disposing of Quest Pro device.", 3);
        GC.SuppressFinalize(this);
        Stop();

        sender.Dispose();
        faceTracker.Dispose();
        session.Dispose();
        instance.Dispose();

        Log("Quest Pro device disposal success!", 3);
    }
}



