using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.HFT.Abstractions
{
    public interface ISample
    {
        string Id { get; }
        string Name { get; }
        string Description { get; }
    }

    public class Sample : ISample
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public static Sample Map(ISample src)
        {
            return Map(src, new Sample());
        }

        public static Sample Map(ISample src, Sample dest)
        {
            dest.Id = src.Id;
            dest.Name = src.Name;
            dest.Description = src.Description;

            return dest;
        }
    }

    public interface ISamplesRepository
    {
        Task InsertAsync(ISample model);
        Task UpdateAsync(ISample model);
        Task<ISample> GetAsync(string id);
        Task<IEnumerable<ISample>> GetAsync();
    }
}