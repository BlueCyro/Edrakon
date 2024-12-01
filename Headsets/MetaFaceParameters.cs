using System.Numerics;
using KoboldOSC;

namespace Edrakon.Headsets;

public struct MetaFaceParameters : IParameterStruct
{
    public static OSCInfo[] PathInfo { get; }
    public static int BundleCount { get; }

    static MetaFaceParameters()
    {
        PathInfo = [];
        BundleCount = 5;
    }


    public readonly float BrowLowererLFB;
    public readonly float BrowLowererRFB;
    public readonly float CheekPuffLFB;
    public readonly float CheekPuffRFB;
    public readonly float CheekRaiserLFB;
    public readonly float CheekRaiserRFB;
    public readonly float CheekSuckLFB;
    public readonly float CheekSuckRFB;
    public readonly float ChinRaiserBFB;
    public readonly float ChinRaiserTFB;
    public readonly float DimplerLFB;
    public readonly float DimplerRFB;
    public readonly float EyesClosedLFB;
    public readonly float EyesClosedRFB;
    public readonly float EyesLookDownLFB;
    public readonly float EyesLookDownRFB;
    public readonly float EyesLookLeftLFB;
    public readonly float EyesLookLeftRFB;
    public readonly float EyesLookRightLFB;
    public readonly float EyesLookRightRFB;
    public readonly float EyesLookUpLFB;
    public readonly float EyesLookUpRFB;
    public readonly float InnerBrowRaiserLFB;
    public readonly float InnerBrowRaiserRFB;
    public readonly float JawDropFB;
    public readonly float JawSidewaysLeftFB;
    public readonly float JawSidewaysRightFB;
    public readonly float JawThrustFB;
    public readonly float LidTightenerLFB;
    public readonly float LidTightenerRFB;
    public readonly float LipCornerDepressorLFB;
    public readonly float LipCornerDepressorRFB;
    public readonly float LipCornerPullerLFB;
    public readonly float LipCornerPullerRFB;
    public readonly float LipFunnelerLBFB;
    public readonly float LipFunnelerLTFB;
    public readonly float LipFunnelerRBFB;
    public readonly float LipFunnelerRTFB;
    public readonly float LipPressorLFB;
    public readonly float LipPressorRFB;
    public readonly float LipPuckerLFB;
    public readonly float LipPuckerRFB;
    public readonly float LipStretcherLFB;
    public readonly float LipStretcherRFB;
    public readonly float LipSuckLBFB;
    public readonly float LipSuckLTFB;
    public readonly float LipSuckRBFB;
    public readonly float LipSuckRTFB;
    public readonly float LipTightenerLFB;
    public readonly float LipTightenerRFB;
    public readonly float LipsTowardFB;
    public readonly float LowerLipDepressorLFB;
    public readonly float LowerLipDepressorRFB;
    public readonly float MouthLeftFB;
    public readonly float MouthRightFB;
    public readonly float NoseWrinklerLFB;
    public readonly float NoseWrinklerRFB;
    public readonly float OuterBrowRaiserLFB;
    public readonly float OuterBrowRaiserRFB;
    public readonly float UpperLidRaiserLFB;
    public readonly float UpperLidRaiserRFB;
    public readonly float UpperLipRaiserLFB;
    public readonly float UpperLipRaiserRFB;
    public readonly float TongueTipInterdentalFB;
    public readonly float TongueTipAlveolarFB;
    public readonly float TongueFrontDorsalPalateFB;
    public readonly float TongueMidDorsalPalateFB;
    public readonly float TongueBackDorsalVelarFB;
    public readonly float TongueOutFB;
    public readonly float TongueRetreatFB;


    public readonly void Serialize(Span<KOscBundle> bundles)
    {
        #region Eye Look
        KOscMessage eyeTrackedGazePoint = new("/sl/eyeTrackedGazePoint");
        eyeTrackedGazePoint.WriteVector(default);

        float leftX = EyesLookLeftLFB - EyesLookRightLFB;
        KOscMessage leftEyeX = new("/avatar/parameters/LeftEyeX");
        leftEyeX.WriteFloat(leftX);

        float leftY = EyesLookUpLFB - EyesLookDownLFB;
        KOscMessage leftEyeY = new("/avatar/parameters/LeftEyeY");
        leftEyeY.WriteFloat(leftY);

        float rightX = EyesLookLeftRFB - EyesLookRightRFB;
        KOscMessage rightEyeX = new("/avatar/parameters/RightEyeX");
        rightEyeX.WriteFloat(rightX);

        float rightY = EyesLookUpRFB - EyesLookDownRFB;
        KOscMessage rightEyeY = new("/avatar/parameters/RightEyeY");
        rightEyeY.WriteFloat(rightY);

        Vector3 centerVec = new((leftX + rightX) / 2, (leftY + rightY) / 2, 0);
        KOscMessage centerVecFull = new("/tracking/eye/CenterVecFull");
        centerVecFull.WriteVector(in centerVec);

        #endregion


        #region Eye lids

        KOscMessage rightEyeLid =  new("/avatar/parameters/RightEyeLid");
        rightEyeLid.WriteFloat(EyesClosedRFB);

        KOscMessage rightEyeLidExpandedSqueeze =  new("/avatar/parameters/RightEyeLidExpandedSqueeze");
        rightEyeLidExpandedSqueeze.WriteFloat(UpperLidRaiserRFB);

        KOscMessage rightEyeSqueezeToggle =  new("/avatar/parameters/RightEyeSqueezeToggle");
        rightEyeSqueezeToggle.WriteFloat(0f);

        KOscMessage rightEyeWidenToggle =  new("/avatar/parameters/RightEyeWidenToggle");
        rightEyeWidenToggle.WriteInt(UpperLidRaiserRFB > 0f ? 1 : 0);

        KOscMessage leftEyeLid =  new("/avatar/parameters/LeftEyeLid");
        leftEyeLid.WriteFloat(EyesClosedLFB);

        KOscMessage leftEyeLidExpandedSqueeze =  new("/avatar/parameters/LeftEyeLidExpandedSqueeze");
        leftEyeLidExpandedSqueeze.WriteFloat(UpperLidRaiserLFB);

        KOscMessage leftEyeSqueezeToggle =  new("/avatar/parameters/LeftEyeSqueezeToggle");
        leftEyeSqueezeToggle.WriteFloat(0f);

        KOscMessage leftEyeWidenToggle =  new("/avatar/parameters/LeftEyeWidenToggle");
        leftEyeWidenToggle.WriteFloat(UpperLidRaiserLFB > 0 ? 1 : 0);

        KOscMessage eyesClosedAmount =  new("/tracking/eye/EyesClosedAmount");
        eyesClosedAmount.WriteFloat(Math.Max(EyesClosedLFB, EyesClosedRFB));


        #endregion

        bundles[0] = new(eyeTrackedGazePoint, leftEyeX, leftEyeY, rightEyeX, rightEyeY, centerVecFull, rightEyeLid, leftEyeLid);
    }
}
