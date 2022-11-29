using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Owin.Security;
using NuGet.Server.Core.Infrastructure;
using NuGet.Server.Core.Logging;
using NuGet.Server.V2;
using Owin;
using static System.Net.Mime.MediaTypeNames;

namespace Atas.NuGet.Web
{

	public class Startup
	{
		public void Configuration(IAppBuilder appBuilder)
		{
			var config = new HttpConfiguration();

			config.SuppressDefaultHostAuthentication();

			appBuilder.UseBasicAuthentication();

			var builder = new ContainerBuilder();
			builder.RegisterWebApiFilterProvider(config);

			ConfigureIoC(builder);

			var container = builder.Build();
			appBuilder.UseAutofacMiddleware(container);
			config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

			config.Routes.MapHttpRoute
			(
				name: "DefaultApi",
				routeTemplate: "api/{controller}/{id}",
				defaults: new {id = RouteParameter.Optional}
			);

			appBuilder.UseAutofacWebApi(config);
			appBuilder.UseWebApi(config);

			config.UseNuGetV2WebApiFeed
			(
				routeName: "NuGet",
				routeUrlRoot: "nuget",
				oDatacontrollerName: "NuGet"
			);
		}

		private void ConfigureIoC(ContainerBuilder builder)
		{
			builder.RegisterApiControllers(typeof(Startup).Assembly);

			builder.Register(c => new ApiKeyPackageAuthenticationService(false, null))
				.As<IPackageAuthenticationService>()
				.SingleInstance();

			builder.Register(c => new DictionarySettingsProvider(new Dictionary<string, object>
				{
					{"enableDelisting", false}, //default=false
					{"enableFrameworkFiltering", false}, //default=false
					{"ignoreSymbolsPackages", true}, //default=false,
					{"allowOverrideExistingPackageOnPush", true}
				}))
				.As<ISettingsProvider>()
				.SingleInstance();

			builder.RegisterType<ConsoleLogger>()
				.As<ILogger>()
				.SingleInstance();

			builder.Register(c => NuGetV2WebApiEnabler.CreatePackageRepository
				(
					@"NuGetRepository",
					c.Resolve<ISettingsProvider>(),
					c.Resolve<ILogger>()
				)).As<IServerPackageRepository>()
				.SingleInstance();
		}
	}
}