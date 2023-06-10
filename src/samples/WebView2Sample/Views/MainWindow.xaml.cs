using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using Tenray.TopazView;
using WebView2Sample.Server;

namespace WebView2Sample.Views;

public partial class MainWindow : Window
{
    readonly IViewServer ViewServer;

    public MainWindow(IViewServer viewServer, FileSystemContentWatcher contentWatcher)
    {
        InitializeComponent();
        web.CoreWebView2InitializationCompleted += CoreWebView2InitializationCompleted;
        ViewServer = viewServer;
        contentWatcher.OnViewCacheInvalidated +=
            () => Dispatcher.Invoke(() => web.CoreWebView2.Reload());
    }

    void CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        web.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
        web.CoreWebView2.WebResourceRequested += WebResourceRequested;

        try
        {
            web.CoreWebView2.SetVirtualHostNameToFolderMapping(
                                "web", "../../../web",
                                CoreWebView2HostResourceAccessKind.Allow);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
            Environment.Exit(1);
        }
    }

    string GetViewPath(string uri)
    {
        return uri[uri.IndexOf('/', 7)..] + ".view";
    }

    void WebResourceRequested(object sender,
       CoreWebView2WebResourceRequestedEventArgs e)
    {
        try
        {
            var uri = new Uri(e.Request.Uri);
            switch (uri.Host)
            {
                case "app.example":
                    e.Response = CreateViewResponse(e);
                    break;
                case "api.example":
                    break;
            }
        }
        catch (Exception ex)
        {
            e.Response = CreateHtmlResponse(HttpStatusCode.InternalServerError, ex.ToString());
        }
    }

    private CoreWebView2WebResourceResponse CreateViewResponse(CoreWebView2WebResourceRequestedEventArgs e)
    {
        var request = e.Request;
        var task = ViewServer.GetView(GetViewPath(request.Uri)).AsTask();
        task.Wait();
        (var status, var html) = task.Result;
        var res = CreateHtmlResponse(status, html);
        return res;
    }

    CoreWebView2WebResourceResponse CreateHtmlResponse(
        HttpStatusCode status,
        string html)
    {
        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(html));
        return web.CoreWebView2.Environment.CreateWebResourceResponse(
            memoryStream,
            (int)status,
            status.ToString(),
            null);
    }
}
