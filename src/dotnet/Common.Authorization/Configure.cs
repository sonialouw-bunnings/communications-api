using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bunnings.Common.Extensions;
using Bunnings.Common.WebApi.Authorization.IdentityResources;
using Bunnings.Common.WebApi.Authorization.Privileges;
using Bunnings.Common.WebApi.Authorization.Roles;
using Bunnings.Common.WebApi.Authorization.Scopes;
using Bunnings.Common.WebApi.Extensions.Logging;
using Bunnings.Common.WebApi.Filter;
using Bunnings.Common.WebApi.Models;
using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.AspNetCore.OAuth2Introspection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Serilog;


namespace Common.Authorization
{
	public static class Configure
	{
		private const string IntrospectionScheme = "introspection";

		/// <summary>
		/// Configure authentication and authorisation.
		/// </summary>
		/// <param name="services">The service collection to setup authorisation into</param>
		/// <param name="appSettings">The application settings that can be used to get authentication and authorization configuration from</param>
		/// <param name="authorisationProviders">
		/// Assemblies that contain Privileges Scopes and Identity Resources to register
		/// <see cref="Privilege"/> <see cref="Scope"/> <see cref="IdentityResources"/>
		/// and privileges should have contained privilege handlers
		/// <see cref="PrivilegeHandler{TPrivilege}"/> or <see cref="ResourcePrivilegeHandler{TPrivilege,TResources}"/>
		/// </param>
		/// <param name="authorizationOptions">allows additional configuration to the authorization Options beyond the common configuration specific for the calling API. e.g. Adding additional polices not from a privilege.</param>
		/// <param name="authenticationJwtBearOptions">allows additional configuration to the authentication Jwt Bear Options beyond the common configuration specific for the calling API.</param>
		/// <param name="authenticationIntrospectionOptions">allows additional configuration to the authentication Introspection Options beyond the common configuration specific for the calling API.</param>
		public static void ConfigureAuthService(
			this IServiceCollection services,
			IApplicationSettings appSettings,
			IEnumerable<Assembly> authorisationProviders,
			Action<AuthorizationOptions> authorizationOptions = null,
			Action<JwtBearerOptions> authenticationJwtBearOptions = null,
			Action<OAuth2IntrospectionOptions> authenticationIntrospectionOptions = null)
		{
			authorisationProviders ??= new Assembly[0];

			if (string.IsNullOrWhiteSpace(appSettings.RoleMap))
				throw new InvalidOperationException("A path to the role map must be provided.");

			services.AddHttpClient<OpenIdConnectHttpClient>()
				.SetHandlerLifetime(Timeout.InfiniteTimeSpan);
			//.AddIstioDistributedTracePropagation();

			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options =>
				{
					options.Authority = appSettings.Authority;
					options.Audience = appSettings.Audience;
					options.IncludeErrorDetails =
						true; // Includes WWW-Authenticate response header which is super useful when trying to figure out 401 & 403 responses
					options.Events = new JwtBearerEvents
					{
						OnAuthenticationFailed = context =>
						{
							//Log.Logger.Warning(context.Exception, "Authentication Failed");
							return Task.CompletedTask;
						},
						OnForbidden = context =>
						{
							//Log.Logger.Warning("Authorisation of jwt failed");
							return Task.CompletedTask;
						},
						OnChallenge = context =>
						{
							//Log.Logger.Warning(
							//"Authentication Failed {AuthenticationError} with description {AuthenticationDescription}",
							//context.Error, context.ErrorDescription);
							return Task.CompletedTask;
						}
					};
					//options.ForwardDefaultSelector = Selector.ForwardReferenceToken();
					authenticationJwtBearOptions?.Invoke(options);
					options.Validate();
				})
				.AddOAuth2Introspection(IntrospectionScheme, options =>
				{
					options.Authority = appSettings.Authority;
					options.ClientId = appSettings.ApplicationName;
					options.ClientSecret = appSettings.ApiSecret;
					options.EnableCaching = true;
					options.SaveToken = true;

					// as per https://identitymodel.readthedocs.io/en/latest/client/discovery.html the introspection authority needs to match the issuer otherwise errors are thrown
					// the issuer is different from the authority URL as we have a https://bunnings.com.au/ENV issuer to stop clashed between 'authorities' being issued from .mobileapps. and .apps. sub domains
					// so don't validate the endpoint and issuer name as they'll cause errors
					// these validations are pre-flight only before the introspect endpoint is called
					options.DiscoveryPolicy.ValidateEndpoints = false;
					options.DiscoveryPolicy.ValidateIssuerName = false;

					authenticationIntrospectionOptions?.Invoke(options);
					options.Validate();
				});

			services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, PostConfigurationJwtBearerOptions>();
			services.AddSingleton<IOpenIdConnectConfigurationService, OpenIdConnectConfigurationService>();

