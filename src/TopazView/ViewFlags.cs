namespace Tenray.TopazView;

[Flags]
public enum ViewFlags
{
    /// <summary>
    /// Javascript Engine does not belong to any view.
    /// The default option for best performance.
    /// Views should not modify global objects that might affect other compiled views.
    /// </summary>
    None = 0,

    /// <summary>
    /// Javascript engine belongs to the view.
    /// </summary>
    PrivateJavascriptEngine = 1
}
