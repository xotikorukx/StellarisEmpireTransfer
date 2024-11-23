using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarisEmpireTransfer.Client.Screens
{
    internal static class LeaveRoomScreen
    {
        internal static Screen Show(UserSettings prefs)
        {
            Console.Clear();
            Console.WriteLine($"You are about to leave the room `{State.CurrentRoomName}`!");
            Console.WriteLine("Are you SURE?", Color.Orange);
            Console.WriteLine();
            Console.WriteLine();
            Console.Write("Type the letter `Y` to leave the room, otherwise, hit `Enter` to go back.");

            string input = Console.ReadLine();

            if ( input == "Y" ) {
                prefs.LastRoom = null;
                prefs.Save();
                State.CurrentRoomName = null;

                return Screen.RoomSelectScreen;
            }

            return Screen.EmpiresScreen;
        }
    }
}
