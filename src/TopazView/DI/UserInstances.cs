using System.Text.Encodings.Web;

namespace Tenray.TopazView.DI;

internal sealed class UserInstances
{
    public IContentProvider ContentProvider { get; set; }

    public TextEncoder TextEncoder { get; set; }

    public ITopazFactory TopazFactory { get; set; }
}
