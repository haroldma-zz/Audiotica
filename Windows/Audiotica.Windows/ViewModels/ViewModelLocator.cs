using Audiotica.Windows.AppEngine;
using Audiotica.Windows.Factories;

namespace Audiotica.Windows.ViewModels
{
    internal class ViewModelLocator
    {
        private AppKernel _kernel => App.Current?.Kernel ?? AppKernelFactory.Create();

        public MainPageViewModel MainPage => _kernel.Resolve<MainPageViewModel>();
    }
}