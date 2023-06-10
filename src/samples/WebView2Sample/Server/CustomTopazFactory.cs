using Tenray.Topaz;
using Tenray.TopazView;

namespace WebView2Sample.Server;

class CustomTopazFactory : ITopazFactory
{
    public CustomTopazFactory()
    {
    }

    public TopazEngine CreateTopazEngine()
    {
        var topazEngine = new TopazFactory().CreateTopazEngine();

        // Expose .NET types to the script according to your needs.
        // See https://github.com/koculu/Topaz documentation for more.
        // topazEngine.AddType(typeof(Dictionary<,>), "Dictionary");

        return topazEngine;
    }
}
