using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CustomEmpireManager = EmpireManager.EmpireManager;

namespace Server
{
    internal static class Program
    {
        static CustomEmpireManager empireManager;

        static async Task Main(string[] args)
        {
            empireManager = new CustomEmpireManager(true);


            int port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);

            Console.WriteLine($"Starting TCP server on port {port}");

            var ipEndPoint = new IPEndPoint(IPAddress.Any, port);
            TcpListener connector = new TcpListener(ipEndPoint);

            connector.Start();
            Console.WriteLine("Listening for incoming connections...");

            while (true)
            {
                TcpClient client = await connector.AcceptTcpClientAsync();
                _ = Task.Run(() => ClientConnected(client));
            }
        }

        static async Task ClientConnected(TcpClient client)
        {
            Console.WriteLine($"[{client.Client.RemoteEndPoint}]: Connected");

            var disconnectToken = new TaskCompletionSource<bool>();

            _ = Task.Run(() => HeartbeatLoop(client, disconnectToken));
            _ = Task.Run(() => IncomingPacketLoop(client, disconnectToken));

            await disconnectToken.Task;

            DisconnectClient(client, 56);
        }

        static async Task HeartbeatLoop(TcpClient client, TaskCompletionSource<bool> disconnectToken)
        {
            try
            {
                while (true)
                {
                    empireManager.SendPacket(client, 28181, Array.Empty<byte>());
                    await Task.Delay(15000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HeartbeatLoop error: {ex.Message}");
                disconnectToken.TrySetResult(true);
            }
        }

        static void DisconnectClient(TcpClient client, byte exitCode)
        {
            try
            {
                Console.WriteLine($"[{client.Client.RemoteEndPoint}]: Disconnected. Code {exitCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging disconnection: {ex.Message}");
            }

            try
            {
                client.Close();
                Console.WriteLine($"[{client.Client.RemoteEndPoint}]: Client closed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing client: {ex.Message}");
            }
        }

        static async Task IncomingPacketLoop(TcpClient client, TaskCompletionSource<bool> disconnectToken)
        {
            try
            {
                while (true)
                {
                    var packet = await empireManager.ReceivePacket(client);

                    switch (packet.Item1)
                    {
                        case 10000: // RequestAllEmpires
                            Console.WriteLine($"[{client.Client.RemoteEndPoint}]: RequestAllEmpires");
                            byte[] output = CustomEmpireManager.SerializeString(empireManager.ReadFile());
                            empireManager.SendPacket(client, 20000, output);
                            break;

                        case 10001: // UploadEmpire
                            Console.WriteLine($"[{client.Client.RemoteEndPoint}]: UploadEmpire");
                            string incomingEmpireData = CustomEmpireManager.DeserializeString(packet.Item2);
                            string incomingEmpireName = empireManager.GetAllEmpireNames(incomingEmpireData).ElementAt(0);
                            string existingEmpireData = empireManager.ReadFile();
                            Console.WriteLine($"Updating empire `{incomingEmpireName}`");
                            string newEmpireData = empireManager.UpdateEmpire(incomingEmpireName, incomingEmpireData, existingEmpireData);
                            empireManager.WriteFile(newEmpireData);
                            break;

                        case 10002: // DeleteEmpire
                            Console.WriteLine($"[{client.Client.RemoteEndPoint}]: DeleteEmpire");
                            empireManager.WriteFile(empireManager.DeleteEmpire(CustomEmpireManager.DeserializeString(packet.Item2), empireManager.ReadFile()));
                            break;

                        case 18181: // HeartbeatResponse
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"IncomingPacketLoop error: {ex.Message}");

                disconnectToken.TrySetResult(true);


            }
        }
    }
}
