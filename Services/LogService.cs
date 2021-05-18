using System;

namespace PokerBot.Services
{
    public class LogService
    {
        public void GameLog(string log)
        {
            Console.WriteLine(GetTimestamp() + " Game\t     " + log);
        }
    
        private static string GetTimestamp()
        {
            var currentTime = DateTime.Now;
            return $"{currentTime.Year}-{currentTime.Month:D2}-{currentTime.Day:D2} {currentTime.Hour:D2}:{currentTime.Minute:D2}:{currentTime.Second:D2}";
        }
    }
}