using System.Buffers;

namespace Tenray.TopazView.Utility;

internal readonly struct StreamBufferWriter : IBufferWriter<byte>
{
    readonly MemoryBufferWriter<byte> BufferWriter;

    readonly Stream Stream;

    public StreamBufferWriter(Stream stream)
    {
        Stream = stream;
        BufferWriter = new();
    }

    public void Advance(int count)
    {
        BufferWriter.Advance(count);
        Stream.Write(BufferWriter.WrittenSpan.Slice(0, count));
        BufferWriter.Clear();
    }

    public Memory<byte> GetMemory(int sizeHint = 0) => BufferWriter.GetMemory(sizeHint);

    public Span<byte> GetSpan(int sizeHint = 0) => BufferWriter.GetSpan(sizeHint);
}
