namespace Tenray.TopazView.Impl;

internal sealed class ScriptDef : IEquatable<ScriptDef>
{
    public string Src { get; set; }

    public bool IsModule { get; set; }

    public ScriptDef(string src, bool isModule)
    {
        Src = src;
        IsModule = isModule;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as ScriptDef);
    }

    public bool Equals(ScriptDef other)
    {
        return other != null &&
               Src == other.Src &&
               IsModule == other.IsModule;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Src, IsModule);
    }

    public static bool operator ==(ScriptDef left, ScriptDef right)
    {
        return EqualityComparer<ScriptDef>.Default.Equals(left, right);
    }

    public static bool operator !=(ScriptDef left, ScriptDef right)
    {
        return !(left == right);
    }
}
