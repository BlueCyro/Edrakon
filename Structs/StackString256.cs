using System.Runtime.CompilerServices;
using System.Text;

namespace Edrakon.Structs;

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

    private byte _element;

    public unsafe StackString256(string str)
    {
        int requiredBytes = Encoding.UTF8.GetByteCount(str);

        if (requiredBytes > SIZE)
            throw new IndexOutOfRangeException($"Input string exceeds {SIZE} bytes of UTF8 characters!");

        Span<byte> bytes = this.AsSpan();

        Encoding.UTF8.GetBytes(str, bytes);
    }


    public static implicit operator string(StackString256 other) => Encoding.UTF8.GetString(other.AsSpan());
    public static implicit operator StackString256(string other) => new(other);
}
