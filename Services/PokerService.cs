using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PokerBot.Models;
using PokerBot.Models.Logs;
using PokerBot.Models;
using PokerBot.Utilities;


namespace PokerBot.Services
{
    public class PokerService
    {
        public enum States
        {
            Closed,
            Pregame,
            Beginning,
            Preflop,
            BetweenFlop,
            Flop,
            AfterFlop,
            Turn,
            PreRiver,
            River,
            Showdown
        };

        private LogService _logService;
        private SqlService _sqlService;
        
        private States _gameState;
        private List<PokerPlayer> _playerList;
        private List<PokerPlayer> _foldedPlayers;
        private Deck _deck;
        private Card[] _river;
        private int _pot = 0;
        private int _dealer;
        private int _currentPlayer;
        private int _playerToMatch;
        private int _call;
        

        public PokerService(LogService logService, SqlService sqlService)
        {
            _logService = logService;
            _gameState = States.Closed;
            _playerList = new List<PokerPlayer>();
            _foldedPlayers = new List<PokerPlayer>();
            _river = new Card[5];
            _sqlService = sqlService;
        }

        public void Test()
        {
            Console.WriteLine("Service test good");
        }

        #region PreGame

        public async Task NewGame(SocketCommandContext context) 
        {
            if (_gameState == States.Pregame)
            {
                await context.Channel.SendMessageAsync(
                    $"Currently in pregame. Try joining with \"{Environment.GetEnvironmentVariable("COMMAND_PREFIX")}joingame\" or starting with \"{Environment.GetEnvironmentVariable("COMMAND_PREFIX")}start\"!");
                _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Warning, "Player attempted to start new game in pregame"));
                return;
            }

