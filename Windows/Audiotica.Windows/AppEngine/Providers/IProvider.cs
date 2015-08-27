using Autofac;

namespace Audiotica.Windows.AppEngine.Providers
{
    public interface IProvider<out T>
    {
        T CreateInstance(IComponentContext context);
    }
}