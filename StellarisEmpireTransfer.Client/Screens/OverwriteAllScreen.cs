using StellarisEmpireTransfer.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StellarisEmpireTransfer.Client.Screens
{
    internal static class OverwriteAllScreen
    {

        internal static async Task<Screen> Show(XNetTools.REST.Client httpclient, EmpireManager empireManager)
        {
            Console.Clear();
            Console.WriteLine($"You are about download ALL empires from the server, then overwrite empires");
            Console.WriteLine($"on your computer that also have a copy on the server with the servers copies.");
            Console.WriteLine();
            Console.WriteLine("Make sure you've uploaded any changes you've made before doing this.");
            Console.WriteLine(" ---------------- ");
            Console.WriteLine("Are you SURE?", Color.Orange);
            Console.WriteLine();
            Console.WriteLine();
            Console.Write("Type the letter `Y` to leave the room, otherwise, hit `Enter` to go back.");

            string input = Console.ReadLine();

            if (input != "Y")
            {
                return Screen.EmpiresScreen;
            }

            string ServerEmpiresData = await httpclient.GETAsString($"/room/{State.CurrentRoomName}");
            string ClientEmpiresData = empireManager.ReadFile();

            List<string> serverEmpireNames = empireManager.GetAllEmpireNames(ServerEmpiresData);
            
            foreach( string serverEmpireName in serverEmpireNames )
            {
                string serverEmpireData = empireManager.GetEmpireData(serverEmpireName, ServerEmpiresData);
                ClientEmpiresData = empireManager.UpdateEmpire(serverEmpireName, serverEmpireData, ClientEmpiresData);
            }

            return Screen.EmpiresScreen;
        }



    }
}
