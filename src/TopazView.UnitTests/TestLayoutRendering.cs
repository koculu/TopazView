using Tenray.Topaz.API;

namespace TopazView.UnitTests;

public class TestLayout
{
    [Test]
    public void TestLayoutRendering()
    {
        var viewEngine = new ViewEngineFactory()
            .SetContentProvider(new FileSystemContentProvider("../../../test-data"))
            .CreateViewEngine();

        var contentProvider = viewEngine.ContentProvider;

        var view = viewEngine.GetOrCreateView("basic/subfolder/test1.view").GetCompiledView();
        var context = viewEngine.CreateViewRenderContext();

        dynamic model = context.Model = new JsObject();
        model.Title = "Test Basic Use Case";
        model.Message = "Hello world!";

        var text = view.RenderViewToString(context).GetAwaiter().GetResult();
        var expected = contentProvider.GetContent("/basic/subfolder/test1.rendered.view");

        Console.WriteLine(text);
        Assert.That(text, Is.EqualTo(expected));
    }

    [Test]
    public void TestViewScript()
    {
        var viewEngine = new ViewEngineFactory()
            .SetContentProvider(new FileSystemContentProvider("../../../test-data"))
            .CreateViewEngine();

        var contentProvider = viewEngine.ContentProvider;

        var view = viewEngine.GetOrCreateView("basic/subfolder/test2.view").GetCompiledView();
        var context = viewEngine.CreateViewRenderContext();

        dynamic model = context.Model = new JsObject();
        model.Title = "Test Basic Use Case";
        model.Message = "Hello world!";

        var text = view.RenderViewToString(context).GetAwaiter().GetResult();
        var expected = contentProvider.GetContent("/basic/subfolder/test2.rendered.view");

        Console.WriteLine(text);
        Assert.That(text, Is.EqualTo(expected));
    }

    [Test]
    public void TestParameteredSection()
    {
        var viewEngine = new ViewEngineFactory()
            .SetContentProvider(new FileSystemContentProvider("../../../test-data"))
            .CreateViewEngine();

        var contentProvider = viewEngine.ContentProvider;

        var view = viewEngine.GetOrCreateView("basic/subfolder/test3.view").GetCompiledView();
        var context = viewEngine.CreateViewRenderContext();

        context.Model = new JsObject();

        var text = view.RenderViewToString(context).GetAwaiter().GetResult();
        var expected = contentProvider.GetContent("/basic/subfolder/test3.rendered.view");

        Console.WriteLine(text);
        Assert.That(text, Is.EqualTo(expected));
    }

    [Test]
    public void TestElseIf()
    {
        var viewEngine = new ViewEngineFactory()
            .SetContentProvider(new FileSystemContentProvider("../../../test-data"))
            .CreateViewEngine();

        var contentProvider = viewEngine.ContentProvider;

        var view = viewEngine.GetOrCreateView("basic/subfolder/test4.view").GetCompiledView();
        var context = viewEngine.CreateViewRenderContext();

        context.Model = new JsObject();

        var text = view.RenderViewToString(context).GetAwaiter().GetResult();
        var expected = contentProvider.GetContent("/basic/subfolder/test4.rendered.view");

        Console.WriteLine(text);
        Assert.That(text, Is.EqualTo(expected));
    }
}