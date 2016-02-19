using Windows.ApplicationModel;
using Autofac;

namespace Audiotica.Windows.Engine
{
    public abstract class AppModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            if (DesignMode.DesignModeEnabled)
                LoadDesignTime(builder);
            else
                LoadRunTime(builder);
        }

        public abstract void LoadDesignTime(ContainerBuilder builder);
        public abstract void LoadRunTime(ContainerBuilder builder);
    }
}