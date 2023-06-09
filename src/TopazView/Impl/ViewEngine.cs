using System.Buffers;
using Tenray.TopazView.DI;
using Tenray.TopazView.Utility;

namespace Tenray.TopazView.Impl;

internal sealed class ViewEngine : IViewEngine
{
    IViewRepository ViewRepository { get; }

    IPageProvider PageProvider { get; }

    IContentProviderProvider ContentProviderProvider { get; }

    public IContentProvider ContentProvider => ContentProviderProvider.GetContentProvider();

    public ViewEngine(
        IViewEngineComponents viewEngineComponents,
        IPageProvider pageProvider,
        IContentProviderProvider contentProviderProvider)
    {
        ViewRepository = viewEngineComponents.ViewRepository;
        PageProvider = pageProvider;
        ContentProviderProvider = contentProviderProvider;
    }

    public IView GetOrCreateView(string path, ViewFlags flags)
    {
        return ViewRepository.GetOrCreateView(path, flags);
    }

    public void DropViewByKey(string key, TimeSpan delayDispose)
    {
        ViewRepository.DropViewByKey(key, delayDispose);
    }

    public void DropViewByPath(string path, ViewFlags viewFlags, TimeSpan delayDispose)
    {
        ViewRepository.DropViewByPath(path, viewFlags, delayDispose);
    }

    public void DropAll(TimeSpan delayDispose)
    {
        ViewRepository.DropAll(delayDispose);
    }

    public IViewRenderContext CreateViewRenderContext(
        IBufferWriter<byte> bufferWriter)
    {
        var renderContext = new ViewRenderContext(this, bufferWriter)
        {
            Page = PageProvider.GetPage()
        };
        ((Page)renderContext.Page).SetViewRenderContext(renderContext);
        return renderContext;
    }

    public IViewRenderContext CreateViewRenderContext(Stream stream)
        => CreateViewRenderContext(new StreamBufferWriter(stream));

    public IViewStringRendererContext CreateViewRenderContext()
    {
        var bufferWriter = new MemoryBufferWriter<byte>();
        return (IViewStringRendererContext)CreateViewRenderContext(bufferWriter);
    }
}
