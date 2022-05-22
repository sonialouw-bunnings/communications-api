using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Communications.Application.Common.Interfaces;

namespace Communications.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHealthChecks()
                .AddDbContextCheck<CommunicationsDbContext>(name: "Application Database");

            services.AddDbContext<CommunicationsDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("Net6WebApiConnection"),
                b => b.MigrationsAssembly(typeof(CommunicationsDbContext).Assembly.FullName))
                .LogTo(Console.WriteLine, LogLevel.Information)); // disable for production;

            services.AddScoped<ICommunicationsDbContext>(provider =>
                provider.GetService<CommunicationsDbContext>());

            return services;
        }
    }
}