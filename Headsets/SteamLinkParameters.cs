using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using KoboldOSC;

namespace Edrakon.Headsets;

[Obsolete("May be removed pending certain use cases.")]
[StructLayout(LayoutKind.Sequential)]
public readonly struct SteamLinkParameters : IParameterStruct
{
    public static OSCInfo[] PathInfo { get; }
    public static int BundleCount { get; }

    static SteamLinkParameters()
    {
        PathInfo = SteamLinkHelpers.GenerateOSCInfo<SteamLinkParameters>();
        BundleCount = PathInfo.Select(info => info.BundleId).Where(id => id != -1).Distinct().Count();
    }


    [OSCInfo("sl/eyeTrackedGazePoint", 0)]
    public readonly Vector3 EyeTrackedGazePoint;

    [OSCInfo("/avatar/parameters/LeftEyeX", 0)]
    public readonly float LeftEyeX;
    
    [OSCInfo("/avatar/parameters/LeftEyeY", 0)]
    public readonly float LeftEyeY;
    
    [OSCInfo("/avatar/parameters/RightEyeX", 0)]
    public readonly float RightEyeX;
    
    [OSCInfo("/avatar/parameters/RightEyeY", 0)]
    public readonly float RightEyeY;
    
    [OSCInfo("/tracking/eye/CenterVecFull", 0)]
    public readonly Vector3 CenterVecFull;
    
    [OSCInfo("/avatar/parameters/RightEyeLid", 0)]
    public readonly float RightEyeLid;
    
    [OSCInfo("/avatar/parameters/RightEyeLidExpandedSqueeze", 0)]
    public readonly float RightEyeLidExpandedSqueeze;
    
    [OSCInfo("/avatar/parameters/RightEyeSqueezeToggle", 0)]
    public readonly float RightEyeSqueezeToggle;
    
    [OSCInfo("/avatar/parameters/RightEyeWidenToggle", 0)]
    public readonly float RightEyeWidenToggle;
    
    [OSCInfo("/avatar/parameters/LeftEyeLid", 0)]
    public readonly float LeftEyeLid;
    
    [OSCInfo("/avatar/parameters/LeftEyeLidExpandedSqueeze", 0)]
    public readonly float LeftEyeLidExpandedSqueeze;
    
    [OSCInfo("/avatar/parameters/LeftEyeSqueezeToggle", 0)]
    public readonly float LeftEyeSqueezeToggle;
    
    [OSCInfo("/avatar/parameters/LeftEyeWidenToggle", 0)]
    public readonly float LeftEyeWidenToggle;
    
    [OSCInfo("/tracking/eye/EyesClosedAmount", 0)]
    public readonly float EyesClosedAmount;





    [OSCInfo("/sl/xrfb/facec/LowerFace", 1)]
    public readonly float LowerFace;

    [OSCInfo("/sl/xrfb/facec/UpperFace", 1)]
    public readonly float UpperFace;

    [OSCInfo("/sl/xrfb/facew/BrowLowererL", 1)]
    public readonly float BrowLowererL;

    [OSCInfo("/sl/xrfb/facew/BrowLowererR", 1)]
    public readonly float BrowLowererR;

    [OSCInfo("/sl/xrfb/facew/CheekPuffL", 1)]
    public readonly float CheekPuffL;

    [OSCInfo("/sl/xrfb/facew/CheekPuffR", 1)]
    public readonly float CheekPuffR;

    [OSCInfo("/sl/xrfb/facew/CheekRaiserL", 1)]
    public readonly float CheekRaiserL;

    [OSCInfo("/sl/xrfb/facew/CheekRaiserR", 1)]
    public readonly float CheekRaiserR;

    [OSCInfo("/sl/xrfb/facew/CheekSuckL", 1)]
    public readonly float CheekSuckL;

    [OSCInfo("/sl/xrfb/facew/CheekSuckR", 1)]
    public readonly float CheekSuckR;

    [OSCInfo("/sl/xrfb/facew/ChinRaiserB", 1)]
    public readonly float ChinRaiserB;

    [OSCInfo("/sl/xrfb/facew/ChinRaiserT", 1)]
    public readonly float ChinRaiserT;

    [OSCInfo("/sl/xrfb/facew/DimplerL", 1)]
    public readonly float DimplerL;

    [OSCInfo("/sl/xrfb/facew/DimplerR", 1)]
    public readonly float DimplerR;

    [OSCInfo("/sl/xrfb/facew/EyesClosedL", 1)]
    public readonly float EyesClosedL;

    [OSCInfo("/sl/xrfb/facew/EyesClosedR", 1)]
    public readonly float EyesClosedR;

    [OSCInfo("/sl/xrfb/facew/EyesLookDownL", 1)]
    public readonly float EyesLookDownL;

    [OSCInfo("/sl/xrfb/facew/EyesLookDownR", 1)]
    public readonly float EyesLookDownR;

    [OSCInfo("/sl/xrfb/facew/EyesLookLeftL", 1)]
    public readonly float EyesLookLeftL;

    [OSCInfo("/sl/xrfb/facew/EyesLookLeftR", 1)]
    public readonly float EyesLookLeftR;

    [OSCInfo("/sl/xrfb/facew/EyesLookRightL", 1)]
    public readonly float EyesLookRightL;

    [OSCInfo("/sl/xrfb/facew/EyesLookRightR", 1)]
    public readonly float EyesLookRightR;

    [OSCInfo("/sl/xrfb/facew/EyesLookUpL", 1)]
    public readonly float EyesLookUpL;






    [OSCInfo("/sl/xrfb/facew/EyesLookUpR", 2)]
    public readonly float EyesLookUpR;

    [OSCInfo("/sl/xrfb/facew/InnerBrowRaiserL", 2)]
    public readonly float InnerBrowRaiserL;

    [OSCInfo("/sl/xrfb/facew/InnerBrowRaiserR", 2)]
    public readonly float InnerBrowRaiserR
    ;
    [OSCInfo("/sl/xrfb/facew/JawDrop", 2)]
    public readonly float JawDrop;

    [OSCInfo("/sl/xrfb/facew/JawSidewaysLeft", 2)]
    public readonly float JawSidewaysLeft;

    [OSCInfo("/sl/xrfb/facew/JawSidewaysRight", 2)]
    public readonly float JawSidewaysRight;

    [OSCInfo("/sl/xrfb/facew/JawThrust", 2)]
    public readonly float JawThrust;

    [OSCInfo("/sl/xrfb/facew/LidTightenerL", 2)]
    public readonly float LidTightenerL;

    [OSCInfo("/sl/xrfb/facew/LidTightenerR", 2)]
    public readonly float LidTightenerR;

    [OSCInfo("/sl/xrfb/facew/LipCornerDepressorL", 2)]
    public readonly float LipCornerDepressorL;

    [OSCInfo("/sl/xrfb/facew/LipCornerDepressorR", 2)]
    public readonly float LipCornerDepressorR;

    [OSCInfo("/sl/xrfb/facew/LipCornerPullerL", 2)]
    public readonly float LipCornerPullerL;

    [OSCInfo("/sl/xrfb/facew/LipCornerPullerR", 2)]
    public readonly float LipCornerPullerR;

    [OSCInfo("/sl/xrfb/facew/LipFunnelerLB", 2)]
    public readonly float LipFunnelerLB;

    [OSCInfo("/sl/xrfb/facew/LipFunnelerLT", 2)]
    public readonly float LipFunnelerLT;

    [OSCInfo("/sl/xrfb/facew/LipFunnelerRB", 2)]
    public readonly float LipFunnelerRB;

    [OSCInfo("/sl/xrfb/facew/LipFunnelerRT", 2)]
    public readonly float LipFunnelerRT;

    [OSCInfo("/sl/xrfb/facew/LipPressorL", 2)]
    public readonly float LipPressorL;

    [OSCInfo("/sl/xrfb/facew/LipPressorR", 2)]
    public readonly float LipPressorR;

    [OSCInfo("/sl/xrfb/facew/LipPuckerL", 2)]
    public readonly float LipPuckerL;



    

    [OSCInfo("/sl/xrfb/facew/LipPuckerR", 3)]
    public readonly float LipPuckerR;

    [OSCInfo("/sl/xrfb/facew/LipStretcherL", 3)]
    public readonly float LipStretcherL;

    [OSCInfo("/sl/xrfb/facew/LipStretcherR", 3)]
    public readonly float LipStretcherR;

    [OSCInfo("/sl/xrfb/facew/LipSuckLB", 3)]
    public readonly float LipSuckLB;

    [OSCInfo("/sl/xrfb/facew/LipSuckLT", 3)]
    public readonly float LipSuckLT;

    [OSCInfo("/sl/xrfb/facew/LipSuckRB", 3)]
    public readonly float LipSuckRB;

    [OSCInfo("/sl/xrfb/facew/LipSuckRT", 3)]
    public readonly float LipSuckRT;

    [OSCInfo("/sl/xrfb/facew/LipTightenerL", 3)]
    public readonly float LipTightenerL;

    [OSCInfo("/sl/xrfb/facew/LipTightenerR", 3)]
    public readonly float LipTightenerR;

    [OSCInfo("/sl/xrfb/facew/LipsToward", 3)]
    public readonly float LipsToward;

    [OSCInfo("/sl/xrfb/facew/LowerLipDepressorL", 3)]
    public readonly float LowerLipDepressorL;

    [OSCInfo("/sl/xrfb/facew/LowerLipDepressorR", 3)]
    public readonly float LowerLipDepressorR
    ;
    [OSCInfo("/sl/xrfb/facew/MouthLeft", 3)]
    public readonly float MouthLeft;

    [OSCInfo("/sl/xrfb/facew/MouthRight", 3)]
    public readonly float MouthRight;

    [OSCInfo("/sl/xrfb/facew/NoseWrinklerL", 3)]
    public readonly float NoseWrinklerL;

    [OSCInfo("/sl/xrfb/facew/NoseWrinklerR", 3)]
    public readonly float NoseWrinklerR;

    [OSCInfo("/sl/xrfb/facew/OuterBrowRaiserL", 3)]
    public readonly float OuterBrowRaiserL;

    [OSCInfo("/sl/xrfb/facew/OuterBrowRaiserR", 3)]
    public readonly float OuterBrowRaiserR;

    [OSCInfo("/sl/xrfb/facew/UpperLidRaiserL", 3)]
    public readonly float UpperLidRaiserL;

    [OSCInfo("/sl/xrfb/facew/UpperLidRaiserR", 3)]
    public readonly float UpperLidRaiserR;

    [OSCInfo("/sl/xrfb/facew/UpperLipRaiserL", 3)]
    public readonly float UpperLipRaiserL;

    [OSCInfo("/sl/xrfb/facew/UpperLipRaiserR", 3)]
    public readonly float UpperLipRaiserR;





    [OSCInfo("/sl/xrfb/facew/TongueTipInterdental", 4)]
    public readonly float TongueTipInterdental;

    [OSCInfo("/sl/xrfb/facew/TongueTipAlveolar", 4)]
    public readonly float TongueTipAlveolar;

    [OSCInfo("/sl/xrfb/facew/FrontDorsalPalate", 4)]
    public readonly float FrontDorsalPalate;

    [OSCInfo("/sl/xrfb/facew/MidDorsalPalate", 4)]
    public readonly float MidDorsalPalate;

    [OSCInfo("/sl/xrfb/facew/BackDorsalVelar", 4)]
    public readonly float BackDorsalVelar;

    [OSCInfo("/sl/xrfb/facew/TongueOut", 4)]
    public readonly float TongueOut;

    [OSCInfo("/sl/xrfb/facew/TongueRetreat", 4)]
    public readonly float TongueRetreat;
}



[AttributeUsage(AttributeTargets.Field)]
public class OSCInfoAttribute(string path, int bundleId = -1) : Attribute
{
    public readonly OSCInfo Info = new();
}



public static class SteamLinkHelpers
{
    public static OSCInfo[] GenerateOSCInfo<T>() where T : unmanaged, IParameterStruct
    {
        FieldInfo[] fields = typeof(T).GetFields();
        if (!fields.Any(field => field.GetCustomAttributes().Any(attr => attr is OSCInfoAttribute)))
            return [];

        List<OSCInfo> infos = [];
        foreach (FieldInfo field in fields)
        {
            if (field.GetCustomAttribute<OSCInfoAttribute>() is OSCInfoAttribute attr)
            {
                infos.Add(attr.Info);
            }
        }

        return [.. infos];
    }



    public static void Serialize<T>(this T paramStruct, Span<KOscBundle> bundles, Span<KOscMessage> messages) where T : unmanaged, IParameterStruct
    {
        // int lastBundleId = 0;

        // paramStruct;
    }
}