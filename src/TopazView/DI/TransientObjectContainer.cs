using System.Text.Encodings.Web;
using Tenray.TopazView.Exceptions;
using Tenray.TopazView.Impl;

namespace Tenray.TopazView.DI;

internal sealed class TransientObjectContainer :
    IPageProvider, IContentProviderProvider, IJavascriptEngineProvider
{
    public UserFactories UserFactories { get; } = new();

    public UserInstances UserInstances { get; } = new();

    IViewEngineComponents GetViewEngineComponents()
    {
        return new ViewEngineComponents(
            GetJavascriptEngine(),
            GetViewRepository(),
            GetViewCompiler(),
            GetContentProvider());
    }

    IViewCompiler GetViewCompiler()
    {
        return new ViewCompiler(this);
    }

    IViewRepository GetViewRepository()
    {
        return new ViewRepository(this);
    }

    ITopazFactory GetTopazFactory()
    {
        if (UserInstances.TopazFactory != null)
            return UserInstances.TopazFactory;
        if (UserFactories.TopazFactoryFactory != null)
            return UserFactories.TopazFactoryFactory();
        return new TopazFactory(GetTextEncoder());
    }

    TextEncoder GetTextEncoder()
    {
        if (UserInstances.TextEncoder != null)
            return UserInstances.TextEncoder;
        if (UserFactories.TextEncoderFactory != null)
            return UserFactories.TextEncoderFactory();
        return HtmlEncoder.Default;
    }

    public IJavascriptEngine GetJavascriptEngine()
    {
        return new JavascriptEngine(GetTopazFactory());
    }

    public IContentProvider GetContentProvider()
    {
        if (UserInstances.ContentProvider != null)
            return UserInstances.ContentProvider;
        if (UserFactories.ContentProviderFactory != null)
            return UserFactories.ContentProviderFactory();
        throw new MissingConfigurationException("Content provider is not configured.");
    }

    public IPage GetPage()
    {
        return new Page(GetTextEncoder());
    }

    public IViewEngine GetViewEngine()
    {
        return new ViewEngine(GetViewEngineComponents(), this, this);
    }
}
