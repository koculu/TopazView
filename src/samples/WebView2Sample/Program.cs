using System;
using Microsoft.Extensions.DependencyInjection;
using WebView2Sample.Views;

namespace WebView2Sample;

static class Program
{
    static readonly Startup Startup = new();

    [STAThread]
    static void Main()
    {
        Startup.RegisterServices();
        var app = new App();
        var mainWindow = Startup.GetServiceProvider().GetService<MainWindow>();
        app.Run(mainWindow);
    }
}
