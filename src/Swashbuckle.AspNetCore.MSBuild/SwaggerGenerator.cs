using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

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
            throw new System.NotImplementedException();
        }
    }
}
