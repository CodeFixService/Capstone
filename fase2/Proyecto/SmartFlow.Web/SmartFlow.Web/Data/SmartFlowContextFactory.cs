using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace SmartFlow.Web.Data
{
    public class SmartFlowContextFactory : IDesignTimeDbContextFactory<SmartFlowContext>
    {
        public SmartFlowContext CreateDbContext(string[] args)
        {
            // subir dos niveles desde bin\debug\net8.0
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<SmartFlowContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("SmartFlowConnection"));

            return new SmartFlowContext(optionsBuilder.Options);
        }
    }
}
