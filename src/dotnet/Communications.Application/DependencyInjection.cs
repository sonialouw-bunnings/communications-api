using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Communications.Application.Common.Behaviours;
using System.Reflection;

namespace Communications.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
	        // Register Fluent Validation service
	        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // Register MediatR Services
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));

            return services;
        }
    }
}