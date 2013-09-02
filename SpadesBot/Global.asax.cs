using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace SpadesBot
{
    public class SpadesApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            RouteTable.Routes.MapHubs();
            RouteTable.Routes.IgnoreRoute("signalr/{*pathInfo}");
            GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                "api", "{gameId}/{action}", new { controller = "Spades", gameId = RouteParameter.Optional });
        }
    }
}