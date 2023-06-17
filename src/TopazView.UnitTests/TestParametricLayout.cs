using System.Text;
using Tenray.Topaz.API;

namespace TopazView.UnitTests;

public class TestParametricLayout
{
    [Test]
    public void TestScopeLayoutWithParameters()
    {
        var viewEngine = new ViewEngineFactory()
            .SetContentProvider(new FileSystemContentProvider("../../../test-data"))
            .CreateViewEngine();

        var contentProvider = viewEngine.ContentProvider;

        var view = viewEngine.GetOrCreateView("basic/subfolder/test5.view").GetCompiledView();
        var context = viewEngine.CreateViewRenderContext();
        context.Model = new JsObject();
        var text = view.RenderViewToString(context).GetAwaiter().GetResult();
        var expected = contentProvider.GetContent("/basic/subfolder/test5.rendered.view");

        Console.WriteLine(text);
        Assert.That(text, Is.EqualTo(expected));
    }
}
