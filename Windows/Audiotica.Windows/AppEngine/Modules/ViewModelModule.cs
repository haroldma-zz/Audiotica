using System.Reflection;
using Audiotica.Core.Extensions;
using Audiotica.Windows.Tools.Mvvm;
using Autofac;
using Module = Autofac.Module;

namespace Audiotica.Windows.AppEngine.Modules
{
    internal class ViewModelModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Every view model is a child of this base
            var viewModelBase = typeof (ViewModelBase);

            // they should also be located in that assembly (Audiotica.Windows)
            var assembly = viewModelBase.GetTypeInfo().Assembly;

            var types = assembly.DefinedTypes.GetImplementations(viewModelBase);
            foreach (var type in types)
            {
                builder.RegisterType(type);
            }
        }
    }
}