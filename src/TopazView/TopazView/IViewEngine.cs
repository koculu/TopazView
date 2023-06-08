using System.Buffers;

namespace Tenray.TopazView;

public interface IViewEngine
{
    IContentProvider ContentProvider { get; }

    IView GetOrCreateView(string path, ViewFlags flags = ViewFlags.None);

    /// <summary>
    /// </summary>
    /// <param name="key">key = path + (int)ViewFlags</param>
    /// <param name="delayDispose"></param>
    void DropViewByKey(string key, TimeSpan delayDispose);

    void DropViewByPath(string path, ViewFlags viewFlags, TimeSpan delayDispose);

    void DropAll(TimeSpan delayDispose);

    IViewRenderContext CreateViewRenderContext(IBufferWriter<byte> bufferWriter);

    IViewRenderContext CreateViewRenderContext(Stream stream);

    IViewStringRendererContext CreateViewRenderContext();
}
