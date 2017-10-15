﻿using Autofac;
using Lykke.Service.HFT.Core;
using Lykke.SettingsReader;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Lykke.Service.HFT.Modules
{
    public class MongoDbModule : Module
    {
        private readonly IReloadingManager<AppSettings.MongoSettings> _settings;

        public MongoDbModule(IReloadingManager<AppSettings.MongoSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var mongoUrl = new MongoUrl(_settings.CurrentValue.ConnectionString);
            ConventionRegistry.Register("Ignore extra", new ConventionPack { new IgnoreExtraElementsConvention(true) }, x => true);

            var database = new MongoClient(mongoUrl).GetDatabase(mongoUrl.DatabaseName);
            builder.RegisterInstance(database);
        }
    }
}
