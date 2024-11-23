using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarisEmpireTransfer.Core
{
    public static class IPConverter
    {
        public static string IP2Code(string IP)
        {
            string[] split = IP.Split('.');

            return $"{split[0].PadLeft(3, 'C')}-{split[1].PadLeft(3, 'O')}-{split[2].PadLeft(3, 'D')}-{split[3].PadLeft(3, 'E')}";
        }

        public static string Code2IP(string code)
        {
            return code.Replace('-', '.').Replace("C", "").Replace("O", "").Replace("D", "").Replace("E", "");
        }
    }
}
