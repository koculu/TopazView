namespace Tenray.TopazView.DI;

internal sealed class Container
{
    public TransientObjectContainer TransientObjectContainer { get; } = new();

    public UserFactories UserFactories => TransientObjectContainer.UserFactories;

    public UserInstances UserInstances => TransientObjectContainer.UserInstances;
}