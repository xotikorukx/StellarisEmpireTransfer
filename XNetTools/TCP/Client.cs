
using WatsonTcp;
using XNetTools.TCP.Backend.Client.CustomEventArgs;

namespace XNetTools.TCP
{
    public class Client
    {
        public string PublicIP { get; private set; }
        public delegate void PacketRecievedEventHandler(object sender, ClientMessageEventArgs e);
        public event PacketRecievedEventHandler PacketRecieved;
        List<string> Hosts;
        int Port;
        WatsonTcpClient connection;
        public Client(List<string> hosts, int port)
        {
            Hosts = hosts;
            Port = port;

        }

        public void Disconnect()
        {
            connection.Disconnect();
        }
        public async Task<bool> Connect()
        {
            if (connection is not null)
            {
                return false;
            }



            foreach (string host in Hosts)
            {
                try
                {
                    WatsonTcpClient client = new WatsonTcpClient(host, Port);
                    client.Events.StreamReceived += OnPacketStreamRecieved;

                    if (host == "127.0.0.1")
                    {
                        client.Settings.ConnectTimeoutSeconds = 1;
                    }
                    else
                    {
                        client.Settings.ConnectTimeoutSeconds = 3;
                    }

                    client.Connect();

                    if (client.Connected)
                    {
                        connection = client;
                        connection.Events.ServerDisconnected += OnDisconnectedFromServer;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to connect to {host}:{Port} - {ex.Message}");
                }
            }

            return false;
        }
        public async Task<bool> Connect2()
        {
            if (connection is not null)
            {
                return false;
            }

            CancellationTokenSource cts = new CancellationTokenSource();

            List<Task> connectionAttemptTasks = new List<Task>();

            foreach (string host in Hosts)
            {
                connectionAttemptTasks.Add(Task.Run(() => {
                    try
                    {
                        WatsonTcpClient client = new WatsonTcpClient(host, Port);
                        client.Events.StreamReceived += OnPacketStreamRecieved;

                        if (host == "127.0.0.1")
                        {
                            client.Settings.ConnectTimeoutSeconds = 1;
                        }
                        else
                        {
                            client.Settings.ConnectTimeoutSeconds = 3;
                        }

                        client.Connect();

                        if (client.Connected)
                        {
                            connection = client;
                            connection.Events.ServerDisconnected += OnDisconnectedFromServer;
                            cts.Token.ThrowIfCancellationRequested();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to connect to {host}:{Port} - {ex.Message}");
                    }
                }, cts.Token));
            }

            try
            {
                await Task.WhenAny(connectionAttemptTasks);
                cts.Cancel();
            }
            catch (OperationCanceledException)
            {

            }

            foreach (var task in connectionAttemptTasks)
            {
                if (task.IsCompletedSuccessfully)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnPacketStreamRecieved(object? sender, StreamReceivedEventArgs e)
        {
            ReadMessage(e.DataStream);
        }

        private void OnDisconnectedFromServer(object? sender, DisconnectionEventArgs e)
        {
            connection = null;
        }

        public void SendMessage(ushort messageId, byte version, byte[] data)
        {
            MemoryStream ms = new();
            BinaryWriter bw = new(ms);
            bw.Write(messageId);
            bw.Write(version);
            bw.Write(data.Length);
            bw.Write(data);

            connection.SendAsync(ms.ToArray());
        }

        public void ReadMessage(Stream stream)
        {
            Console.WriteLine("ReadMessage");
            BinaryReader br = new BinaryReader(stream);
            ushort messageId = br.ReadUInt16();
            byte version = br.ReadByte();
            int length = br.ReadInt32();

            if (messageId == 0) //SetMyIPPacket
            {
                Console.WriteLine("Get SetMyIPPacket");
                PublicIP = br.ReadString();
                Console.WriteLine($" - Data: `{PublicIP}`");
                return;
            }

            PacketRecieved?.Invoke(this, new ClientMessageEventArgs(this, messageId, version, length, br));
        }
    }
}
