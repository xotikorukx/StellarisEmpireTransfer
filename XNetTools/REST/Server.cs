using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WatsonWebserver;
using WatsonWebserver.Core;
using WatsonWebserver.Extensions.HostBuilderExtension;
using XNetTools.REST.Backend.Server;
using static WatsonWebserver.Core.WebserverSettings;

namespace XNetTools.REST
{
    public class Server
    {
        Webserver server;
        public Server(int port, IEnumerable<RouteContainer> registerRoutes, Func<HttpContextBase, Task>? defaultMethod = null)
        {
            WebserverSettings settings = new WebserverSettings();
            settings.Hostname = "0.0.0.0";
            settings.Port = port;

            SslSettings sslSettings = new SslSettings();
            sslSettings.Enable = false;
            settings.Ssl = sslSettings; //TODO

            HostBuilder serverScaffold = new HostBuilder(settings, defaultMethod is null ? defaultMethod : BadRoute);

            foreach (RouteContainer routeContainer in registerRoutes)
            {
                if (routeContainer.RouteSubURL.Contains("{"))
                {
                    serverScaffold.MapParameterRoute(routeContainer.RequestType, routeContainer.RouteSubURL, routeContainer.RequestHandler);
                } else
                {
                    serverScaffold.MapStaticRoute(routeContainer.RequestType, routeContainer.RouteSubURL, routeContainer.RequestHandler);
                }
            }

            server = serverScaffold.Build();
            server.Start();
        }

        public static async Task BadRoute(HttpContextBase ctx)
        {
            await ctx.Response.Send("Bad route.");
        }
    }
}
