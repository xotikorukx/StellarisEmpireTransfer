using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarisEmpireTransfer.Client
{
    internal static class State
    {
        internal static string CurrentRoomName;
        internal static string SelectedEmpireName;
        internal static bool SelectedEmpireExistsOnClient = false;
        internal static bool SelectedEmpireExistsOnServer = false;
    }
}
