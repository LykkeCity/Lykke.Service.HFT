using System.Collections.Generic;
using System.Linq;
using Lykke.Service.HFT.Middleware;
using Microsoft.AspNetCore.Mvc.Authorization;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.HFT.Infrastructure
{
    internal class AuthorizationHeaderOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var filterPipeline = context.ApiDescription.ActionDescriptor.FilterDescriptors;
            var isAuthorized = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is AuthorizeFilter authorizeFilter && authorizeFilter.Policy.AuthenticationSchemes.Any(x => x == HmacAuthOptions.AuthenticationScheme));
            var allowAnonymous = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is IAllowAnonymousFilter);
             if (isAuthorized && !allowAnonymous)
            {
                if (operation.Parameters == null)
                    operation.Parameters = new List<IParameter>();
                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = HmacAuthOptions.DefaultHeaderName,
                    In = "header",
                    Description = "HMAC apiKey:timestamp:signature",
                    Required = true,
                    Type = "string"
                });

                operation.Responses.Add("401", new Response
                {
                    Description = "Not authorized or invalid authorization header"
                });
            }
        }
    }
}
