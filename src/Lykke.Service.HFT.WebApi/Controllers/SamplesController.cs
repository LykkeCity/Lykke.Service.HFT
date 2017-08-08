using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Lykke.Service.HFT.Abstractions;
using Lykke.Service.HFT.WebApi.Helpers;

namespace Lykke.Service.HFT.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class SamplesController : Controller
    {
        private readonly ISamplesRepository _samplesRepository;

        public SamplesController(ISamplesRepository samplesRepository)
        {
            _samplesRepository = samplesRepository;
        }

        [HttpPost]
        public Task<IActionResult> InsertAsync([FromBody] Sample model)
        {
            return _samplesRepository
                .InsertAsync(model)
                .ToActionResult();
        }
        
        [HttpPut("{id}")]
        public Task<IActionResult> UpdateAsync(string id, [FromBody] Sample model)
        {
            if (!id.Equals(model.Id))
                return Task.FromException<IActionResult>(
                    new Exception("Invalid data: Wrong Id value."))
                    .ToActionResult();

            return _samplesRepository
                .UpdateAsync(model)
                .ToActionResult();
        }

        [HttpGet("{id}")]
        public Task<IActionResult> GetAsync(string id)
        {
            return _samplesRepository
                .GetAsync(id)
                .ToActionResult();
        }

        [HttpGet]
        public Task<IActionResult> GetAsync()
        {
            return _samplesRepository
                .GetAsync()
                .ToActionResult();
        }
    }
}