using System;
using System.Threading.Tasks;
using Lykke.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.HFT.Helpers
{
    public static class TaskExtender
    {
        public static async Task<IActionResult> ToActionResult(this Task task)
        {
            try
            {
                await task;
                return new NoContentResult();
            }
            catch (Exception e)
            {
                return GetExeptionResult(e);
            }
        }

        public static async Task<IActionResult> ToActionResult<T>(this Task<T> task)
        {
            try
            {
                var result = await task;
                if (result == null)
                    return new NotFoundResult();

                return new OkObjectResult(result);
            }
            catch (Exception e)
            {
                return GetExeptionResult(e);
            }
        }

        private static IActionResult GetExeptionResult(Exception e)
        {
            return new BadRequestObjectResult(RestException.Map(e));
        }
    }
}