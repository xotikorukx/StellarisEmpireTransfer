using StellarisEmpireTransfer.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StellarisEmpireTransfer.Client.Screens
{
    internal static class EmpiresScreen
    {
        internal static async Task<Screen> Show(XNetTools.REST.Client httpclient, EmpireManager empireManager, string? errorMessage = null)
        {
            Console.Clear();

            Console.WriteLine("Fetching empire names from the server...", Color.DimGray);

            //Fetch server empire names
            Stream stream = await httpclient.GETAsStream($"/room/{State.CurrentRoomName}/names");

            BinaryReader reader = new BinaryReader(stream);

            int namesSent = reader.ReadInt32();

            List<string> ServerEmpireNames = new List<string>();

            while ( namesSent > 0 )
            {
                string name = reader.ReadString();
                ServerEmpireNames.Add(name);
                namesSent--;
            }

            Console.Clear();

            //Give the user options
            Console.WriteLine($"ROOM `{State.CurrentRoomName}`");
            Console.WriteLine(" ---------------- ");
            if (errorMessage != null)
            {
                Console.WriteLine($"ERROR: {errorMessage}. Try again!", Color.Red);
                Console.WriteLine(" ---------------- ");
            }
            Console.WriteLine(" - Room Options -");
            Console.WriteLine(" [D] Download all Missing (SERVER) Empires");
            Console.WriteLine(" [L] Leave Room");
            Console.WriteLine(" [O] Download (SERVER) and (BOTH) empires. OVERWRITE Local (BOTH) Copies");
            Console.WriteLine(" ---------------- ");
            Console.WriteLine(" --- Empires ---");

            //Enumerate Empires
            string ClientEmpiresData = empireManager.ReadFile();
            List<string> ClientEmpiresNames = empireManager.GetAllEmpireNames(ClientEmpiresData);

            List<string> ClientMissingEmpireNames = ServerEmpireNames.Except(ClientEmpiresNames).ToList();
            List<string> ServerMissingEmpireNames = ClientEmpiresNames.Except(ServerEmpireNames).ToList();
            List<string> BothHasEmpireNames = ServerEmpireNames.Intersect(ClientEmpiresNames).ToList();
            List<string> CombinedEmpireNames = ClientMissingEmpireNames.Union(ServerMissingEmpireNames).Union(BothHasEmpireNames).ToList();

            CombinedEmpireNames.Sort();

            int index = 1;
            foreach ( string name in CombinedEmpireNames )
            {
                Console.Write($" [{index}] ");

                if (ClientMissingEmpireNames.Contains(name))
                {
                    Console.Write(" (SERVER) ");
                } else if (ServerMissingEmpireNames.Contains(name))
                {
                    Console.Write(" (CLIENT) ");
                } else
                {
                    Console.Write(" (BOTH)   ");
                }

                Console.Write($"{name}\n");
                index++;
            }

            Console.WriteLine();
            Console.WriteLine();

            Console.Write("Enter an option letter or empire number: ");
            string input = Console.ReadLine();

            if (string.IsNullOrEmpty(input))
            {
                return await Show(httpclient, empireManager);
            }

            if (input == "L")
            {
                return Screen.LeaveRoomScreen;
            }
            else if (input == "O") {
                return Screen.OverwriteAllScreen;
            } else if (input == "D")
            {
                string ServerEmpiresData = await httpclient.GETAsString($"/room/{State.CurrentRoomName}");

                List<string> MissingEmpiresNames = ServerEmpireNames.Except(ClientEmpiresNames).ToList();

                foreach (string missingEmpireName in MissingEmpiresNames)
                {
                    string incomingEmpireData = empireManager.GetEmpireData(missingEmpireName, ServerEmpiresData);
                    ClientEmpiresData = empireManager.UpdateEmpire(missingEmpireName, incomingEmpireData, ClientEmpiresData);
                }

                empireManager.WriteFile(ClientEmpiresData);
            }

            int selectedIndex;

            try
            {
                selectedIndex = Convert.ToInt32(input);
                State.SelectedEmpireName = CombinedEmpireNames.ElementAt(selectedIndex);
                State.SelectedEmpireExistsOnClient = ClientEmpiresNames.IndexOf(State.SelectedEmpireName) > -1;
                State.SelectedEmpireExistsOnServer = ServerEmpireNames.IndexOf(State.SelectedEmpireName) > -1;
                return Screen.EmpireScreen;
            } catch
            {
                return await Show(httpclient, empireManager, $"Invalid Option `{input}`");
            }
        }
    }
}
