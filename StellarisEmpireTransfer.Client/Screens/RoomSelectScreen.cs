using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarisEmpireTransfer.Client.Screens
{
    internal static class RoomSelectScreen
    {
        internal static Screen Show(UserSettings prefs, string? errorMessage = null)
        {
            Console.Clear();

            if (!string.IsNullOrEmpty(prefs.LastRoom))
            {
                State.CurrentRoomName = prefs.LastRoom;
                return Screen.EmpiresScreen;
            }

            Console.WriteLine("ROOM SELECTION");
            Console.WriteLine(" ---------------- ");

            if (errorMessage != null)
            {
                Console.WriteLine($"ERROR: {errorMessage}. Try again!", Color.Red);
                Console.WriteLine(" ---------------- ");
            }

            Console.WriteLine("    Enter a room name to join.");
            Console.WriteLine("     - If you are setting up a room, choose a unique name!");
            Console.WriteLine("        That's how others will check out your room!");
            Console.WriteLine("     - If you are joining someone else's room, enter the room name they gave you.");
            Console.WriteLine("        Others that join will be able to upload, delete, and download empires just like you were.");
            Console.WriteLine();
            Console.WriteLine();
            Console.Write("Room Name: ");

            string roomName = Console.ReadLine();

            if (string.IsNullOrEmpty(roomName)) return Show(prefs, "You gotta type in a name! Or, at least *something*. Anything?");

            prefs.LastRoom = roomName;
            prefs.Save();
            State.CurrentRoomName = roomName;
            return Screen.EmpiresScreen;
        }
    }
}
