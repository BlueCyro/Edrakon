using System.Runtime.InteropServices;

namespace Edrakon.Structs;

public static class StackStringHelpers
{
    public static Span<byte> AsSpan(this ref StackString128 str) => MemoryMarshal.Cast<StackString128, byte>(new Span<StackString128>(ref str));
    public static Span<byte> AsSpan(this ref StackString256 str) => MemoryMarshal.Cast<StackString256, byte>(new Span<StackString256>(ref str));
}