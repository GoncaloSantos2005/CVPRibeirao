using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SistemaPDI.Infrastructure.Data
{
    /// <summary>
    /// Factory para criar o DbContext em tempo de design (migrations).
    /// </summary>
    public class PdiDbContextFactory : IDesignTimeDbContextFactory<PdiDbContext>
    {
        public PdiDbContext CreateDbContext(string[] args)
        {
            // Caminho para o appsettings.json do projeto API
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "SistemaPDI.API");
            
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<PdiDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new PdiDbContext(optionsBuilder.Options);
        }
    }
}