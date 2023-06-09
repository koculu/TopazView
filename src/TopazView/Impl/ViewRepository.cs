using System.Collections.Concurrent;
using System.IO;
using Tenray.TopazView.DI;

namespace Tenray.TopazView.Impl;

internal sealed class ViewRepository : IViewRepository, IViewEngineComponentsProvider
{
    ConcurrentDictionary<string, IView> Views { get; } = new();

    public IJavascriptEngineProvider JavascriptEngineProvider { get; }

    public IViewEngineComponents ViewEngineComponents { get; set; }

    public ViewRepository(
        IJavascriptEngineProvider javascriptEngineProvider)
    {
        JavascriptEngineProvider = javascriptEngineProvider;
    }

    public void DropViewByKey(string key, TimeSpan delayDispose)
    {
        if (Views
            .TryRemove(
                NormalizePath(key),
                out var view))
            view.DisposeView(delayDispose).AsTask();
    }

    static string NormalizePath(string path)
    {
        if (path.StartsWith('/'))
            return path[1..];
        return path;
    }

    public void DropViewByPath(string path, ViewFlags viewFlags, TimeSpan delayDispose)
    {
        DropViewByKey(NormalizePath(path) + (int)viewFlags, delayDispose);
    }

    public IView GetOrCreateView(string path, ViewFlags viewFlags)
    {
        var key = NormalizePath(path) + (int)viewFlags;
        if (Views.TryGetValue(key, out var view))
            return view;
        var newView = new View(path, viewFlags,
            ViewEngineComponents, JavascriptEngineProvider);
        if (Views.TryAdd(key, newView))
            return newView;

        if (Views.TryGetValue(key, out view))
        {
            newView.DisposeView(TimeSpan.Zero).AsTask();
            return view;
        }

        // There was a drop view operation between TryAdd and TryGet. path: {path}

        if (Views.TryAdd(key, newView))
            return newView;

        if (Views.TryGetValue(key, out view))
        {
            newView.DisposeView(TimeSpan.Zero).AsTask();
            return view;
        }

        // There was a second drop view operation between TryAdd and TryGet. path: {path}

        // bad chance:
        // there was a drop view operation between TryAdd and TryGet
        // we don't add another iteration and
        // return new view as a transient compilable object.
        // with no harm. (1 in a billion edge case)
        return newView;
    }

    public void DropAll(TimeSpan delayDispose)
    {
        var keys = Views.Keys.ToArray();
        foreach (var key in keys)
            DropViewByKey(key, delayDispose);
    }
}
