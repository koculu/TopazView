using System.Buffers;

namespace Tenray.TopazView.Utility;

/// <summary>
/// The modified ArrayBufferWriter.
/// 1. Clear method does not clear the bytes. (faster)
/// 2. Not critical boundary checks are removed. (faster)
/// </summary>
/// <typeparam name="T">Type of the buffer.</typeparam>
internal sealed class MemoryBufferWriter<T> : IBufferWriter<T>
{
    const int DefaultInitialBufferSize = 256;

    T[] Buffer;

    int Index;

    public ReadOnlyMemory<T> WrittenMemory => Buffer.AsMemory(0, Index);

    public ReadOnlySpan<T> WrittenSpan => Buffer.AsSpan(0, Index);

    public int WrittenCount => Index;

    public int Capacity => Buffer.Length;

    public int FreeCapacity => Buffer.Length - Index;

    public MemoryBufferWriter()
    {
        Buffer = Array.Empty<T>();
        Index = 0;
    }

    public MemoryBufferWriter(int initialCapacity)
    {
        if (initialCapacity <= 0)
            throw new ArgumentException(null, nameof(initialCapacity));

        Buffer = new T[initialCapacity];
        Index = 0;
    }

    public void Clear()
    {
        Index = 0;
    }

    public void Advance(int count)
    {
        Index += count;
    }

    public Memory<T> GetMemory(int sizeHint = 0)
    {
        CheckAndResizeBuffer(sizeHint);
        return Buffer.AsMemory(Index);
    }

    public Span<T> GetSpan(int sizeHint = 0)
    {
        CheckAndResizeBuffer(sizeHint);
        return Buffer.AsSpan(Index);
    }

    void CheckAndResizeBuffer(int sizeHint)
    {
        if (sizeHint <= FreeCapacity)
            return;

        if (sizeHint <= 0)
        {
            sizeHint = 1;
        }

        int currentLength = Buffer.Length;

        // Attempt to grow at least double the current size.
        int growBy = Math.Max(sizeHint, currentLength);

        if (currentLength == 0)
        {
            growBy = Math.Max(growBy, DefaultInitialBufferSize);
        }

        int newSize = currentLength + growBy;

        if ((uint)newSize > int.MaxValue)
        {
            newSize = Array.MaxLength;
        }

        Array.Resize(ref Buffer, newSize);
    }
}