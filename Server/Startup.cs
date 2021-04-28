using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NestorHub.Sentinels.Domain.Class;
using NestorHub.Sentinels.Domain.Interfaces;
using NestorHub.Sentinels.Infra;
using NestorHub.Server.Class;
using NestorHub.Server.Domain.Class;
using NestorHub.Server.Domain.Interfaces;
using NestorHub.Server.Hubs;
using NestorHub.Server.Interfaces;

namespace NestorHub.Server
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;
        private static string _packageStorePath;
        private static PackageRunner _packageRunner;
        private static PackagesStore _packagesStore;

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            _env = env;
            Configuration = configuration;
            _packageStorePath = configuration.GetValue<string>("PackagesStorePath");
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSignalR();

            AddDomainServices(services, _env);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime)
        {
            applicationLifetime.ApplicationStopping.Register(OnShutDown);

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });

            app.UseRewriter(new RewriteOptions().AddRedirect(@"(.*)/isonline", @"/connection", (int)HttpStatusCode.Redirect));

            app.Use(async (context, next) =>
            {
                var forwardedPath = context.Request.Headers["X-Forwarded-Path"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedPath))
                {
                    context.Request.PathBase = forwardedPath;
                }

                await next();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            AddCors(app);

            AddSignalRRoutes(app);

            app.UseHttpsRedirection();
            app.UseMvc();

            DeletePackagesMarkedToUninstallAndThenLoadInstancesToRun(_packagesStore, _packageRunner);
        }

        private void OnShutDown()
        {
            _packageRunner?.StopAllInstances();
        }

        private static void AddCors(IApplicationBuilder app)
        {
            app.UseCors(builder =>
                builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetIsOriginAllowed((host) => true)
                    .AllowCredentials()
            );
        }

        private static void AddSignalRRoutes(IApplicationBuilder app)
        {
            app.UseSignalR(route =>
            {
                route.MapHub<SubscriptionsHub>("/subscriptionshub");
                route.MapHub<StateValueHub>("/statevaluehub");
            });
        }

        private static void AddDomainServices(IServiceCollection services, IHostingEnvironment env)
        {
            services.AddSingleton<IStateValueManagement, StateValueManagement>();
            services.AddSingleton<ISubscribeManager, SubscribeManager>();

            var packageToRunStore = new PackageToRunStorage(_packageStorePath);
            services.AddSingleton<IPackageToRunStorage>(packageToRunStore);

            _packagesStore = new PackagesStore(_packageStorePath, new PackageToDeleteStorage(_packageStorePath), new PackageInfoDefinitionStorage());
            services.AddSingleton<IPackagesStore>(_packagesStore);

            var packageInstances = new PackagesInstances(packageToRunStore);
            services.AddSingleton<IPackagesInstances>(packageInstances);

            _packageRunner = new PackageRunner(_packageStorePath, packageInstances, new HostingConfiguration());
            services.AddSingleton<IPackageRunner>(_packageRunner);
        }

        private static void DeletePackagesMarkedToUninstallAndThenLoadInstancesToRun(PackagesStore packagesStore,
            PackageRunner packageRunner)
        {
            packagesStore.UninstallPackagesMarkedToDelete();
            packageRunner.RunAllInstancesOnServerStart();
        }
    }
}
