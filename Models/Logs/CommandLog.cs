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

        public CommandLog(IUser user, Severity severity, string content)
        {
            User = user;
            LogSeverity = severity;
            LogContent = content;
        }

        public override string ToString() 
        {
            return $"User '{User.Username}' executed command with message: {LogContent}";
        }
    }
}
