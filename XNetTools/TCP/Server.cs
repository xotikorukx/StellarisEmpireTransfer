using WatsonTcp;
using XNetTools.TCP.Backend.Server;
using XNetTools.TCP.Backend.Server.CustomEventArgs;

namespace XNetTools.TCP
{
    public class Server
    {
        int Port;
        ulong serverId = 0;

        Dictionary<Guid, ClientConnection> connectedClients = new Dictionary<Guid, ClientConnection>();


        public delegate void ClientConnectedEventHandler(object sender, ClientConnectedEventArgs e);
        public event ClientConnectedEventHandler ClientConnected;

        public delegate void ClientDisconnectedEventHandler(object sender, ClientDisconnectedEventArgs e);
        public event ClientDisconnectedEventHandler ClientDisconnected;

        WatsonTcpServer server;
        public Server(int port)
        {
            Port = port;
            server = new WatsonTcpServer("0.0.0.0", Port);

            server.Events.ClientConnected += OnClientConnected;
            server.Events.ClientDisconnected += OnClientDisconnected;
            server.Events.StreamReceived += OnClientStreamMessageRecieved;

            server.Start();
        }

        private void OnClientStreamMessageRecieved(object? sender, StreamReceivedEventArgs e)
        {
            connectedClients[e.Client.Guid].ReadMessage(e.DataStream);
        }

        private void OnClientDisconnected(object? sender, DisconnectionEventArgs e)
        {
            ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(connectedClients[e.Client.Guid]));
            connectedClients.Remove(e.Client.Guid);
        }

        private void OnClientConnected(object? sender, ConnectionEventArgs e)
        {
            serverId++;
            ClientConnection wclient = new ClientConnection(server, e.Client, serverId, Port);
            connectedClients.Add(e.Client.Guid, wclient);
            ClientConnected?.Invoke(this, new ClientConnectedEventArgs(wclient));

            string publicIp = e.Client.IpPort.Split(':')[0];

            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(publicIp);

            wclient.SendMessage(0, 0, ms.ToArray());
        }
    }
}
