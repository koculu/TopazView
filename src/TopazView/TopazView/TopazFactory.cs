using System.Text.Encodings.Web;
using Tenray.Topaz;
using Tenray.Topaz.API;
using Tenray.Topaz.Options;

namespace Tenray.TopazView;

public class TopazFactory : ITopazFactory
{
    readonly Func<object, object> EncodeText;

    TextEncoder TextEncoder { get; }

    public TopazFactory(TextEncoder textEncoder)
    {
        TextEncoder = textEncoder;
        EncodeText = (value) =>
        {
            if (value == null)
                return string.Empty;
            return TextEncoder.Encode(value.ToString());
        };
    }

    public TopazEngine CreateTopazEngine()
    {
        var engine = new TopazEngine(new TopazEngineSetup
        {
            IsThreadSafe = false
        });
        engine.Options.AllowNullReferenceMemberAccess = false;
        engine.Options.AllowUndefinedReferenceMemberAccess = false;
        engine.Options.AllowUndefinedReferenceAccess = false;
        engine.Options.NoUndefined = true;
        engine.Options.VarScopeBehavior = VarScopeBehavior.FunctionScope;
        engine.AddType<DateTime>("DateTime");
        ConfigureGlobalObjects(engine);
        return engine;
    }

    void ConfigureGlobalObjects(TopazEngine topazEngine)
    {
        topazEngine.SetValue("encode", EncodeText);
        topazEngine.SetValue("JSON", new JSONObject());
    }
}
