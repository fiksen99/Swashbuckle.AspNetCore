using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Swashbuckle.AspNetCore.MSBuild
{
    public class SwaggerGenerator : Task
    {
        [Required]
        public string AssemblyToLoad { get; set; }

        public override bool Execute()
        {
            throw new System.NotImplementedException();
        }
    }
}
