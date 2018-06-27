using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Reflection;

namespace Swashbuckle.AspNetCore.MSBuild
{
    public class SwaggerGenerator : Task
    {
        [Required]
        public string AssemblyToLoad { get; set; }

        [Required]
        public string SwaggerDoc { get; set; }

        public string Host { get; set; }

        public string BasePath { get; set; }

        public override bool Execute()
        {
            // 1) Configure host with provided startupassembly
            var startupAssembly = Assembly.LoadFrom(Path.Combine(Directory.GetCurrentDirectory(), AssemblyToLoad));
            var builder = WebHost.CreateDefaultBuilder();
            builder.UseStartup(startupAssembly.FullName);
            var host = builder.Build();

            // 2) Retrieve Swagger via configured provider
            var swaggerProvider = host.Services.GetRequiredService<Swagger.ISwaggerProvider>();
            var swagger = swaggerProvider.GetSwagger(
                SwaggerDoc,
                Host,
                BasePath,
                null);

            // 3) Serialize to specified output location or stdout
            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "swagger.json");

            using (var streamWriter = (outputPath != null ? File.CreateText(outputPath) : Console.Out))
            {
                var mvcOptionsAccessor = (IOptions<MvcJsonOptions>)host.Services.GetService(typeof(IOptions<MvcJsonOptions>));
                var serializer = new JsonSerializer
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented,
                    ContractResolver = new SwaggerContractResolver(new JsonSerializerSettings())
                };
                ////var serializer = SwaggerSerializerFactory.Create(mvcOptionsAccessor);

                serializer.Serialize(streamWriter, swagger);
                Console.WriteLine($"Swagger JSON succesfully written to {outputPath}");
            }

            return true;
        }
    }
    public class SwaggerContractResolver : DefaultContractResolver
    {
        private readonly JsonConverter _applicationTypeConverter;

        public SwaggerContractResolver(JsonSerializerSettings applicationSerializerSettings)
        {
            NamingStrategy = new CamelCaseNamingStrategy { ProcessDictionaryKeys = false };
            _applicationTypeConverter = new ApplicationTypeConverter(applicationSerializerSettings);
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var jsonProperty = base.CreateProperty(member, memberSerialization);

            if (member.Name == "Example" || member.Name == "Examples" || member.Name == "Default")
                jsonProperty.Converter = _applicationTypeConverter;

            return jsonProperty;
        }

        private class ApplicationTypeConverter : JsonConverter
        {
            private JsonSerializer _applicationTypeSerializer;

            public ApplicationTypeConverter(JsonSerializerSettings applicationSerializerSettings)
            {
                _applicationTypeSerializer = JsonSerializer.Create(applicationSerializerSettings);
            }

            public override bool CanConvert(Type objectType) { return true; }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                _applicationTypeSerializer.Serialize(writer, value);
            }
        }
    }
}
