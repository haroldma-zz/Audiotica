using Audiotica.Web.MatchEngine.Interfaces.Validators;
using Audiotica.Web.MatchEngine.Validators;
using Autofac;

namespace Audiotica.AppEngine.Modules
{
    internal class MatchEngineValidatorModule : AppModule
    {
        public override void LoadDesignTime(ContainerBuilder builder)
        {
        }

        public override void LoadRunTime(ContainerBuilder builder)
        {
            builder.RegisterType<FlexibleTypeValidator>().As<ISongTypeValidator>();

            builder.RegisterType<SongAcapellaTypeValidator>().As<ISongTypeValidator>();
            builder.RegisterType<SongAcousticTypeValidator>().As<ISongTypeValidator>();
            builder.RegisterType<SongCoverTypeValidator>().As<ISongTypeValidator>();
            builder.RegisterType<SongLiveTypeValidator>().As<ISongTypeValidator>();
            builder.RegisterType<SongPreviewTypeValidator>().As<ISongTypeValidator>();
            builder.RegisterType<SongRadioTypeValidator>().As<ISongTypeValidator>();
            builder.RegisterType<SongRemixTypeValidator>().As<ISongTypeValidator>();
        }
    }
}