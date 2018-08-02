using System.Net;
using Lykke.Service.HFT.Contracts;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.HFT.Swagger
{
    internal class DefaultFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var item in swaggerDoc.Paths.Values)
            {
                UpdateItem(item, HttpStatusCode.BadRequest, "Bad or malformed request.", true);
                UpdateItem(item, HttpStatusCode.InternalServerError, "Internal server error.");
                UpdateItem(item, HttpStatusCode.ServiceUnavailable, "Service in maintenance mode.");
                UpdateItem(item, HttpStatusCode.TooManyRequests, "Request limit reached, maximum of 300 requests per minute.");
            }
        }

        private static void UpdateItem(PathItem item, HttpStatusCode code, string description, bool errorModel = false)
        {
            TrySetValue(item.Delete, code, description, errorModel);
            TrySetValue(item.Post, code, description, errorModel);
            TrySetValue(item.Get, code, description, errorModel);
            TrySetValue(item.Patch, code, description, errorModel);
            TrySetValue(item.Put, code, description, errorModel);
        }

        private static void TrySetValue(Operation operation, HttpStatusCode code, string description, bool errorModel)
        {
            if (operation == null)
            {
                return;
            }

            var key = ((int)code).ToString();

            if (operation.Responses.ContainsKey(key))
            {
                return;
            }

            Schema GetSchema()
            {
                if (!errorModel)
                {
                    return null;
                }

                return new Schema
                {
                    Ref = $"#/definitions/{nameof(ResponseModel)}"
                };
            }

            operation.Responses.Add(key, new Response
            {
                Description = description,
                Schema = GetSchema()
            });
        }
    }
}