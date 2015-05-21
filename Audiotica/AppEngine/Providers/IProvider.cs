using Autofac;

namespace Audiotica.AppEngine.Providers
{
    public interface IProvider<out T>
    {
        T CreateInstance(IComponentContext context);
    }
}