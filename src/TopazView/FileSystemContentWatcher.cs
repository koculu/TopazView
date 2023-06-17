using System.Security;

namespace Tenray.TopazView;

public sealed class FileSystemContentWatcher : IDisposable
{
    IViewEngine ViewEngine { get; set; }

    FileSystemWatcher Watcher { get; set; }

    string RootPath { get; set; }

    public event ViewCacheInvalidated OnViewCacheInvalidated;

    public delegate void ViewCacheInvalidated();

    bool TrackViewsUsingPrivateJavascriptEngine { get; }

    /// <summary>
    /// Immediate dispose might hurt ongoing requests.
    /// </summary>
    TimeSpan DelayViewDispose { get; }

    public FileSystemContentWatcher()
    {
        DelayViewDispose = TimeSpan.FromSeconds(2);
    }

    public FileSystemContentWatcher(TimeSpan delayViewDispose, bool trackViewsUsingPrivateJavascriptEngine = false)
    {
        DelayViewDispose = delayViewDispose;
        TrackViewsUsingPrivateJavascriptEngine = trackViewsUsingPrivateJavascriptEngine;
    }

    public FileSystemContentWatcher(bool trackViewsUsingPrivateJavascriptEngine)
    {
        DelayViewDispose = TimeSpan.FromSeconds(2);
        TrackViewsUsingPrivateJavascriptEngine = trackViewsUsingPrivateJavascriptEngine;
    }

    public void StartWatcher(string path, IViewEngine viewEngine)
    {
        if (ViewEngine != null)
            return;
        ViewEngine = viewEngine;
        RootPath = path;
        void startWatcher()
        {
            var watcher = new FileSystemWatcher(Path.GetFullPath(path))
            {
                NotifyFilter =
                    NotifyFilters.CreationTime |
                    NotifyFilters.LastWrite |
                    NotifyFilters.FileName |
                    NotifyFilters.DirectoryName,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };
            watcher.Error += OnError;
            watcher.Changed += OnChanged;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;
            Watcher = watcher;
        }

        Task.Run(startWatcher).Wait();
    }

    void OnError(object sender, ErrorEventArgs e)
    {
        Task.Run(() => EnsureWatcherIsRunning());
    }

    void EnsureWatcherIsRunning()
    {
        var watcher = Watcher;
        watcher.EnableRaisingEvents = false;
        while (watcher.EnableRaisingEvents == false)
        {
            if (!Directory.Exists(watcher.Path))
            {
                Console.WriteLine($"Content Watcher: Directory does not exist: {watcher.Path}");
                Thread.Sleep(2000);
                continue;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                watcher.EnableRaisingEvents = false;
                Thread.Sleep(2000);
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }
    }

    void DropView(string fullPath)
    {
        var path = GetRelativePath(fullPath);
        if (TrackViewsUsingPrivateJavascriptEngine)
        {
            ViewEngine.DropViewByPath(
                path,
                ViewFlags.PrivateJavascriptEngine,
                DelayViewDispose);
        }
        ViewEngine.DropViewByPath(
            path,
            ViewFlags.None,
            DelayViewDispose);
    }

    void RaiseViewCacheInvalidated()
    {
        OnViewCacheInvalidated?.Invoke();
    }

    string GetRelativePath(string fullPath)
    {
        return GetRelativePath(fullPath, RootPath);
    }

    static string GetRelativePath(string filespec, string folder)
    {
        folder = Path.GetFullPath(folder);
        var pathUri = new Uri(filespec);
        if (!folder.EndsWith(Path.DirectorySeparatorChar))
        {
            folder += Path.DirectorySeparatorChar;
        }
        var folderUri = new Uri(folder);
        return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString());
    }

    void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Changed)
            return;
        DropView(e.FullPath);
        RaiseViewCacheInvalidated();
    }

    void OnDeleted(object sender, FileSystemEventArgs e)
    {
        DropView(e.FullPath);
        RaiseViewCacheInvalidated();
    }

    void OnRenamed(object sender, RenamedEventArgs e)
    {
        DropView(e.OldFullPath);
        DropView(e.FullPath);
        DropIfDirectoryRenamed(e.OldFullPath, e.FullPath);
        RaiseViewCacheInvalidated();
    }

    void DropIfDirectoryRenamed(string oldFullPath, string fullPath)
    {
        if (!Directory.Exists(fullPath))
            return;
        try
        {
            foreach (
                string newLocation in
                Directory.EnumerateFileSystemEntries(fullPath, "*", SearchOption.AllDirectories))
            {
                var oldLocation = Path.Combine(
                    oldFullPath,
                    newLocation.Substring(fullPath.Length + 1));
                DropView(oldLocation);
            }
        }
        catch (Exception e) when (
            e is IOException ||
            e is SecurityException ||
            e is DirectoryNotFoundException ||
            e is UnauthorizedAccessException)
        {
            // Swallow the exception.
        }
    }

    public void Dispose()
    {
        Watcher.Changed -= OnChanged;
        Watcher.Deleted -= OnDeleted;
        Watcher.Renamed -= OnRenamed;
        Watcher?.Dispose();
    }
}