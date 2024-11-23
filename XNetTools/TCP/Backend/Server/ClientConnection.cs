using WatsonTcp;
using XNetTools.TCP.Backend.Server.CustomEventArgs;

namespace XNetTools.TCP.Backend.Server
{
    public class ClientConnection
    {
        public delegate void PacketRecievedEventHandler(object sender, ClientMessageEventArgs e);
        public event PacketRecievedEventHandler PacketRecieved;

        public readonly ulong serverId;
        WatsonTcpServer server;
        Guid ClientGUID = Guid.Empty;
        public string IP { get; private set; }
        public int Port { get; private set; }
        public ClientConnection(WatsonTcpServer serverInstance, ClientMetadata metadata, ulong ServerId, int port)
        {
            server = serverInstance;
            ClientGUID = metadata.Guid;
            serverId = ServerId;

            string[] split = metadata.IpPort.Split(':');

            IP = split[0];
            Port = port;
        }

        public void SendMessage(ushort messageId, byte version, byte[] data)
        {
            MemoryStream ms = new();
            BinaryWriter bw = new(ms);
            bw.Write(messageId);
            bw.Write(version);
            bw.Write(data.Length);
            bw.Write(data);

            server.SendAsync(ClientGUID, ms.ToArray());
        }

        public void ReadMessage(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            ushort messageId = br.ReadUInt16();
            byte version = br.ReadByte();
            int length = br.ReadInt32();

            PacketRecieved?.Invoke(this, new ClientMessageEventArgs(this, messageId, version, length, br));
        }
    }
}
