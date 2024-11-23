using WatsonWebserver.Core;

namespace XNetTools.REST.Backend.Server
{
    public class RouteContainer
    {
        readonly public WatsonWebserver.Core.HttpMethod RequestType;
        readonly public Func<HttpContextBase, Task> RequestHandler;
        readonly public string RouteSubURL;
        public RouteContainer(WatsonWebserver.Core.HttpMethod type, Func<HttpContextBase, Task> method, string route)
        {
            RequestType = type;
            RequestHandler = method;
            RouteSubURL = route;
        }
    }
}
