using System.Text;

namespace Tenray.TopazView;

public sealed class FileSystemContentProvider : IContentProvider
{
    readonly string Root;

    public Encoding Encoding { get; }

    public FileSystemContentProvider(string root, Encoding encoding = null)
    {
        Root = Path.GetFullPath(root);
        Encoding = encoding ?? Encoding.UTF8;
    }

    public string GetContent(string path)
    {
        var root = Root;
        if (path.StartsWith('/'))
            path = path.Substring(1);
        path = Path.Combine(root, path);
        if (!path.StartsWith(root, StringComparison.Ordinal))
            throw new ArgumentException($"{path} is not a subpath of root.", nameof(path));
        return File.ReadAllText(path, Encoding);
    }

    public ValueTask<string> GetContentAsync(string path)
    {
        return ValueTask.FromResult(GetContent(path));
    }
}
