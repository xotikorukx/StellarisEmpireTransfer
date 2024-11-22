using System.Net.Sockets;
using System.Configuration;
using CustomEmpireManager = EmpireManager.EmpireManager;

namespace StellarisEmpireTransferClient
{
    internal static class Program
    {
        static TcpClient connector = new TcpClient();
        static CustomEmpireManager empireManager;
        static bool overwriteChanges = false;
        static bool serverUpdateComplete = true;
        static bool hasConnectedToServer = false;

        static void Main(string[] args)
        {
            Console.WriteLine("Verifying empires file...");
            empireManager = new CustomEmpireManager(false);
            string fileData = empireManager.ReadFile();

            Console.WriteLine("Connecting to server...");
            ConnectToServer();

            while (!hasConnectedToServer) { }

            RequestServerEmpires();
            GetUserSelectedEmpire(fileData);

            while (true)
            {
                fileData = empireManager.ReadFile();
                GetUserSelectedEmpire(fileData);
            }
        }

        private static void RequestServerEmpires()
        {
            serverUpdateComplete = false;
            Console.WriteLine("Requesting server empires...");
            empireManager.SendPacket(connector, 10000, Array.Empty<byte>());
        }

        private static async void ConnectToServer()
        {
            try
            {
                int port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);

                if (await IsPortOpenAsync("127.0.0.1", port, 1000))
                {
                    Console.WriteLine("Localhost server available!");
                    await Task.Delay(1000);
                    await connector.ConnectAsync("127.0.0.1", port);
                } else if (await IsPortOpenAsync(ConfigurationManager.AppSettings["RemoteHostName"], port, 1000))
                {
                    Console.WriteLine("Domain server available!");
                    await Task.Delay(1000);
                    await connector.ConnectAsync(ConfigurationManager.AppSettings["RemoteHostName"], port);
                }
                else
                {
                    Console.WriteLine("Attempting fallback server...");
                    await Task.Delay(1000);
                    await connector.ConnectAsync(ConfigurationManager.AppSettings["RemoteHostIP"], port);
                }

                hasConnectedToServer = true;
                Console.WriteLine("Connected to server.");

                _ = Task.Run(() => IncomingPacketLoop());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to server: {ex.Message}");
            }
        }

        private static void GetUserSelectedEmpire(string empireData)
        {
            List<string> empireNames = empireManager.GetAllEmpireNames(empireData);
            Console.WriteLine("Select an option by typing its number and hitting enter below:\n");

            Console.WriteLine("[1] Download all empires from server (Overwrites! Upload your changes first!)");

            int index = 2;

            foreach (string name in empireNames)
            {
                Console.WriteLine($"[{index}] `{name}`");
                index++;
            }

            Console.Write("\nIndex: ");

            string? input = null;
            bool isInputValid = false;
            int outInput = -1;

            do
            {
                input = Console.ReadLine();

                if (input == null)
                {
                    Console.WriteLine($"Index cannot be empty.");
                    continue;
                }

                int inputIndex;

                try
                {
                    inputIndex = Convert.ToInt32(input);
                }
                catch
                {
                    Console.WriteLine($"Invalid Index: `{input}`");
                    continue;
                }

                isInputValid = true;
                outInput = inputIndex;

            } while (!isInputValid);

            if (outInput == 1)
            {
                overwriteChanges = true;
                RequestServerEmpires();
                while (!serverUpdateComplete) { }
                return;
            }

            string empireName = empireNames.ElementAt(outInput - 2);
            Console.WriteLine($"Selected: `{empireName}`");
            Console.WriteLine("- Upload to server (u)");
            Console.WriteLine("- Delete from server (d)");
            Console.WriteLine("- Cancel (c/enter)");
            Console.Write("u/d/c?: ");

            string? readLine = Console.ReadLine();

            if (readLine == null || readLine == "c")
            {
                return;
            }
            else if (readLine == "u")
            {
                Console.WriteLine($"Uploading `{empireName}` to server...");
                byte[] uEmpireData = CustomEmpireManager.SerializeString(empireManager.GetEmpireData(empireName, empireData));
                empireManager.SendPacket(connector, 10001, uEmpireData);
            }
            else if (readLine == "d")
            {
                Console.WriteLine($"Deleting `{empireName}` from server...");
                empireManager.SendPacket(connector, 10002, CustomEmpireManager.SerializeString(empireName));
            }
        }

        private static async Task<bool> IsPortOpenAsync(string host, int port, int timeout)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var task = client.ConnectAsync(host, port);
                    if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
                    {
                        return client.Connected;
                    }
                    else
                    {
                        return false; // Timeout
                    }
                }
            }
            catch
            {
                return false; // Exception occurred
            }
        }

        static async void IncomingPacketLoop()
        {
            try
            {
                while (true)
                {
                    var packet = await empireManager.ReceivePacket(connector);
                    //Console.WriteLine($"Received packet with ID {packet.Item1}");

                    switch (packet.Item1)
                    {
                        case 20000: // RequestAllEmpiresResponse
                            Console.WriteLine("Got server empires");
                            string empiresServerData = CustomEmpireManager.DeserializeString(packet.Item2);
                            List<string> serverEmpires = empireManager.GetAllEmpireNames(empiresServerData);
                            string empiresFile = empireManager.ReadFile();
                            List<string> clientEmpires = empireManager.GetAllEmpireNames(empiresFile);
                            List<string> clientMissingEmpireNames = serverEmpires.Except(clientEmpires).ToList();

                            if (!overwriteChanges)
                            {
                                foreach (string missingEmpireName in clientMissingEmpireNames)
                                {
                                    Console.WriteLine($" - Adding missing empire `{missingEmpireName}`");
                                    empiresFile = empireManager.UpdateEmpire(missingEmpireName, empireManager.GetEmpireData(missingEmpireName, empiresServerData), empiresFile);
                                }

                                serverUpdateComplete = true;
                            }
                            else
                            {
                                foreach (string missingEmpireName in clientMissingEmpireNames)
                                {
                                    Console.WriteLine($" - Adding missing empire `{missingEmpireName}`");
                                }

                                foreach (string empireName in serverEmpires)
                                {
                                    empiresFile = empireManager.UpdateEmpire(empireName, empireManager.GetEmpireData(empireName, empiresServerData), empiresFile);
                                }

                                serverUpdateComplete = true;
                                Console.WriteLine("!! RELOAD STELLARIS TO SEE OVERWRITTEN CHANGES !!");
                            }

                            empireManager.WriteFile(empiresFile);
                            break;

                        case 28181: // HeartbeatResponse
                            //Console.WriteLine("Received HeartbeatResponse");
                            empireManager.SendPacket(connector, 18181, Array.Empty<byte>());
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in IncomingPacketLoop: {ex.Message}");
                connector.Close();
            }
        }
    }
}
