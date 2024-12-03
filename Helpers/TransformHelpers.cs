using System.Numerics;
using System.Runtime.CompilerServices;
using Silk.NET.OpenXR;

namespace Edrakon.Helpers;

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
