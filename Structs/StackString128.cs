using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Edrakon.Structs;

[InlineArray(SIZE)]
public unsafe struct StackString128
{
    public const int SIZE = 128;

    public readonly int Length
    {
        get
        {
            int count;
            for (count = 0; count < SIZE; count++)
                if (this[count] == 0)
                    break;

            return count;
        }
    }


    public unsafe Span<byte> Span
    {
        get
        {
            fixed (byte* ptr = &_element)
                return new(ptr, SIZE);
        }
    }

    public ref byte Bytes => ref MemoryMarshal.GetReference(Span);


    private byte _element;

    public unsafe StackString128(string str)
    {
        int requiredBytes = Encoding.UTF8.GetByteCount(str);

        if (requiredBytes > SIZE)
            throw new IndexOutOfRangeException($"Input string exceeds 128 bytes of UTF8 characters!");


        fixed (byte* ptr = &_element)
            Encoding.UTF8.GetBytes(str, Span);
    }


    public static implicit operator string(StackString128 other) => Encoding.UTF8.GetString(other.Span);
    public static implicit operator StackString128(string other) => new(other);
}


[InlineArray(SIZE)]
public unsafe struct StackString256
{
    public const int SIZE = 256;

    public readonly int Length
    {
        get
        {
            int count;
            for (count = 0; count < SIZE; count++)
                if (this[count] == 0)
                {
                    count++;
                    break;
                }

            return count;
        }
    }

    public unsafe Span<byte> Span
    {
        get
        {
            fixed (byte* ptr = &_element)
                return new(ptr, SIZE);
        }
    }

    public ref byte Bytes => ref MemoryMarshal.GetReference(Span);


    private byte _element;

    public unsafe StackString256(string str)
    {
        int requiredBytes = Encoding.UTF8.GetByteCount(str);

        if (requiredBytes > SIZE)
            throw new IndexOutOfRangeException($"Input string exceeds 128 bytes of UTF8 characters!");


        fixed (byte* ptr = &_element)
            Encoding.UTF8.GetBytes(str, Span);
    }


    public static implicit operator string(StackString256 other) => Encoding.UTF8.GetString(other.Span);
    public static implicit operator StackString256(string other) => new(other);
}