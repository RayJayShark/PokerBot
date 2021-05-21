#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerBot.Models.Logs
{
    public abstract class LogObject
    {
        public enum Severity
        {
            Info,
            Warning,
            Error
        }
        
        public Severity LogSeverity { get; set; }
        public string LogContent { get; set; }
        public Exception? StoredException { get; set; }

    }
}
