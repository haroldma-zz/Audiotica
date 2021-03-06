﻿using System.Reflection;
using Audiotica.Core.Extensions;
using Audiotica.Web.MatchEngine.Interfaces.Validators;
using Autofac;

namespace Audiotica.Windows.Engine.Modules
{
    internal class MatchEngineValidatorModule : AppModule
    {
        public override void LoadDesignTime(ContainerBuilder builder)
        {
        }

        public override void LoadRunTime(ContainerBuilder builder)
        {
            // Every validator most implement this interface
            var validatorInterface = typeof (ISongTypeValidator);

            // they should also be located in that assembly (Audiotica.Web)
            var assembly = validatorInterface.GetTypeInfo().Assembly;

            var types = assembly.ExportedTypes.GetImplementations(validatorInterface);
            foreach (var type in types)
            {
                builder.RegisterType(type).As(validatorInterface);
            }
        }
    }
}