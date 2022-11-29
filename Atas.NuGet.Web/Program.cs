using System;
using System.Linq;
using System.Net;
using Microsoft.Owin.Hosting;

namespace Atas.NuGet.Web
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var baseAddress = args.FirstOrDefault() ?? $"http://{Dns.GetHostName()}:8080/";

			using (var pap = WebApp.Start<Startup>(new StartOptions(baseAddress)))
			{
				Console.WriteLine("Server listening at baseaddress: " + baseAddress);
				Console.WriteLine("[ENTER] to close server");
				Console.ReadLine();
			}
		}
	}
}