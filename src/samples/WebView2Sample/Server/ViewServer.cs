using System;
using System.Net;
using System.Threading.Tasks;
using Tenray.Topaz.API;
using Tenray.TopazView;

namespace WebView2Sample.Server;

class ViewServer : IViewServer
{
    private IViewEngine ViewEngine;

    public ViewServer(ViewEngineFactory viewEngineFactory, FileSystemContentWatcher contentWatcher)
    {
        var path = "../../../web/views";
        ViewEngine = viewEngineFactory
            // Enable the line below to customize script environment.
            //.SetTopazFacory(new CustomTopazFactory()) 
            .SetContentProvider(new FileSystemContentProvider(path))
            .CreateViewEngine();
        contentWatcher.StartWatcher(path, ViewEngine);
    }

    public async ValueTask<ViewResult> GetView(string uri)
    {
        var context = ViewEngine.CreateViewRenderContext();
        context.MaximumScriptDuration = TimeSpan.FromSeconds(2);
        context.Model = new { Title = "Topaz View for WebView2" }.ToJsObject();
        var html = await ViewEngine.GetOrCreateView(uri).GetCompiledView().RenderViewToString(context);
        return new(HttpStatusCode.OK, html);
    }
}
