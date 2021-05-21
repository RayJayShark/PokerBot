using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace PokerBot.Models.Logs
{
    class CommandLog : LogObject
    {
        public IUser User { get; set; }
        public string Command { get; set; }

        public CommandLog(IUser user, string command, Severity severity, string content)
        {
            User = user;
            Command = command;
            LogSeverity = severity;
            LogContent = content;
        }

        public override string ToString() 
        {
            return $"User '{User.Username}' executed command '{Command}' with message: {LogContent}";
        }
    }
}
