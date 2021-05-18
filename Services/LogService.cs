using System;

namespace PokerBot.Services
{
    public static class LogService
    {
        public static void GameLog(string log)
        {
            Console.WriteLine(GetTimestamp() + " Game\t     " + log);
        }
    
        private static string GetTimestamp()
        {
            var currentTime = DateTime.Now;
            return $"{currentTime.Hour:D2}:{currentTime.Minute:D2}:{currentTime.Second:D2}";
        }
    }
}