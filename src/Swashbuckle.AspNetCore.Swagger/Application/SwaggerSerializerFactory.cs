﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Swashbuckle.AspNetCore.Swagger
{
    public class SwaggerSerializerFactory
    {
        public static JsonSerializer Create(IOptions<MvcJsonOptions> applicationJsonOptions)
        {
            // TODO: Should this handle case where mvcJsonOptions.Value == null?
            return new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
                ContractResolver = new SwaggerContractResolver(applicationJsonOptions.Value.SerializerSettings)
            };
        }
    }
}
