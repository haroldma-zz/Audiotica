using Audiotica.Windows.ViewModels;
using Autofac;

namespace Audiotica.Windows.AppEngine.Modules
{
    internal class ViewModelModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MainPageViewModel>();
        }
    }
}