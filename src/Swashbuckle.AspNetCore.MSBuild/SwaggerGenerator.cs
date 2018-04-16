using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
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
            var host = WebHost.CreateDefaultBuilder()
                .UseStartup(startupAssembly.FullName)
                .Build();

            // 2) Retrieve Swagger via configured provider
            var swaggerProvider = host.Services.GetRequiredService<ISwaggerProvider>();
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
                var serializer = SwaggerSerializerFactory.Create(mvcOptionsAccessor);

                serializer.Serialize(streamWriter, swagger);
                Console.WriteLine($"Swagger JSON succesfully written to {outputPath}");
            }

            return true;
        }
    }
}
