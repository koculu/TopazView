using System.Buffers;
using System.Text;
using Tenray.Topaz.API;
using Tenray.TopazView.Utility;

namespace Tenray.TopazView.Impl;

internal sealed class ViewRenderContext : IViewRenderContextInternal
{
    public IViewEngine ViewEngine { get; }

    public IBufferWriter<byte> BufferWriter { get; }

    public ICompiledViewInternal Body { get; set; }

    public ICompiledViewInternal Layout { get; set; }

    public ICompiledViewInternal RenderingNow { get; set; }

    public IPage Page { get; set; }

    /// <summary>
    /// The model is IJsObject to avoid reflection.
    /// </summary>
    public IJsObject Model { get; set; }

    public string RenderingPath { get; set; }

    HashSet<string> RenderingNowSet { get; } = new();

    public ViewFlags ViewFlags { get; set; }

    public Encoding Encoding { get; set; } = Encoding.UTF8;

    public string RenderedString => Encoding.GetString(
        ((MemoryBufferWriter<byte>)BufferWriter).WrittenSpan);

    public ViewRenderContext(
        IViewEngine viewEngine,
        IBufferWriter<byte> bufferWriter)
    {
        ViewEngine = viewEngine;
        BufferWriter = bufferWriter;
    }

    public bool IsRenderingPartNow(string part)
    {
        return RenderingNowSet.Contains(part);
    }

    public void MarkPartRenderingNow(string part)
    {
        RenderingNowSet.Add(part);
    }

    public void MarkPartRenderingDone(string part)
    {
        RenderingNowSet.Remove(part);
    }
}
