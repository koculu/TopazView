namespace Tenray.TopazView.Impl;

internal interface IViewRepository
{
    IView GetOrCreateView(string path, ViewFlags viewFlags);

    /// <summary>
    /// </summary>
    /// <param name="key">key = path + (int)ViewFlags</param>
    /// <param name="delayDispose"></param>
    void DropViewByKey(string key, TimeSpan delayDispose);

    void DropViewByPath(string path, ViewFlags viewFlags, TimeSpan delayDispose);

    void DropAll(TimeSpan delayDispose);
}
