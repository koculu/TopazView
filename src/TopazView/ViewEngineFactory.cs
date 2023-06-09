using System.Text.Encodings.Web;
using Tenray.TopazView.DI;
using Tenray.TopazView.Exceptions;

namespace Tenray.TopazView;

public class ViewEngineFactory
{
    Container Container { get; set; }

    public IContentProvider ContentProvider { get; set; }

    public Func<IContentProvider> ContentProviderFactory { get; set; }

    public Func<TextEncoder> TextEncoderFactory { get; set; }

    public TextEncoder TextEncoder { get; set; }

    public ITopazFactory TopazFactory { get; set; }

    public Func<ITopazFactory> TopazFactoryFactory { get; set; }

    public IViewEngine CreateViewEngine()
    {
        SetupServices();
        return Container.TransientObjectContainer.GetViewEngine();
    }

    void SetupServices()
    {
        if (Container != null)
            return;
        EnsureContentProviderIsConfigured();
        Container = new();
        SetupUserInstances();
        SetupUserFactories();
        return;
    }

    void EnsureContentProviderIsConfigured()
    {
        if (ContentProvider == null && ContentProviderFactory == null)
            throw new MissingConfigurationException("Initialization failed. Content provider is not configured.");
    }

    void SetupUserFactories()
    {
        Container.UserFactories.TopazFactoryFactory = TopazFactoryFactory;
        Container.UserFactories.ContentProviderFactory = ContentProviderFactory;
        Container.UserFactories.TextEncoderFactory = TextEncoderFactory;
    }

    void SetupUserInstances()
    {
        Container.UserInstances.TopazFactory = TopazFactory;
        Container.UserInstances.TextEncoder = TextEncoder ?? HtmlEncoder.Default;
        Container.UserInstances.ContentProvider = ContentProvider;
    }

    #region Fluent factory methods

    public ViewEngineFactory SetContentProvider(IContentProvider contentProvider)
    {
        ContentProvider = contentProvider;
        return this;
    }

    public ViewEngineFactory SetContentProvider(Func<IContentProvider> contentProviderFactory)
    {
        ContentProviderFactory = contentProviderFactory;
        return this;
    }

    public ViewEngineFactory SetTextEncoder(TextEncoder textEncoder)
    {
        TextEncoder = textEncoder;
        return this;
    }

    public ViewEngineFactory SetTextEncoder(Func<TextEncoder> textEncoderFactory)
    {
        TextEncoderFactory = textEncoderFactory;
        return this;
    }

    public ViewEngineFactory SetTopazFacory(ITopazFactory topazFactory)
    {
        TopazFactory = topazFactory;
        return this;
    }

    public ViewEngineFactory SetTopazFacory(Func<ITopazFactory> topazFactoryFactory)
    {
        TopazFactoryFactory = topazFactoryFactory;
        return this;
    }
    #endregion
}

// TODO: Implement developer friendly error page similar to razor.
// https://github.com/dotnet/aspnetcore/tree/c2488eead6ead7208f543d0a57104b5d167b93f9/src/Middleware/Diagnostics/src/DeveloperExceptionPage/Views