			//add a hosted service that will perform refreshes of the config from identity server this will run every 10 sec but
			//config by default is only refreshed once per day
			services.AddHostedService<ConfigurationRefreshHostedService>();

			//Register a single instance of the default role service
			//this service is responsible for finding a user role that has the
			//privilege requested.
			services.AddSingleton<IRoleService>(new FileSystemRoleService(appSettings.RoleMap));

			//find all privileges in the given classes
			var privileges = authorisationProviders
				.SelectMany(a => a.GetTypes())
				.Where(t => !t.IsInterface && !t.IsAbstract && typeof(Privilege).IsAssignableFrom(t))
				.ToList();

			//register all default handlers contained in the privileges into the container
			privileges
				.SelectMany(privilege => privilege
					.GetNestedTypes(BindingFlags.NonPublic)
					.Where(t => !t.IsInterface && !t.IsAbstract && typeof(IAuthorizationHandler).IsAssignableFrom(t)))
				.Do(handler => services.AddScoped(typeof(IAuthorizationHandler), handler));

			//configure authorization and add a default policy for each privilege found.
			services.AddAuthorization(c =>
			{
				privileges
					.Select(Activator.CreateInstance)
					.OfType<Privilege>()
					.Do(pri => c.AddPolicy(pri.Name, pri.ConfigureDefaultPrivilegePolicy));

				authorizationOptions?.Invoke(c);
			});

			// Replaces the default messaging handler with the custom code which helps to modify logs as required
			services
				.Where(x => x.ServiceType == typeof(IHttpMessageHandlerBuilderFilter))
				.ToList()
				.Do(service => services.Remove(service));

			services.TryAdd(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, CustomLoggingFilter>());
		}

		public static IHealthChecksBuilder AddIdentityTokenValidationHealthCheck(this IHealthChecksBuilder builder,
			IEnumerable<string> tags = null)
		{
			return builder.AddAsyncCheck("IdentityServer4+TokenValidation", (provider) =>
			{
				var configService = provider.GetRequiredService<IOpenIdConnectConfigurationService>();
				return async token =>
				{
					var isConfigured = await configService.RefreshIfExpiredAsync(token).ConfigureAwait(false);

					return isConfigured ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
				};
			}, tags);
		}

		private class PostConfigurationJwtBearerOptions : IPostConfigureOptions<JwtBearerOptions>
		{
			private readonly IOpenIdConnectConfigurationService _service;

			public PostConfigurationJwtBearerOptions(IOpenIdConnectConfigurationService service)
			{
				_service = service;
			}

			public void PostConfigure(string name, JwtBearerOptions options)
			{
				if (!name.StartsWith(JwtBearerDefaults.AuthenticationScheme))
					return;

				if (!(string.IsNullOrEmpty(options.MetadataAddress) && string.IsNullOrEmpty(options.Authority)))
				{
					if (string.IsNullOrEmpty(options.MetadataAddress) && !string.IsNullOrEmpty(options.Authority))
					{
						options.MetadataAddress = options.Authority;
						if (!options.MetadataAddress.EndsWith("/", StringComparison.Ordinal))
						{
							options.MetadataAddress += "/";
						}

						options.MetadataAddress += ".well-known/openid-configuration";
					}

					if (options.MetadataAddress != null
					    && options.RequireHttpsMetadata
					    && !options.MetadataAddress.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
					{
						throw new InvalidOperationException(
							"The MetadataAddress or Authority must use HTTPS unless disabled for development by setting RequireHttpsMetadata=false.");
					}

					options.ConfigurationManager = _service.GetOrCreateConfigurationManager(options);
				}
				else
				{
					throw new InvalidOperationException("The meta address or the Authority must be specified");
				}
			}
		}

		private class ConfigurationRefreshHostedService : IHostedService
		{
			private readonly IOpenIdConnectConfigurationService _configurationService;
			private readonly CancellationTokenSource _cts = new CancellationTokenSource();
			private Task _executingTask;

			public ConfigurationRefreshHostedService(IOpenIdConnectConfigurationService configurationService)
			{
				_configurationService = configurationService;
			}

			public Task StartAsync(CancellationToken cancellationToken)
			{
				_executingTask = RefreshLoop(_cts.Token);

				return _executingTask.IsCompleted ? _executingTask : Task.CompletedTask;
			}

			public async Task StopAsync(CancellationToken cancellationToken)
			{
				try
				{
					_cts.Cancel();
				}
				finally
				{
					try
					{
						await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
					}
					catch (TaskCanceledException)
					{
					}
				}
			}

