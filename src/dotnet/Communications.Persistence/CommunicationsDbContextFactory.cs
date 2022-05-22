using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Communications.Persistence
{
    public class CommunicationsDbContextFactory : IDesignTimeDbContextFactory<CommunicationsDbContext>
    {
        private const string ConnectionStringName = "Net6WebApiConnection";
        private const string AspNetCoreEnvironment = "ASPNETCORE_ENVIRONMENT";

        public CommunicationsDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CommunicationsDbContext>();
            optionsBuilder.UseSqlServer(GetConnectionString());

            return new CommunicationsDbContext(optionsBuilder.Options);
        }

        private static string GetConnectionString()
        {
            var basePath = Directory.GetCurrentDirectory();

            var environmentName = Environment.GetEnvironmentVariable(AspNetCoreEnvironment);
            Console.WriteLine(environmentName);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.Local.json", optional: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            Console.WriteLine(configuration.GetConnectionString(ConnectionStringName));
            var connectionString = configuration.GetConnectionString(ConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException($"Connection string '{ConnectionStringName}' is null or empty.", nameof(connectionString));
            }

            return connectionString;
        }
    }
}