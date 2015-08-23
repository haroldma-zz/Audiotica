﻿using Audiotica.Core.Windows.Services;
using Audiotica.Database.Services.DesignTime;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Database.Services.RunTime;
using Audiotica.Web.MatchEngine.Interfaces.Providers;
using Audiotica.Web.MatchEngine.Services;
using Audiotica.Windows.Services.DesignTime;
using Audiotica.Windows.Services.Interfaces;
using Audiotica.Windows.Services.NavigationService;
using Audiotica.Windows.Services.RunTime;
using Autofac;

namespace Audiotica.Windows.AppEngine.Modules
{
    internal class ServiceModule : AppModule
    {
        public override void LoadDesignTime(ContainerBuilder builder)
        {
            builder.RegisterType<DesignInsightsService>().As<IInsightsService>();
            builder.RegisterType<DesignPlayerService>().As<IPlayerService>();
            builder.RegisterType<DesignNavigationService>().As<INavigationService>();
            builder.RegisterType<DesignLibraryService>().As<ILibraryService>();
            builder.RegisterType<DesignMatchEngineService>().As<IMatchEngineService>();
        }

        public override void LoadRunTime(ContainerBuilder builder)
        {
            builder.RegisterType<InsightsService>().As<IInsightsService>();
            builder.RegisterType<PlayerService>().As<IPlayerService>().SingleInstance();
            builder.RegisterType<NavigationService>().As<INavigationService>().SingleInstance();
            builder.RegisterType<LibraryService>().As<ILibraryService>().SingleInstance();
            builder.RegisterType<MatchEngineService>().As<IMatchEngineService>();
            builder.RegisterType<TrackSaveService>().As<ITrackSaveService>();
            builder.RegisterType<LibraryMatchingService>().As<ILibraryMatchingService>().SingleInstance();
        }
    }
}