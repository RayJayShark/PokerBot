using System;
using PokerBot.Models.Logs;

namespace PokerBot.Services
{
    public class LogService
    {
        private readonly LogObject.Severity _logLevel;

        public LogService(LogObject.Severity logLevel)
        {
            _logLevel = logLevel;
        }
        
        public void WriteLog(LogObject log)
        {
            if (log.LogSeverity > _logLevel) return;
            Console.WriteLine(GetTimestamp() + $" - {log.LogSeverity} - {log}");

            if (log.StoredException != null)
            {
                Console.WriteLine("\t" + log.StoredException.StackTrace);
            }
        }
    
        private static string GetTimestamp()
        {
            var currentTime = DateTime.Now;
            return $"{currentTime.Year}-{currentTime.Month:D2}-{currentTime.Day:D2} {currentTime.Hour:D2}:{currentTime.Minute:D2}:{currentTime.Second:D2}";
        }
    }
}