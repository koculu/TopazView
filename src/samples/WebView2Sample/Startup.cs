using SimpleInjector;
using System;
using Tenray.TopazView;
using WebView2Sample.Server;
using WebView2Sample.Views;

namespace WebView2Sample;

class Startup
{
    readonly Container Container = new();

    public void RegisterServices()
    {
        Container.Options.ResolveUnregisteredConcreteTypes = true;
        Container.Options.EnableAutoVerification = false;
        Container.Options.SuppressLifestyleMismatchVerification = true;
        Container.RegisterInstance<IServiceProvider>(Container);
        Container.Register<MainWindow>();
        Container.RegisterSingleton<IViewServer, ViewServer>();
        Container.RegisterSingleton(() => new FileSystemContentWatcher());
        Container.RegisterSingleton<ViewEngineFactory>();
    }

    public IServiceProvider GetServiceProvider() => Container;
}
