using System;
using System.Collections.Generic;
using System.Globalization;
using its.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace its
{
    public class Startup
    {
        private const string DefaultCulture = "en-US";
        private const string CacheInstanceInternal = "its.internal";
        private const string DataProtectionKeyKey = "dpk";

        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            _config = configuration;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // set a default culture of en-US if none is specified
            string culture = _config[Keys.Configuration.Culture] ?? DefaultCulture;
            _logger.LogInformation("Configuring for culture: {0}", culture);
            services.Configure<RequestLocalizationOptions>(_ =>
            {
                _.DefaultRequestCulture = new RequestCulture(culture);
                _.SupportedCultures = new List<CultureInfo> { new CultureInfo(culture) };
                _.SupportedUICultures = new List<CultureInfo> { new CultureInfo(culture) };
            });

            // configure distributed cache
            string redisConfiguration
                        = _config[Keys.Configuration.DistributedCacheRedisConfiguration]
                        ?? throw new Exception($"{Keys.Configuration.DistributedCacheRedisConfiguration} is not set.");
            string instanceName = Keys.CacheInstance.its;
            if (!instanceName.EndsWith("."))
            {
                instanceName += ".";
            }
            string cacheDiscriminator
                = _config[Keys.Configuration.DistributedCacheInstanceDiscriminator]
                ?? string.Empty;
            if (!string.IsNullOrEmpty(cacheDiscriminator))
            {
                instanceName = $"{instanceName}{cacheDiscriminator}.";
            }
            _logger.LogInformation("Using Redis distributed cache {0} instance {1}",
                redisConfiguration,
                instanceName);
            services.AddDistributedRedisCache(_ =>
            {
                _.Configuration = redisConfiguration;
                _.InstanceName = instanceName;
            });
            var redis = ConnectionMultiplexer.Connect(redisConfiguration);
            services.AddDataProtection()
                .PersistKeysToRedis(redis,
                    $"{CacheInstanceInternal}.{DataProtectionKeyKey}");

            // configure session
            var sessionTimeout = TimeSpan.FromHours(2 * 60);

            services.AddSession(_ =>
            {
                _.IdleTimeout = sessionTimeout;
                _.Cookie.HttpOnly = true;
            });

            // configure authentication
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(_ =>
                {
                    _.AccessDeniedPath = "/Home/Unauthorized";
                    _.LoginPath = "/Home/Authenticate";
                });

            // configure mvc
            services.AddMvc()
                .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1)
                .AddSessionStateTempDataProvider();

            // Add data context
            services.AddScoped<ComputerUsageContext>();

            // filters
            services.AddScoped<Filters.AuthenticationFilter>();
            services.AddScoped<Filters.CachedDataFilter>();

            // helpers
            services.AddScoped<Helpers.WebHelper>();

            return services.BuildServiceProvider();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // use the culture configured above in services
            app.UseRequestLocalization();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseStatusCodePagesWithReExecute("/Error/Index/{0}");
            }

            // insert remote address into the log context for each request
            app.Use(async (context, next) =>
            {
                // use Edge rendering even if the site is in the Intranet zone
                context.Response.Headers["X-UA-Compatible"] = "IE=edge";
                using (Serilog.Context
                    .LogContext
                    .PushProperty("RemoteAddress", context.Connection.RemoteIpAddress))
                {
                    await next.Invoke().ConfigureAwait(false);
                }
            });

            app.UseStaticFiles();

            app.UseSession();

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
