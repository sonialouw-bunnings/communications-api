﻿using FluentEmail.MailKitSmtp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Communications.Application.Common.Interfaces;
using Communications.Infrastructure.ApiClients.GitHub;
using Communications.Infrastructure.Cache.InMemory;
using Communications.Infrastructure.DataProtection;
using Communications.Infrastructure.Identity;
using Communications.Infrastructure.Notifications.Email;
using Communications.Infrastructure.Oauth;
using Polly;
using System.Text;

namespace Communications.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastrucutre(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            // Register OAuth services
            services.AddTransient<IJwtTokenManager, JwtTokenManager>();
            services.AddScoped<ISignInManager, SignInManager>();

            // Configure JWT Authentication and Authorization
            var jwtSettings = new JwtSettings();
            configuration.Bind(nameof(JwtSettings), jwtSettings);
            services.AddSingleton(jwtSettings);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
                ValidateIssuer = jwtSettings.ValidateIssuer,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = jwtSettings.ValidateAudience,
                ValidAudience = jwtSettings.Audience,
                RequireExpirationTime = jwtSettings.RequireExpirationTime,
                ValidateLifetime = jwtSettings.ValidateLifetime,
                ClockSkew = jwtSettings.Expiration
            };
            services.AddSingleton(tokenValidationParameters);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = tokenValidationParameters;
            });

            // Register Identity DbContext and Server
            services.AddDbContext<ApplicationIdentityDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("Net6WebApiConnection")));

            var identityOptionsConfig = new IdentityOptions();
            configuration.GetSection(nameof(IdentityOptions)).Bind(identityOptionsConfig);

            services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = identityOptionsConfig.RequiredLength;
                options.Password.RequireDigit = identityOptionsConfig.RequiredDigit;
                options.Password.RequireLowercase = identityOptionsConfig.RequireLowercase;
                options.Password.RequiredUniqueChars = identityOptionsConfig.RequiredUniqueChars;
                options.Password.RequireUppercase = identityOptionsConfig.RequireUppercase;
                options.Lockout.MaxFailedAccessAttempts = identityOptionsConfig.MaxFailedAttempts;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(identityOptionsConfig.LockoutTimeSpanInDays);
            })
            .AddEntityFrameworkStores<ApplicationIdentityDbContext>();

            // Register Data Protection Services
            services.AddDataProtection()
                .SetDefaultKeyLifetime(TimeSpan.FromDays(30));
            services.AddSingleton<IDataEncryption, RouteDataProtection>();

            // Register InMemory Cache services
            services.AddMemoryCache();
            services.AddSingleton<ICacheProvider, CacheProvider>();

            // Register Fluent Email Services
            var emailConfig = new EmailConfiguration();
            configuration.GetSection(nameof(EmailConfiguration)).Bind(emailConfig);

            services.AddFluentEmail(defaultFromEmail: emailConfig.Email)
                .AddRazorRenderer()
                .AddMailKitSender(new SmtpClientOptions()
                {
                    Server = emailConfig.Host,
                    Port = emailConfig.Port,
                    //User = emailConfig.Email,
                    //Password = emailConfig.Password,
                    //RequiresAuthentication = true,
                    PreferredEncoding = "utf-8",
                    UsePickupDirectory = true,
                    MailPickupDirectory = @"C:\Users\mgayle\email",
                    UseSsl = emailConfig.EnableSsl
                });

            // Register Email Notification Service
            services.AddScoped<IEmailNotification, EmailNotificationService>();


            // Register Names HTTP Client
            services.AddHttpClient(name: "GitHub", client =>
            {
                client.BaseAddress = new Uri("https://api.github.com/");
                client.DefaultRequestHeaders.Add(name: "Accept", value: "application/vnd.github.v3+json");
                client.DefaultRequestHeaders.Add(name: "User-Agent", value: "HttpClientFactoryExample");
            })
                .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(300)));

            // Register GitHubApiService
            services.AddScoped<IGitHubService, GitHubApiService>();

            return services;
        }
    }
}