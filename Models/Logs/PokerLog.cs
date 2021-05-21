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
        public List<PokerPlayer> PlayerList { get; set; }
        public Deck PokerDeck { get; set; }

        public PokerLog(List<PokerPlayer> playerList, Deck deck, Severity severity, string content)
        {
            PlayerList = playerList;
            PokerDeck = deck;
            LogSeverity = severity;
            LogContent = content;
        }

        public PokerLog(List<PokerPlayer> playerList, Deck deck, Severity severity, string content, Exception exception)
        {
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
