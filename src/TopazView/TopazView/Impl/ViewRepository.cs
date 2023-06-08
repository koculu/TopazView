using System.Collections.Concurrent;

namespace Tenray.TopazView.Impl;

internal sealed class ViewRepository : IViewRepository, IViewEngineComponentsProvider
{
    ConcurrentDictionary<string, IView> Views { get; } = new();

    public IServiceProvider ServiceProvider { get; }

    public IViewEngineComponents ViewEngineComponents { get; set; }

    public ViewRepository(
        IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public void DropViewByKey(string key, TimeSpan delayDispose)
    {
        if (Views
            .TryRemove(
                key,
                out var view))
            view.DisposeView(delayDispose).AsTask();
    }

    public void DropViewByPath(string path, ViewFlags viewFlags, TimeSpan delayDispose)
    {
        DropViewByKey(path + (int)viewFlags, delayDispose);
    }

    public IView GetOrCreateView(string path, ViewFlags viewFlags)
    {
        var key = path + (int)viewFlags;
        if (Views.TryGetValue(key, out var view))
            return view;
        var newView = new View(path, viewFlags,
            ViewEngineComponents, ServiceProvider);
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
