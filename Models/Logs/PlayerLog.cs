using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokerBot.Classes;

namespace PokerBot.Models.Logs
{
    class PlayerLog : LogObject
    {
        public ulong PlayerId { get; set; }
        public string PlayerName { get; set; }

        public PlayerLog(PokerPlayer player, Severity severity, string content)
        {
            PlayerId = player.GetId();
            PlayerName = player.GetName();
            LogSeverity = severity;
            LogContent = content;
        }

        public PlayerLog(PokerPlayer player, Severity severity, string content, Exception exception)
        {
            PlayerId = player.GetId();
            PlayerName = player.GetName();
            LogSeverity = severity;
            LogContent = content;
            StoredException = exception;
        }

        public override string ToString()
        {
            return $"'Player {PlayerName}' ({PlayerId}), {LogContent}";
        }
    }
}
