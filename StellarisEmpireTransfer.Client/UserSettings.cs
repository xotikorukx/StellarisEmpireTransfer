using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StellarisEmpireTransfer.Client
{
    //TODO: Make save have defaults. We'll need it if I add more config in an update to preserve settings.
    internal class UserSettings
    {
        public string LastRoom { get; set; }

        public void Save()
        {
            File.WriteAllText("usersettings.json", JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        }

        public void EnsureExists()
        {
            if (File.Exists("usersettings.json")) return;

            LastRoom = null;

            Save();
        }
    }
}
