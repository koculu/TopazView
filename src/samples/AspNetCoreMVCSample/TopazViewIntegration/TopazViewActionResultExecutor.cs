using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Tenray.Topaz.API;
using Tenray.TopazView;

namespace TestMVC.Reg;

class TopazViewActionResultExecutor : IActionResultExecutor<ViewResult>
{
    public IViewEngine ViewEngine { get; }

    public TopazViewActionResultExecutor(IViewEngine viewEngine)
    {
        ViewEngine = viewEngine;
    }

    public async Task ExecuteAsync(ActionContext context, ViewResult result)
    {
        var paths = context.ActionDescriptor.RouteValues.OrderByDescending(x => x.Key).Select(x => x.Value);
        var path = string.Join("/", paths) + ".view";
        var bodyWriter = context.HttpContext.Response.BodyWriter;
        var renderContext = ViewEngine.CreateViewRenderContext(bodyWriter);

        // You can convert any object to JsObject.
        renderContext.Model = result.Model?
            .ToJsObject(JsObjectConverterOption.UseLowerCasePropertyNames);
        /*
         * provide any custom object to the Page.data according to your needs.
         * var pageData = renderContext.Page.data;
         * pageData.SetValue("ViewData", result.ViewData);
         * pageData.SetValue("TempData", result.TempData);*/

        await ViewEngine.GetOrCreateView(path).GetCompiledView().RenderView(renderContext);
    }
}