            if (_gameState > States.Pregame)
            {
                await context.Channel.SendMessageAsync(
                    "A game is currently in progress. Wait for it to finish before attempting to start a new one.");
                _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Warning, "Player attempted to start new game while game is ongoing"));
                return;
            }

            try
            {
                var user = (IGuildUser) context.User;
            var player = await _sqlService.GetPlayerAsync(user.Id);
            if (player == null)
            {
                player = new PokerPlayer(user.Id, user.Nickname);
                player.GiveMoney(100);
                await _sqlService.AddPlayerAsync(player);
            }
            _playerList.Add(player);
            _gameState = States.Pregame;
                await context.Channel.SendMessageAsync(
                    $"New game started! New players can join with \"{Environment.GetEnvironmentVariable("COMMAND_PREFIX")}joingame\" and the game can be started with \"{Environment.GetEnvironmentVariable("COMMAND_PREFIX")}start\"!");
                _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Info, $"Player '{user.Nickname}' has started a new game"));
            }
            catch (Exception ex)
            {
                _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Error, "Attempted new game creation", ex));
            }
        }

        public async Task ClosePregame(SocketCommandContext context)
        {
            if (_gameState > States.Pregame)
            {
                await context.Channel.SendMessageAsync("Game is currently in progress.");
                _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Warning, "User attempted to close currently running game"));
                return;
            }
            
            if (_gameState == States.Closed)
            {
                await context.Channel.SendMessageAsync(
                    $"A game lobby hasn't been opened. Use \"{Environment.GetEnvironmentVariable("COMMAND_PREFIX")}newgame\" to start one!");
                _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Warning, "User attempted to close game with no open game"));
                return;
            }
            
            _playerList = new List<PokerPlayer>();
            _gameState = States.Closed;
            await context.Channel.SendMessageAsync("Pregame lobby closed.");
            _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Info, "Pregame has been closed successfully"));
        }

        public async Task JoinGame(SocketCommandContext context)
        {
            if (_gameState != States.Pregame)
            {
                await context.Channel.SendMessageAsync("Not in pregame.");
                _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Warning, "User attempted to join currently running game"));
                return;
            }
            
            var user = (IGuildUser) context.User;
            var player = await _sqlService.GetPlayerAsync(user.Id);
            if (player == null) 
            {
                player = new PokerPlayer(user.Id, user.Nickname);
                player.GiveMoney(100);
                await _sqlService.AddPlayerAsync(player);
            }

            foreach (var p in _playerList)
            {
                if (p.Equals(player))
                {
                    await context.Channel.SendMessageAsync("You're already in the game!");
                    return;
                }
            }

            _playerList.Add(player);
            await context.Channel.SendMessageAsync($"Welcome to the game {player.GetName()}!");
            _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Info, $"User '{player.GetName()}' has joined the lobby"));
        }

        public async Task LeaveGame(SocketCommandContext context)
        {
            //TODO: Make more efficient
            foreach (var p in _playerList)
            {
                if (p.Equals(context.User.Id))
                {
                    _playerList.Remove(p);
                    _foldedPlayers.Remove(p);
                    await context.Channel.SendMessageAsync("You have successfully left the game.");
                    _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Info, $"User '{p.GetName()}' left the lobby"));
                    if (_playerList.Count == 0)
                    {
                        await ClosePregame(context); 
                    }
                    return;
                }
            }

            await context.Channel.SendMessageAsync("You are not in the game.");
        }

        public async Task ListPlayers(SocketCommandContext context)
        {
            if (_gameState == States.Closed)
            {
                await context.Channel.SendMessageAsync("No game has been started.");
                _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Warning, "Attempted player list while game is closed"));
                return;
            }

            try
            {
                var embed = new EmbedBuilder() {Title = "Player List:"};
                foreach (var p in _playerList)
                {
                    embed.Description += p.GetName() + "\n";
                }

                embed.Description += "Total: " + _playerList.Count;

                await context.Channel.SendMessageAsync("", false, embed.Build());
                _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Info, "Current players listed"));
            }
            catch (Exception ex)
            {
                _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Error, "Exception in ListPlayers method", ex));
            }
        }

        public async Task StartGame(SocketCommandContext context)
        {
            if (_gameState > States.Pregame)
            {
                await context.Channel.SendMessageAsync("Game is currently in progress.");
                _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Warning, "Attempted game start while game is running"));
                return;
            }

            if (_gameState == States.Closed)
            {
                await context.Channel.SendMessageAsync(
                    $"No pregame open. Try \"{Environment.GetEnvironmentVariable("COMMAND_PREFIX")}newgame\"");
                _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Warning, "Attempted game start with no pregame"));
                return;
            }

            try
            {
                await context.Channel.SendMessageAsync("Starting game...");

                _gameState = States.Beginning;
                _deck = new Deck();
                foreach (var player in _playerList)
                {
                    player.GiveMoney(100); //TODO: Change to env variable
                }

                await DealHands(context.Message);
                _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Info, "New game started"));
            }
            catch (Exception ex)
            {
                _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Error, "Unable to start game", ex));
            }
        }

        public async Task EndGame(SocketCommandContext context)
        {
            if (_gameState == States.Pregame)
            {
                await context.Channel.SendMessageAsync($"Still in pregame. Use \"{Environment.GetEnvironmentVariable("COMMAND_PREFIX")}close\" if you want to close the pregame lobby.");
                _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Warning, "Attempted game end while in pregame"));
                return;
            }

            if (_gameState == States.Closed)
            {
                await context.Channel.SendMessageAsync("No game is in progress.");
                return;
            }

            _gameState = States.Closed;
            _playerList = new List<PokerPlayer>();
            _pot = 0;
            _dealer = 0;
            await context.Channel.SendMessageAsync("Game has ended.");
            _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Info, "Game has been ended"));
        }
        #endregion

        #region Ingame
            #region Helpers

        private async Task DealHands(SocketMessage message)
        {
            try
            {
                _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Info, "Shuffling deck..."));
                _deck.Shuffle();

                _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Info, "Dealing cards..."));
                foreach (var p in _playerList)
                {
                    p.GiveHand(_deck.DrawCards(2));
                    p.SendDM("Your hand: " + p.GetHand());
                }

                _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Info, "Hands dealt."));
            }
            catch (Exception ex)
            {
                _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Error, "Exception while dealing hands", ex));
            }

            await StartRound(message);
        }

        private void IncrementPlayer()
        {
            if (_currentPlayer == _playerList.Count - 1)
            {
                _currentPlayer = 0;
            }
            else
            {
                _currentPlayer++;
            }

            if (_foldedPlayers.Contains(_playerList[_currentPlayer]))
            {
                IncrementPlayer();
            }
        }

        private void IncrementDealer()
        {
            if (_dealer == _playerList.Count)
            {
                _dealer = 0;
            }
            else
            {
                _dealer++;
            }
        }

        private bool CheckForEnd()
        {
            if (_foldedPlayers.Count == 0)
            {
                if (_currentPlayer == _playerList.Count - 1)
                {
                    return _playerToMatch == 0;
                }
                
                return _playerToMatch == _currentPlayer + 1;
            }
            
            var nextPlayer = _currentPlayer;
            if (nextPlayer == _playerList.Count - 1)
            {
                nextPlayer = 0;
            }
            else
            {
                nextPlayer++;
            }
            
            while (_foldedPlayers.Contains(_playerList[nextPlayer]))
            {
                if (nextPlayer == _playerList.Count - 1)
                {
                    nextPlayer = 0;
                }
                else
                {
                    nextPlayer++;
                }
            }

            return nextPlayer == _playerToMatch;
        }
            #endregion

            #region Gameplay

        private async Task StartRound(SocketMessage message)
        {
            _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Info, "Round starting..."));
            try
            {
                switch (_gameState)
                {
                    case States.Beginning:
                        var smallBlind = 0;
                        var bigBlind = 0;
                        if (_dealer == _playerList.Count - 2)
                        {
                            smallBlind = _dealer + 1;
                            bigBlind = 0;
                        }
                        else if (_dealer == _playerList.Count - 1)
                        {
                            smallBlind = 0;
                            bigBlind = 1;
                        }
                        else
                        {
                            smallBlind = _dealer + 1;
                            bigBlind = smallBlind + 1;
                        }

                        if (bigBlind == _playerList.Count - 1)
                        {
                            _currentPlayer = 0;
                        }
                        else
                        {
                            _currentPlayer = bigBlind + 1;
                        }

                        _playerList[smallBlind].TakeMoney(5);
                        _playerList[bigBlind].Call(10);
                        _pot += 15;
                        _call = 10;
                        _playerToMatch = bigBlind;
                        await message.Channel.SendMessageAsync(
                            $"Small blind of 5 posted by {_playerList[smallBlind].GetName()}, big blind of 10 posted by {_playerList[bigBlind].GetName()}.");
                        _gameState++;
                        await StartRound(message);
                        return;
                    case States.Preflop:
                        await message.Channel.SendMessageAsync(
                            $"{_playerList[_currentPlayer].GetName()}, would you like to **Call** the {_call - _playerList[_currentPlayer].GetTotalCall()} money, **Raise** by an *amount*, or **Fold**?");
                        Program.AddMessageEvent(PlayRound);
                        return;
                    case States.BetweenFlop:
                        if (_dealer == _playerList.Count - 1)
                        {
                            _currentPlayer = 0;
                        }
                        else
                        {
                            _currentPlayer = _dealer + 1;
                        }

                        _playerToMatch = _currentPlayer;
                        _call = 0;

                        var flopCards = "";
                        _deck.DrawCard();
                        for (var i = 0; i < 3; i++)
                        {
                            _river[i] = _deck.DrawCard();
                            flopCards += _river[i] + "   ";
                        }

                        await message.Channel.SendMessageAsync(flopCards);
                        _gameState++;
                        await StartRound(message);
                        return;
                    case States.Flop:
                        if (_call == 0)
                        {
                            await message.Channel.SendMessageAsync(
                                $"{_playerList[_currentPlayer].GetName()}, would you like to **Check**, **Raise** by an *amount*, or **Fold**?");
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync(
                                $"{_playerList[_currentPlayer].GetName()}, would you like to **Call** the {_call - _playerList[_currentPlayer].GetTotalCall()} money, **Raise** by an *amount*, or **Fold**?");
                        }

                        Program.AddMessageEvent(PlayRound);
                        return;
                    case States.AfterFlop:
                        if (_dealer == _playerList.Count - 1)
                        {
                            _currentPlayer = 0;
                        }
                        else
                        {
                            _currentPlayer = _dealer + 1;
                        }

                        _playerToMatch = _currentPlayer;
                        _call = 0;

                        var turnCards = "";
                        for (var i = 0; i < 3; i++)
                        {
                            turnCards += _river[i] + "   ";
                        }

                        _deck.DrawCard();
                        _river[3] = _deck.DrawCard();
                        turnCards += _river[3];

                        await message.Channel.SendMessageAsync(turnCards);
                        _gameState++;
                        await StartRound(message);
                        return;
                    case States.Turn:
                        if (_call == 0)
                        {
                            await message.Channel.SendMessageAsync(
                                $"{_playerList[_currentPlayer].GetName()}, would you like to **Check**, **Raise** by an *amount*, or **Fold**?");
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync(
                                $"{_playerList[_currentPlayer].GetName()}, would you like to **Call** the {_call - _playerList[_currentPlayer].GetTotalCall()} money, **Raise** by an *amount*, or **Fold**?");
                        }

                        Program.AddMessageEvent(PlayRound);
                        return;
                    case States.PreRiver:
                        if (_dealer == _playerList.Count - 1)
                        {
                            _currentPlayer = 0;
                        }
                        else
                        {
                            _currentPlayer = _dealer + 1;
                        }

                        _playerToMatch = _currentPlayer;
                        _call = 0;

                        var riverCards = "";
                        for (var i = 0; i < 4; i++)
                        {
                            riverCards += _river[i] + "   ";
                        }

                        _deck.DrawCard();
                        _river[4] = _deck.DrawCard();
                        riverCards += _river[4];

                        await message.Channel.SendMessageAsync(riverCards);
                        _gameState++;
                        await StartRound(message);
                        return;
                    case States.River:
                        if (_call == 0)
                        {
                            await message.Channel.SendMessageAsync(
                                $"{_playerList[_currentPlayer].GetName()}, would you like to **Check**, **Raise** by an *amount*, or **Fold**?");
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync(
                                $"{_playerList[_currentPlayer].GetName()}, would you like to **Call** the {_call - _playerList[_currentPlayer].GetTotalCall()} money, **Raise** by an *amount*, or **Fold**?");
                        }

                        Program.AddMessageEvent(PlayRound);
                        return;
                    case States.Showdown:
                        
                        var hands = new List<List<Hand>>();     // All possible hands for each player
                        
                        // Tasks finding possible hands
                        var handTasks = new List<Task>();
                        foreach (var player in _playerList)
                        {
                            var cards = new List<Card>(_river);
                            cards.AddRange(player.GetHand().GetCards());
                            handTasks.Add(Task.Run(() => hands.Add(Combinations.FindCombinations(cards)))); // Scores are calculated in constructor
                        }

                        // Wait for all hands to be created and scored
                        Task.WaitAll(handTasks.ToArray());

                        // Find best hand for each player
                        var bestHands = new List<Hand>();
                        foreach (var pHands in hands)
                        {
                            pHands.Sort();
                            bestHands.Add(pHands.Last());
                        }

                        // Find winner or winners
                        var winners = new List<int> { 0 };
                        var winnerHands = new List<string>();
                        for (var i = 1; i < bestHands.Count; i++)
                        {
                            var comp = bestHands[i].CompareTo(bestHands[winners[0]]);
                            switch (comp)
                            {
                                case > 0:
                                    winners = new List<int> { i };
                                    winnerHands = new List<string> { bestHands[i].GetHandName() };
                                    break;
                                case 0:
                                    winners.Add(i);
                                    winnerHands.Add(bestHands[i].GetHandName());
                                    break;
                            }
                        }

                        // Give winners the money, send winning message
                        await WinPot(message, winners, winnerHands);
                        
                        return;
                }
            }
            catch (Exception ex)
            {
                _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Error, "Exception during preround", ex));
            }
        }

        private async Task PlayRound(SocketMessage message)
        {
            if (message.Author.Id != _playerList[_currentPlayer].GetId())
            {
                _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Warning, $"User '{message.Author.Username}' attempted play out of turn"));
                return;
            }

            try
            {
                var command = message.Content.ToLower().Split(' ');
                switch (command[0])
                {
                    case "fold":
                        if (CheckForEnd())
                        {
                            _foldedPlayers.Add(_playerList[_currentPlayer]);
                            if (_foldedPlayers.Count == _playerList.Count - 1)
                            {
                                await message.Channel.SendMessageAsync(
                                    _playerList[_currentPlayer].GetName() + " folds.");
                                Program.RemoveMessageEvent(PlayRound);
                                var winner = 0;
                                for (var i = 0; i < _playerList.Count; i++)
                                {
                                    if (_foldedPlayers.Contains(_playerList[i]))
                                    {
                                        continue;
                                    }

                                    winner = i;
                                }

                                await WinPot(message, new List<int>{ winner }, new List<string> { "folding" });
                                return;
                            }

                            await message.Channel.SendMessageAsync(_playerList[_currentPlayer].GetName() +
                                                                   " folds. Onto the next stage...");
                            _gameState++;
                            foreach (var p in _playerList)
                            {
                                p.ResetCall();
                            }
                        }
                        else
                        {
                            _foldedPlayers.Add(_playerList[_currentPlayer]);
                            await message.Channel.SendMessageAsync(_playerList[_currentPlayer].GetName() + " folds.");
                            IncrementPlayer();
                            if (_foldedPlayers.Count == _playerList.Count - 1)
                            {
                                Program.RemoveMessageEvent(PlayRound);
                                var winner = 0;
                                for (var i = 0; i < _playerList.Count; i++)
                                {
                                    if (_foldedPlayers.Contains(_playerList[i]))
                                    {
                                        continue;
                                    }

                                    winner = i;
                                }

                                await WinPot(message, new List<int> {winner}, new List<string> { "folding" });
                                return;
                            }
                        }

                        break;
                    case "call":
                        if (_call == 0)
                        {
                            await message.Channel.SendMessageAsync("Nothing to call.");
                            return;
                        }

                        var toCall = _call - _playerList[_currentPlayer].GetTotalCall();
                        _pot += toCall;
                        _playerList[_currentPlayer].Call(toCall);
                        if (CheckForEnd())
                        {
                            await message.Channel.SendMessageAsync(_playerList[_currentPlayer].GetName() +
                                                                   " calls. Onto the next stage...");
                            _gameState++;
                            foreach (var p in _playerList)
                            {
                                p.ResetCall();
                            }
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync(_playerList[_currentPlayer].GetName() + " calls.");
                            IncrementPlayer();
                        }

                        break;
                    case "raise":
                        if (command.Length < 2 || !int.TryParse(command[1], out var raise))
                        {
                            await message.Channel.SendMessageAsync("Invalid raise amount. Must be a positive integer.");
                            return;
                        }

                        _call += raise;
                        _playerToMatch = _currentPlayer;
                        await message.Channel.SendMessageAsync(
                            _playerList[_currentPlayer].GetName() + " raises by " + raise);
                        IncrementPlayer();
                        break;
                    case "check":
                        if (_call > 0)
                        {
                            await message.Channel.SendMessageAsync("Cannot check. Would you like to **Call** the " +
                                                                   (_call - _playerList[_currentPlayer]
                                                                       .GetTotalCall()) + "money?");
                            return;
                        }

                        if (CheckForEnd())
                        {
                            await message.Channel.SendMessageAsync("All players checked. Onto the next stage...");
                            _gameState++;
                        }
                        else
                        {
                            IncrementPlayer();
                        }

                        break;
                    default:
                        return;
                }

                Program.RemoveMessageEvent(PlayRound);
                await StartRound(message);
            }
            catch (Exception ex)
            {
                _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Error, "Exception during round", ex));
            }
        }

        private async Task WinPot(SocketMessage message, List<int> players, List<string> hands)
        {
            try
            {
                if (players.Count == 1)
                {
                    _playerList[players[0]].GiveMoney(_pot);
                    await message.Channel.SendMessageAsync(
                        $"{_playerList[players[0]].GetName()} won the pot of {_pot} with {hands[0]}!");
                        _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Info,
                    $"Player '{_playerList[players[0]].GetName()}' won a pot of {_pot}"));
                }
                else
                {
                    var splitPot = _pot / players.Count;
                    var winners = "";
                    foreach (var p in players)
                    {
                        _playerList[p].GiveMoney(splitPot);
                        winners += $"{_playerList[p].GetName()}, ";
                    }

                    await message.Channel.SendMessageAsync(
                        $"{winners.Substring(0, winners.Length - 1)} have tied and split the pot, getting {splitPot} each!\n" +
                        $"They had {hands[0]} and {hands[1]}");
                        _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Info,
                    $"Players '{_playerList[players[0]].GetName()}' and '{_playerList[players[1]].GetName()}' split a pot of {splitPot} each"));
                }

                for (var i = 0; i < _playerList.Count; i++)
                {
                    if (players.Contains(i))
                    {
                        _playerList[i].AddWin();
                    }
                    else
                    {
                        _playerList[i].AddLoss();
                    }
                }

                await _sqlService.UpdatePlayersAsync(_playerList);

                _pot = 0;
                _currentPlayer = 0;
                _gameState = States.Beginning;
                _river = new Card[5];
                _foldedPlayers = new List<PokerPlayer>();
                _deck = new Deck();
                foreach (var p in _playerList)
                {
                    p.ClearHand();
                }

                IncrementDealer();
                await DealHands(message);
            }
            catch (Exception ex)
            {
                _logService.WriteLog(new PokerLog(_gameState, LogObject.Severity.Error, $"Exception when allotting pot of {_pot} to player '{_playerList[players[0]].GetName()}'", ex));
            }
        }
            #endregion
        #endregion
    }
}
