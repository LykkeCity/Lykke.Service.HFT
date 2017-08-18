using System.Collections.Generic;
using System.Linq;
using Lykke.Service.HFT.Middleware;
using Microsoft.AspNetCore.Mvc.Authorization;
using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Generator;

namespace Lykke.Service.HFT.Infrastructure
{
    public class ApiKeyHeaderOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var filterPipeline = context.ApiDescription.ActionDescriptor.FilterDescriptors;
            var isAuthorized = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is AuthorizeFilter);
            var allowAnonymous = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is IAllowAnonymousFilter);
            if (isAuthorized && !allowAnonymous)
            {
                if (operation.Parameters == null)
                    operation.Parameters = new List<IParameter>();
                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = KeyAuthOptions.DefaultHeaderName,
                    In = "header",
                    Description = "access token",
                    Required = true,
                    Type = "string"
                });
            }
        }
    }
}
