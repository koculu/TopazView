namespace Tenray.TopazView.Extensions;

internal static class PathStringConversions
{
    public static string PathToUnix(this string value)
    {
        return value.Replace(@"\", "/", StringComparison.Ordinal);
    }

    /// <summary>
    /// Appends paths to this path string,
    /// eg: 
    /// /root/example + ../test/test.view
    ///     =>
    /// /root/test/test.view
    /// </summary>
    /// <param name="basePath">The base path.</param>
    /// <param name="paths">Paths to be appended.</param>
    /// <returns>Absolute path starting with /.</returns>
    public static string JoinPath(this string basePath, params string[] paths)
    {
        var lastPath = paths.LastOrDefault();
        if (lastPath.StartsWith('/'))
            return lastPath;
        var start = basePath.ToAbsolutePath();
        return Path
            .Combine(new[] { start }.Concat(paths).ToArray())
            .ToAbsolutePath()
            .PathToUnix();
    }

    public static string ToAbsolutePath(this string path)
    {
        if (!path.StartsWith('/'))
            path = "/" + path;
        var rootLength = Path.GetFullPath("/").Length;
        return '/' + Path.GetFullPath(path)[rootLength..];
    }
}