			/// <summary>
			/// even though this runs every 10 seconds configuration will only be refreshed by the
			/// configuration manager based on the configuration auto refresh timeout by default this is
			/// set to 1 day.
			/// </summary>
			/// <param name="cancellationToken">token to cancel the refresh loop</param>
			/// <returns>a task</returns>
			public async Task RefreshLoop(CancellationToken cancellationToken)
			{
				try
				{
					while (!cancellationToken.IsCancellationRequested)
					{
						await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken).ConfigureAwait(false);
						await _configurationService.RefreshIfExpiredAsync(cancellationToken).ConfigureAwait(false);
					}
				}
				catch (OperationCanceledException)
				{
				}
			}
		}

		public interface IOpenIdConnectConfigurationService
		{
			/// <summary>
			/// Returns the configuration manager if exists else constructs a new
			/// configuration manager and returns that.
			/// </summary>
			/// <param name="options">The JwtBearerOptions for this configuration manager</param>
			/// <returns>the configuration manager</returns>
			IConfigurationManager<OpenIdConnectConfiguration> GetOrCreateConfigurationManager(JwtBearerOptions options);

			/// <summary>
			/// Refreshes the configuration held in the configuration manager
			/// if it has expired. returns true if any configuration exists.
			/// </summary>
			/// <param name="token">cancellation token for the refresh requests</param>
			/// <returns>true if a configuration exists</returns>
			Task<bool> RefreshIfExpiredAsync(CancellationToken token);
		}

		private class OpenIdConnectConfigurationService : IOpenIdConnectConfigurationService,
			IConfigurationManager<OpenIdConnectConfiguration>
		{
			private readonly IServiceProvider _serviceProvider;
			private IConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
			private volatile OpenIdConnectConfiguration _configuration;

			public OpenIdConnectConfigurationService(IServiceProvider serviceProvider)
			{
				_serviceProvider = serviceProvider;
			}

			public async Task<bool> RefreshIfExpiredAsync(CancellationToken token)
			{
				if (_configurationManager == null)
				{
					//initialise the authentication options this will result in the configuration manager being setup.
					var provider = _serviceProvider.GetRequiredService<IAuthenticationSchemeProvider>();
					var schemas = await provider.GetAllSchemesAsync().ConfigureAwait(false);
					var schema = schemas.First(s => typeof(JwtBearerHandler) == s.HandlerType);

					var optionsMonitor = _serviceProvider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>();
					var _ = optionsMonitor.Get(schema.Name);

					if (_configurationManager == null)
						throw new InvalidOperationException(
							"The configuration manager has not been created have you called ConfigureAuthService");
				}

				_configuration = await _configurationManager.GetConfigurationAsync(token).ConfigureAwait(false);
				return _configuration != null;
			}

			public IConfigurationManager<OpenIdConnectConfiguration> GetOrCreateConfigurationManager(JwtBearerOptions options)
			{
				_configurationManager ??= new ConfigurationManager<OpenIdConnectConfiguration>(options.MetadataAddress,
					new OpenIdConnectConfigurationRetriever(),
					new DocumentRetriever(_serviceProvider, options.BackchannelTimeout, options.RequireHttpsMetadata));
				return this;
			}

			/// <summary>
			/// This implementation is intended to be used after a call has been made to
			/// RefreshIfExpiredAsync as it will then use the returned configuration to return synchronously.
			/// once initialised RefreshIfExpiredAsync must be used to keep the configuration refreshed as this
			/// method will never refresh the current configuration. a good method to do this by is to use the
			/// health check
			/// </summary>
			/// <param name="cancel">cancellation token</param>
			/// <returns>the OpenIdConnectConfiguration</returns>
			public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(CancellationToken cancel)
			{
				// ReSharper disable once NonAtomicCompoundOperator the implementation of GetConfigurationAsync is thread safe so if we end up calling twice it's all ok.
				_configuration ??= await _configurationManager.GetConfigurationAsync(cancel).ConfigureAwait(false);

				return _configuration;
			}

			public void RequestRefresh()
			{
				_configurationManager.RequestRefresh();
			}
		}


		private class OpenIdConnectHttpClient
		{
			public OpenIdConnectHttpClient(HttpClient client)
			{
				Client = client;
			}

			public HttpClient Client { get; }
		}

		private class DocumentRetriever : IDocumentRetriever
		{
			private readonly IServiceProvider _serviceProvider;
			private readonly TimeSpan _timeout;
			private readonly bool _requireHttpsMetadata;

			public DocumentRetriever(IServiceProvider serviceProvider, TimeSpan timeout, bool requireHttpsMetadata)
			{
				_serviceProvider = serviceProvider;
				_timeout = timeout;
				_requireHttpsMetadata = requireHttpsMetadata;
			}

			public Task<string> GetDocumentAsync(string address, CancellationToken cancel)
			{
				var client = _serviceProvider.GetRequiredService<OpenIdConnectHttpClient>().Client;
				client.Timeout = _timeout;
				client.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10 MB
				var documentRetriever = new HttpDocumentRetriever(client) { RequireHttps = _requireHttpsMetadata };
				return documentRetriever.GetDocumentAsync(address, cancel);
			}
		}
	}
}