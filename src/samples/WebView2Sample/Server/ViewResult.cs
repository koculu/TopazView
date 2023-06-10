using System.Net;

namespace WebView2Sample.Server;

public readonly struct ViewResult
{
    public HttpStatusCode Status { get; }

    public string Html { get; }

    public ViewResult(HttpStatusCode status, string html)
    {
        Status = status;
        Html = html;
    }

    public void Deconstruct(out HttpStatusCode status, out string html)
    {
        status = Status;
        html = Html;
    }
}