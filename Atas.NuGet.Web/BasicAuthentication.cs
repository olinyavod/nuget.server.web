using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Owin;

namespace Atas.NuGet.Web
{

static class BasicDefaults
{
	public const string AuthenticationType = "Basic";
}

public class BasicAuthenticationOptions : AuthenticationOptions
{
	/// <summary>
	/// Creates an instance of API Key authentication options with default values.
	/// </summary>
	public BasicAuthenticationOptions()
		: base(BasicDefaults.AuthenticationType)
	{
	}
}

public class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
{
	private readonly ILogger _logger;

	public BasicAuthenticationHandler(ILogger logger)
	{
		this._logger = logger;
	}

	protected override async Task<Microsoft.Owin.Security.AuthenticationTicket> AuthenticateCoreAsync()
	{
		var properties = new AuthenticationProperties();
		// Find apiKey in default location
		string apiKey = null;
		var authorization = Request.Headers.Get("Authorization");
		if (!string.IsNullOrEmpty(authorization))
		{
			var authHeader = System.Net.Http.Headers.AuthenticationHeaderValue.Parse(authorization);

			if (string.CompareOrdinal(authHeader.Scheme, BasicDefaults.AuthenticationType) != 0)
				return new AuthenticationTicket(null, properties);

			var parameter = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter));
			var parts = parameter.Split(':');

			var username = parts[0];
			var password = parts[1];
			if (!await ValidateUser(username, password))
				return new AuthenticationTicket(null, properties);

		}
		else
		{
			Response.OnSendingHeaders(state =>
			{
				var response = (IOwinResponse) state;

				response.Headers.Add("WWW-Authenticate", new[] { BasicDefaults.AuthenticationType });
				
			}, Response);

			_logger.WriteWarning("Authorization header not found");

			return new AuthenticationTicket(null, properties);
		}

		var userClaim = new Claim(ClaimTypes.Name, "gvdasa");
		var allClaims = Enumerable.Concat(new Claim[] { userClaim }, Enumerable.Empty<Claim>());

		var identity = new ClaimsIdentity(allClaims, BasicDefaults.AuthenticationType);
		var principal = new ClaimsPrincipal(new ClaimsIdentity[] { identity });

		// resulting identity values go back to caller
		return new AuthenticationTicket(identity, properties);
	}

	private Task<bool> ValidateUser(string username, string password)
	{
		return Task.FromResult(true);
	}
}

public class BasicAuthenticationMiddleware : AuthenticationMiddleware<BasicAuthenticationOptions>
{
	private readonly ILogger logger;

	public BasicAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, BasicAuthenticationOptions options)
		: base(next, options)
	{
		this.logger = app.CreateLogger<AuthenticationHandler>();
	}

	protected override AuthenticationHandler<BasicAuthenticationOptions> CreateHandler()
	{
		return new BasicAuthenticationHandler(logger);
	}
}

public static class BasicAuthenticationExtensions
{
	public static IAppBuilder UseBasicAuthentication(this IAppBuilder app, BasicAuthenticationOptions options = null)
	{
		if (app == null)
			throw new ArgumentNullException(nameof(app));
			

		app.Use(typeof(BasicAuthenticationMiddleware), app, options != null ? options : new BasicAuthenticationOptions());
		app.UseStageMarker(PipelineStage.Authenticate);
		return app;
	}
}
	}