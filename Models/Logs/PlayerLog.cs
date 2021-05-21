using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerBot.Models.Logs
{
    class PlayerLog : LogObject
    {
        public ulong PlayerId { get; set; }
        public string PlayerName { get; set; }

        public override string ToString()
        {
            return $"{PlayerId} - '{PlayerName}'";
        }
    }
}
