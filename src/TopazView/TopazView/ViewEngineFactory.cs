using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using System.Text.Encodings.Web;
using Tenray.TopazView.Exceptions;
using Tenray.TopazView.Impl;

namespace Tenray.TopazView;

public class ViewEngineFactory
{
    Container Container { get; set; }

    IContentProvider ContentProvider;

    Func<IContentProvider> ContentProviderFactory;

    Lifestyle ContentProviderLifestyle = Lifestyle.Singleton;

    public ViewEngineFactory SetContentProvider(IContentProvider contentProvider)
    {
        if (Container != null)
            throw new ViewFactoryException("Can not set content provider after the factory initialization.");
        ContentProvider = contentProvider;
        ContentProviderFactory = null;
        return this;
    }

    public ViewEngineFactory SetContentProviderFactory(Func<IContentProvider> contentProviderFactory)
    {
        if (Container != null)
            throw new ViewFactoryException("Can not set content provider factory after factory initialization.");
        ContentProviderFactory = contentProviderFactory;
        ContentProvider = null;
        return this;
    }

    public ViewEngineFactory SetContentProviderServiceLifetime(ServiceLifetime serviceLifetime)
    {
        if (Container != null)
            throw new ViewFactoryException("Can not set content provider service life time after factory initialization.");
        switch (serviceLifetime)
        {
            case ServiceLifetime.Transient: ContentProviderLifestyle = Lifestyle.Transient; break;
            case ServiceLifetime.Scoped: ContentProviderLifestyle = Lifestyle.Scoped; break;
            case ServiceLifetime.Singleton: ContentProviderLifestyle = Lifestyle.Singleton; break;
        }
        return this;
    }

    public ViewEngineFactory Initialize(TextEncoder textEncoder = null)
    {
        if (Container != null)
            return this;
        return Initialize<TopazFactory>(textEncoder);
    }

    public ViewEngineFactory Initialize<TTopazFactory>(TextEncoder textEncoder = null)
        where TTopazFactory : class, ITopazFactory
    {
        if (Container != null)
            return this;
        SetupServices();
        Container.RegisterInstance(textEncoder ?? HtmlEncoder.Default);
        Container.Register<ITopazFactory, TTopazFactory>();
        return this;
    }

    public ViewEngineFactory Initialize(Func<ITopazFactory> topazFactory, TextEncoder textEncoder = null)
    {
        if (Container != null)
            return this;
        SetupServices();
        Container.Register(topazFactory);
        Container.RegisterInstance(textEncoder ?? HtmlEncoder.Default);
        return this;
    }

    private void SetupServices()
    {
        EnsureContentProviderIsConfigured();
        Container = new();
        Container.Options.EnableAutoVerification = false;
        Container.Options.SuppressLifestyleMismatchVerification = true;
        Container.RegisterInstance<IServiceProvider>(Container);
        RegisterContentProvider();
        Container.Register<IPage, Page>();
        Container.Register<IJavascriptEngine, JavascriptEngine>();
        Container.Register<IViewCompiler, ViewCompiler>();
        Container.Register<IViewRepository, ViewRepository>();
        Container.RegisterSingleton<IViewEngine, ViewEngine>();
        Container.RegisterSingleton<IViewEngineComponents, ViewEngineComponents>();
    }

    void EnsureContentProviderIsConfigured()
    {
        if (ContentProvider == null && ContentProviderFactory == null)
            throw new ViewFactoryException("Initialization failed. Content provider is not configured.");
    }

    void RegisterContentProvider()
    {
        if (ContentProvider != null)
            Container.RegisterInstance(ContentProvider);
        else if (ContentProviderFactory != null)
            Container.Register(ContentProviderFactory, ContentProviderLifestyle);
        else
            throw new ViewFactoryException("Content Provider is not configured.");
    }

    public IViewEngine GetOrCreateViewEngine()
    {
        Initialize();
        return Container.GetService<IViewEngine>();
    }
}

// TODO: Implement developer friendly error page similar to razor.
// https://github.com/dotnet/aspnetcore/tree/c2488eead6ead7208f543d0a57104b5d167b93f9/src/Middleware/Diagnostics/src/DeveloperExceptionPage/Views
