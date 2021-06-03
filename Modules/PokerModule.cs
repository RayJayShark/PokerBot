using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PokerBot.Classes;
using PokerBot.Services;

namespace PokerBot.Modules
{
    [Name("Poker Commands")]
    public class PokerModule : ModuleBase<SocketCommandContext>
    {
        private PokerService _pokerService;
        private SqlService _sqlService;

        public PokerModule(PokerService pokerService, SqlService sqlService)
        {
            _pokerService = pokerService;
            _sqlService = sqlService;
        }

        [Command("newgame")]
        [Alias("newg")]
        public async Task StartNewGame()
            => await _pokerService.NewGame(Context);

        [Command("close")]
        [Description("Closes the pregame lobby.")]
        public async Task ClosePregame()
            => await _pokerService.ClosePregame(Context);

        [Command("joingame")]
        [Alias("join")]
        public async Task JoinGame()
            => await _pokerService.JoinGame(Context);

        [Command("leavegame")]
        [Alias("leave")]
        public async Task LeaveGame()
            => await _pokerService.LeaveGame(Context);

        [Command("playerlist")]
        [Alias("plist")]
        public async Task ListPlayers()
            => await _pokerService.ListPlayers(Context);

        [Command("start")]
        [Alias("begin", "startgame")]
        public async Task StartGame()
            => await _pokerService.StartGame(Context);

        [Command("end")]
        [Alias("stop")]
        public async Task EndGame()
            => await _pokerService.EndGame(Context);

        [Command("stats")]
        [Alias("stat")]
        public async Task GetStats(string playerName = null)
        {
            PokerPlayer player;
            if (string.IsNullOrEmpty(playerName))
            {
                player = await _sqlService.GetPlayerAsync(Context.User.Id);
            }
            else
            {
                player = await _sqlService.GetPlayerAsync(playerName);
            }

            if (player == null)
            {
                await ReplyAsync("This player does not exist, so they doesn't have stats. :/");
                return;
            }

            var embedBuilder = new EmbedBuilder
            {
                Color = Color.Magenta,
                Title = player.GetName() + "'s Stats",
                Description = $"*Money:* {player.GetMoney()}\n" +
                              $"*Wins:* {player.GetWins()}\n" +
                              $"*Losses:* {player.GetLosses()}",
                Footer = new EmbedFooterBuilder {Text = $"Total games: {player.GetWins() + player.GetLosses()}"}
            };

            await ReplyAsync("", embed: embedBuilder.Build());
        }

        [Command("ptest")]
        public async Task Test()
        {
            Console.WriteLine("Starting test...");
            try
            {
                _pokerService.Test();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        
    }
}