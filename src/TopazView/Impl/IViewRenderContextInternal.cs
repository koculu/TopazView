using System.Buffers;

namespace Tenray.TopazView.Impl;

internal interface IViewRenderContextInternal : IViewRenderContext, IViewStringRendererContext
{
    IBufferWriter<byte> BufferWriter { get; }

    ICompiledViewInternal Layout { get; set; }

    ICompiledViewInternal Body { get; set; }

    ICompiledViewInternal RenderingNow { get; set; }

    string RenderingPath { get; set; }

    ViewFlags ViewFlags { get; set; }

    bool IsRenderingPartNow(string part);

    void MarkPartRenderingNow(string part);

    void MarkPartRenderingDone(string part);
}
