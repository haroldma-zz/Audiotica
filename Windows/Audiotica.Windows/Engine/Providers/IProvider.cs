using Autofac;

namespace Audiotica.Windows.Engine.Providers
{
    public interface IProvider<out T>
    {
        T CreateInstance(IComponentContext context);
    }
}