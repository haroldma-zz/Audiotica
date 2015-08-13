using System;
using Autofac;
using Autofac.Builder;

namespace Audiotica.Windows.AppEngine.Providers
{
    public static class ProviderHelper
    {
        public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> RegisterType<T, TProvider>(this ContainerBuilder builder) where  TProvider : IProvider<T>, new() 
        {
            var provider = Activator.CreateInstance<TProvider>();
            return builder.Register(provider.CreateInstance);
        }
    }
}