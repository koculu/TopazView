using System.Buffers;
using System.Text;
using Tenray.Topaz.API;

namespace TopazView.UnitTests;

public class TestBasicRendering
{
    [Test]
    public void TestBufferWriter()
    {
        var viewEngine = new ViewEngineFactory()
            .SetContentProvider(new FileSystemContentProvider("../../../test-data"))
            .GetOrCreateViewEngine();

        var bufferWriter = new ArrayBufferWriter<byte>();
        var contentProvider = viewEngine.ContentProvider;

        var view = viewEngine.GetOrCreateView("simple.view").GetCompiledView();
        var context = viewEngine.CreateViewRenderContext(bufferWriter);

        dynamic model = context.Model = new JsObject();
        model.Title = "Test Basic Use Case";
        model.Message = "Hello world!";

        view.RenderViewNoLayout(context);

        var text = Encoding.UTF8.GetString(bufferWriter.WrittenSpan);
        var expected = contentProvider.GetContent("simple.rendered.view");
        Console.WriteLine(text);

        Assert.That(text, Is.EqualTo(expected));
    }

    [Test]
    public void TestMemoryStream()
    {
        var viewEngine = new ViewEngineFactory()
            .SetContentProvider(new FileSystemContentProvider("../../../test-data"))
            .GetOrCreateViewEngine();

        using var memoryStream = new MemoryStream();
        var contentProvider = viewEngine.ContentProvider;

        var view = viewEngine.GetOrCreateView("simple.view").GetCompiledView();
        var context = viewEngine.CreateViewRenderContext(memoryStream);

        dynamic model = context.Model = new JsObject();
        model.Title = "Test Basic Use Case";
        model.Message = "Hello world!";

        view.RenderViewNoLayout(context);
        memoryStream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(memoryStream, Encoding.UTF8);
        var text = reader.ReadToEnd();
        var expected = contentProvider.GetContent("simple.rendered.view");

        Assert.That(text, Is.EqualTo(expected));
    }

    [Test]
    public void TestRenderToString()
    {
        var viewEngine = new ViewEngineFactory()
            .SetContentProvider(new FileSystemContentProvider("../../../test-data"))
            .GetOrCreateViewEngine();

        var contentProvider = viewEngine.ContentProvider;

        var view = viewEngine.GetOrCreateView("simple.view").GetCompiledView();
        var context = viewEngine.CreateViewRenderContext();

        dynamic model = context.Model = new JsObject();
        model.Title = "Test Basic Use Case";
        model.Message = "Hello world!";

        var text = view.RenderViewNoLayoutToString(context);
        var expected = contentProvider.GetContent("simple.rendered.view");

        Assert.That(text, Is.EqualTo(expected));
    }
}
