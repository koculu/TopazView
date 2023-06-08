namespace Tenray.TopazView;

public interface IContentProvider
{
    ValueTask<string> GetContentAsync(string path);

    string GetContent(string path);
}
