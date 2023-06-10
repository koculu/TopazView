using System.Text.Encodings.Web;

namespace Tenray.TopazView.DI;

internal sealed class UserFactories
{
    public Func<ITopazFactory> TopazFactoryFactory { get; set; }

    public Func<IContentProvider> ContentProviderFactory { get; set; }

    public Func<TextEncoder> TextEncoderFactory { get; set; }
}
