using System;
using PokerBot.Models.Logs;

namespace PokerBot.Services
{
    public class LogService
    {
        public void WriteLog(LogObject log)
        {
            Console.WriteLine(GetTimestamp() + $" - {log.LogSeverity} - {log.LogContent}");
            
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