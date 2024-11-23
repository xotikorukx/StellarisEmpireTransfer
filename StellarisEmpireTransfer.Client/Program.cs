using Microsoft.Extensions.Configuration;
using StellarisEmpireTransfer.Core;
using System.Drawing;

namespace StellarisEmpireTransfer.Client
{
    internal enum Screen
    {
        RoomSelectScreen,
        EmpiresScreen,
        LeaveRoomScreen,
        OverwriteAllScreen,
        EmpireScreen
    }
    internal class Program
    {
        //TODO: BUild autoupdater from github
        static EmpireManager empireManager = new EmpireManager(false);
        static UserSettings userPrefs;
        static XNetTools.REST.Client connection;
        static void Main(string[] args)
        { 
            SetUpUserSettings();

            StartAsync();

            while (true);
        }

        static async void StartAsync()
        {
            connection = new XNetTools.REST.Client("stellyapi.darkfeather.net");

            string response = await connection.GETAsString("ping");

            if (string.IsNullOrEmpty(response))
            {
                Console.WriteLine("Failed to connect to the server!", Color.Red);
                return;
            }

            await ProgramLoop();
        }

        static void SetUpUserSettings()
        {
            userPrefs = new UserSettings();
            userPrefs.EnsureExists();

            var builder = new ConfigurationBuilder();

            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("usersettings.json");

            IConfiguration configuration = builder.Build();
            configuration.GetSection("UserSettings").Bind(userPrefs);
        }

        static async Task ProgramLoop()
        {
            Screen currentScreen = Screen.RoomSelectScreen;
            
            do
            {
                switch(currentScreen)
                {
                    case Screen.RoomSelectScreen:
                        currentScreen = Screens.RoomSelectScreen.Show(userPrefs);
                        break;

                    case Screen.EmpiresScreen:
                        currentScreen = await Screens.EmpiresScreen.Show(connection, empireManager);
                        break;

                    case Screen.LeaveRoomScreen:
                        currentScreen = Screens.LeaveRoomScreen.Show(userPrefs);
                        break;

                    case Screen.OverwriteAllScreen:
                        currentScreen = await Screens.OverwriteAllScreen.Show(connection, empireManager);
                        break;

                    case Screen.EmpireScreen:
                        currentScreen = await Screens.EmpireScreen.Show(connection, empireManager);
                        break;
                }
            } while (true);
        }
    }
}
