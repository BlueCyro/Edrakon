using System.Numerics;
using KoboldOSC;
using KoboldOSC.Messages;

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
}


public readonly struct MetaFaceInfo
{
    public readonly MetaFaceParameters FaceParameters;
    public readonly MetaFaceConfidences FaceConfidences;
}


public readonly struct MetaFaceConfidences
{
    public readonly float Lower;
    public readonly float Upper;
}