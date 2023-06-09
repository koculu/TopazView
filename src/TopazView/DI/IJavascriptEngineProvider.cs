using Tenray.TopazView.Impl;

namespace Tenray.TopazView.DI;

internal interface IJavascriptEngineProvider
{
    IJavascriptEngine GetJavascriptEngine();
}
