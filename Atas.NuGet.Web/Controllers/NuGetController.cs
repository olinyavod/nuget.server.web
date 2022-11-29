using System.Web.Http;
using NuGet.Server.Core.Infrastructure;
using NuGet.Server.V2.Controllers;

namespace Atas.NuGet.Web.Controllers
{
	//[Authorize]
	public class NuGetController : NuGetODataController
	{
		public NuGetController
		(
			IServerPackageRepository repository,
			IPackageAuthenticationService authenticationService
		) : base(repository, authenticationService)
		{
		}
	}
}