using System.Text;
using Tenray.Topaz.API;

namespace Tenray.TopazView;

public interface IViewRenderContext
{
    IViewEngine ViewEngine { get; }

    IPage Page { get; set; }

    IJsObject Model { get; set; }

    Encoding Encoding { get; set; }

    TimeSpan MaximumScriptDuration { get; set; }
}
