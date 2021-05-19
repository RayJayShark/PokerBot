#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerBot.Models
{
    public class Log
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


        public Log(Severity logSeverity, string logContent)
        {
            LogSeverity = logSeverity;
            LogContent = logContent;
        }

        public Log(Severity logSeverity, string logContent, Exception exception) {
            LogSeverity = logSeverity;
            LogContent = logContent;
            StoredException = exception;
        }
    }
}
