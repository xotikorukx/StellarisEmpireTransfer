using StellarisEmpireTransfer.Core;
using System.Drawing;
using WatsonWebserver.Core;
using XNetTools.REST.Backend.Server;

namespace StellarisEmpireTransfer.Server
{
    internal class Program
    {
        //TODO: Find a way to only allow the player who uploaded an empire to delete it.
        //NOTE: For now we just need to be able to "protect" them.
        //  Create an empty file {empireName}.protected?
        //TODO: Lowercase room names. Always! Case shouldn't matter.

        static EmpireManager empireManager = new EmpireManager(true);
        static void Main(string[] args)
        {
            List<RouteContainer> routes = [
                new RouteContainer(WatsonWebserver.Core.HttpMethod.GET, Ping, "/ping"),
                new RouteContainer(WatsonWebserver.Core.HttpMethod.GET, GetAllEmpireNames, "/room/{roomname}/names"),
                new RouteContainer(WatsonWebserver.Core.HttpMethod.GET, GetAllEmpires, "/room/{roomname}"),
                new RouteContainer(WatsonWebserver.Core.HttpMethod.GET, GetEmpire, "/room/{roomname}/{empirename}"),
                new RouteContainer(WatsonWebserver.Core.HttpMethod.POST, UploadEmpire, "/room/{roomname}/{empirename}"),
                new RouteContainer(WatsonWebserver.Core.HttpMethod.DELETE, DeleteEmpire, "/room/{roomname}/{empirename}"),
            ];

            new XNetTools.REST.Server(35742, routes);
            Console.WriteLine($"Started StellarisEmpireTransferAPI on port {35742}!", Color.LimeGreen);

            while (true);
        }

        static async Task Ping(HttpContextBase ctx)
        {
            ctx.Response.Send("Pong!");
        }

        static async Task UploadEmpire(HttpContextBase ctx)
        {
            string roomName = ctx.Request.Url.Parameters["roomname"];
            bool exists = EnsureRoomExists(roomName);

            if (!exists)
            {
                await ctx.Response.Send();
                return;
            }

            string empireName = ctx.Request.Url.Parameters["empirename"];

            Console.WriteLine($"Uploading empire `{empireName}` to room `{roomName}`", Color.DimGray);

            File.WriteAllText($"./rooms/{roomName}/{empireName}", ctx.Request.DataAsString);
        }

        static async Task DeleteEmpire(HttpContextBase ctx)
        {
            string roomName = ctx.Request.Url.Parameters["roomname"];
            bool exists = EnsureRoomExists(roomName);

            if (!exists)
            {
                await ctx.Response.Send();
                return;
            }

            string empireName = ctx.Request.Url.Parameters["empirename"];

            Console.WriteLine($"Uploading empire `{empireName}` to room `{roomName}`");

            File.Delete($"./rooms/{roomName}/{empireName}");
        }

        static async Task GetAllEmpireNames(HttpContextBase ctx)
        {
            string roomName = ctx.Request.Url.Parameters["roomname"];
            bool exists = EnsureRoomExists(roomName);

            if (!exists)
            {
                await ctx.Response.Send();
                return;
            }

            List<string> empireNames = empireManager.GetAllEmpireNames(GetAllRoomEmpireData(roomName));

            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(empireNames.Count);

            foreach (string empireName in empireNames)
            {
                bw.Write(empireName);
            }

            await ctx.Response.Send(ms.ToArray());
        }

        static async Task GetAllEmpires(HttpContextBase ctx)
        {
            string roomName = ctx.Request.Url.Parameters["roomname"];
            bool exists = EnsureRoomExists(roomName);

            if (!exists)
            {
                await ctx.Response.Send();
                return;
            }

            Console.WriteLine($"Sending all empires from room `{roomName}`", Color.DimGray);

            await ctx.Response.Send(GetAllRoomEmpireData(roomName));
        }

        static string GetAllRoomEmpireData(string roomName)
        {
            string[] filePaths = Directory.GetFiles($"./rooms/{roomName}/");

            string output = "";

            foreach (string empirePath in filePaths)
            {
                output = $"{output}\n";
            }

            return output;
        }

        static async Task GetEmpire(HttpContextBase ctx)
        {
            string roomName = ctx.Request.Url.Parameters["roomname"];
            bool exists = EnsureRoomExists(roomName);

            if (!exists)
            {
                await ctx.Response.Send();
                return;
            }

            string empireName = ctx.Request.Url.Parameters["empirename"];
            string contents;

            try
            {
                contents = ReadEmpireFile(roomName, empireName);
            } catch (FileNotFoundException)
            {
                await ctx.Response.Send();
                return;
            }

            Console.WriteLine($"Requested empire `{empireName}` from room `{roomName}`", Color.DimGray);

            await ctx.Response.Send(contents);
        }

        static string ReadEmpireFile(string roomName, string empireName)
        {
            return File.ReadAllText($"./rooms/{roomName}/{empireName}");
        }

        static bool EnsureRoomExists(string roomName)
        {
            if (string.IsNullOrEmpty(roomName)) return false;
            Directory.CreateDirectory($"./Rooms/{roomName}");
            return true;
        }
    }
}
