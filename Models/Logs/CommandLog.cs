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
        public string Module { get; set; }
        public string Command { get; set; }
        public string Message { get; set; }
        
        public CommandLog(IUser user, string module, string command, string message, Severity severity, string content)
        {
            User = user;
            Module = module;
            Command = command;
            Message = message;
            LogSeverity = severity;
            LogContent = content;
        }

        public CommandLog(IUser user, string module, string command, string message, Severity severity, string content, Exception exception)
        {
            User = user;
            Module = module;
            Command = command;
            Message = message;
            LogSeverity = severity;
            LogContent = content;
            StoredException = exception;
        }

        public override string ToString() 
        {
            return $"User '{User.Username}' executed command '{Command}' in module {Module} with message: {Message}";
        }
    }
}
