﻿using Autofac;
using Lykke.Service.HFT.Core;
using Lykke.SettingsReader;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Lykke.Service.HFT.Modules
{
    public class MongoDbModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public MongoDbModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(x =>
                {
                    ConventionRegistry.Register("Ignore extra", new ConventionPack { new IgnoreExtraElementsConvention(true) }, _ => true);

                    var mongoUrl = new MongoUrl(_settings.CurrentValue.HighFrequencyTradingService.MongoSettings.ConnectionString);
                    MongoDefaults.GuidRepresentation = GuidRepresentation.Standard;
                    return new MongoClient(mongoUrl).GetDatabase(mongoUrl.DatabaseName);
                })
                .As<IMongoDatabase>()
                .SingleInstance();
        }
    }
}
