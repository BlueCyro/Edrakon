using System.Runtime.CompilerServices;
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



    private byte _element;

    public unsafe StackString128(string str)
    {
        int requiredBytes = Encoding.UTF8.GetByteCount(str);

        if (requiredBytes > SIZE)
            throw new IndexOutOfRangeException($"Input string exceeds {SIZE} bytes of UTF8 characters!");

        
        Encoding.UTF8.GetBytes(str, this.AsSpan());
    }


    public static implicit operator string(StackString128 other) => Encoding.UTF8.GetString(other.AsSpan());
    public static implicit operator StackString128(string other) => new(other);
}
