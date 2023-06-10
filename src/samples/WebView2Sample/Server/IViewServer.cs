using System.Threading.Tasks;

namespace WebView2Sample.Server;

public interface IViewServer
{
    ValueTask<ViewResult> GetView(string uri);
}