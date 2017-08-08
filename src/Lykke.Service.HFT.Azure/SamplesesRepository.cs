using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Microsoft.WindowsAzure.Storage.Table;
using Lykke.Service.HFT.Abstractions;

namespace Lykke.Service.HFT.Azure
{
    public class SampleEntity : TableEntity, ISample
    {
        public const string Partition = "Sample";

        public SampleEntity()
        {
            PartitionKey = Partition;
        }

        public SampleEntity(string id)
            : this()
        {
            RowKey = id;
        }

        public string Id => RowKey;
        public string Name { get; set; }
        public string Description { get; set; }

        public static SampleEntity Map(ISample src)
        {
            return Map(src, new SampleEntity(src.Id));
        }

        public static SampleEntity Map(ISample src, SampleEntity dest)
        {
            dest.Name = src.Name;
            dest.Description = src.Description;

            return dest;
        }
    }

    public class SamplesRepository : ISamplesRepository
    {
        private readonly INoSQLTableStorage<SampleEntity> _tableStorage;

        public SamplesRepository(INoSQLTableStorage<SampleEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task InsertAsync(ISample model)
        {
            var entity = SampleEntity.Map(model);
            return _tableStorage.InsertAsync(entity);
        }

        public Task UpdateAsync(ISample model)
        {
            return _tableStorage.MergeAsync(SampleEntity.Partition, model.Id,
                entity => SampleEntity.Map(model, entity));
        }

        public async Task<ISample> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            var entity = await _tableStorage
                .GetDataAsync(SampleEntity.Partition, id);

            return entity != null
                ? Sample.Map(entity)
                : null;
        }

        public async Task<IEnumerable<ISample>> GetAsync()
        {
            var entities = await _tableStorage
                .GetDataAsync(SampleEntity.Partition);

            return entities.Select(Sample.Map);
        }
    }
}