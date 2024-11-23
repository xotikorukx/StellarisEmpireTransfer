using StellarisEmpireTransfer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarisEmpireTransfer.Client.Screens
{
    internal class EmpireScreen
    {
        internal static async Task<Screen> Show(XNetTools.REST.Client httpclient, EmpireManager empireManager, string? successMessage = null, string? errorMessage = null)
        {
            Console.Clear();
            Console.WriteLine($"Selected Empire: {State.SelectedEmpireName}");
            Console.WriteLine(" ---------------- ");

            if (State.SelectedEmpireExistsOnServer)
            {
                Console.WriteLine(" -Server Options- ");
                Console.WriteLine(" [D] DELETE from server");
                Console.WriteLine(" [P] Protect. Disallows ANY CHANGES. Can no longer be deleted from the server.");

                if (State.SelectedEmpireExistsOnClient) Console.WriteLine(" [F] Update from Server; OVERWRITE local copy.");
                else Console.WriteLine(" [F] Download from server.");

                Console.WriteLine();
            }

            if (State.SelectedEmpireExistsOnClient)
            {
                Console.WriteLine(" -Client Options- ");
                Console.WriteLine($" [U] Upload to server{(State.SelectedEmpireExistsOnServer ? "; OVERWRITE server copy" : "")}");
                Console.WriteLine($" [R] Delete your local copy.");

                Console.WriteLine();
            }

            if (State.SelectedEmpireExistsOnClient &&  State.SelectedEmpireExistsOnServer)
            {
                Console.WriteLine(" -Shared Options- ");
                Console.WriteLine(" [W] WIPE from BOTH server and your game.");
                Console.WriteLine();
            }

            Console.WriteLine(" [G] Go Back");

            Console.WriteLine();
            Console.WriteLine();

            Console.Write("Enter an option letter (such as `Y`): ");
            string input = Console.ReadLine();

            if (string.IsNullOrEmpty(input))
            {
                return await Show(httpclient, empireManager, null, null);
            }

            if (input == "D" && State.SelectedEmpireExistsOnServer)
            {
                //TODO: Delete from server.
            } else if (input == "F" && State.SelectedEmpireExistsOnServer)
            {
                //TODO: Update from server
            } else if (input == "P" && State.SelectedEmpireExistsOnServer)
            {
                //TODO: Protect.
            } 
            else if (input == "U" && State.SelectedEmpireExistsOnClient)
            {
                //TODO: Upload to server
            } else if (input == "R" && State.SelectedEmpireExistsOnClient)
            {
                //TODO: Delete local copy
            } else if (input == "W" && State.SelectedEmpireExistsOnServer && State.SelectedEmpireExistsOnClient)
            {
                //TODO: Delete local copy AND server copy.
            } else if (input == "G")
            {
                return Screen.EmpiresScreen;
            }

            //TODO: Add error and success sections.
            return await Show(httpclient, empireManager, null, $"Invalid option: `{input}`");
        }
    }
}

