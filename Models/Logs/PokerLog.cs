using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PokerBot.Services;

namespace PokerBot.Models.Logs
{
    class PokerLog : LogObject
    {
        public PokerService.States GameState { get; set; }

        public PokerLog(PokerService.States gameState, Severity severity, string content)
        {
            GameState = gameState;
            LogSeverity = severity;
            LogContent = content;
        }

        public PokerLog(PokerService.States gameState ,Severity severity, string content, Exception exception)
        {
            GameState = gameState;
            LogSeverity = severity;
            LogContent = content;
            StoredException = exception;
        }

        public override string ToString()
        {
            return $"Game State: {GameState}; {LogContent}";
        }
    }
}
