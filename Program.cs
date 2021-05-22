using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using dotenv.net;
using PokerBot.Services;
using Microsoft.Extensions.DependencyInjection;
using PokerBot.Models;
using PokerBot.Models.Logs;


namespace PokerBot
{
    class Program
    {
        private static DiscordSocketClient _client;
        private static CommandService _commands;
        private static LogService _logService;
        private static IServiceProvider _services;

        private static void Main(string[] arg) => new Program().MainAsync().GetAwaiter().GetResult();
        private async Task MainAsync()
        {
            if (!File.Exists(".env"))
            {
                File.Copy(".env.example", ".env");
                Console.WriteLine(".env file created. Please configure and restart.");
                return;
            }
            DotEnv.Config(false);

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 100
            });

            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_TOKEN"));
            await _client.StartAsync();

            _logService = new LogService();
            
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(new PokerService(_logService))
                .AddSingleton(_logService)
                .BuildServiceProvider();

            _commands = new CommandService();
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            
            
            _client.MessageUpdated += MessageUpdated;
            _client.MessageReceived += HandleCommandAsync;

            await Task.Delay(-1);
        }
        
        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
        
        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            var argPos = 0;

            var prefix = (Environment.GetEnvironmentVariable("COMMAND_PREFIX") ?? "+")[0];

            if (!(message.HasCharPrefix(prefix, ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)) || message.Author.IsBot)
                return;

            var context = new SocketCommandContext(_client, message);

            var result = await _commands.ExecuteAsync(context: context, argPos: argPos, services: _services);
            
            // Log command
            _logService.WriteLog(new CommandLog(
                message.Author,
                message.Content.Substring(0, message.Content.IndexOf(' ')),
                result.IsSuccess ? LogObject.Severity.Info : LogObject.Severity.Warning,
                message.Content
            ));
        }

        private static async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after,
            ISocketMessageChannel channel)
        {
            var message = await before.GetOrDownloadAsync();
        }

        public static SocketGuild GetGuild(ulong id)
        {
            return _client.GetGuild(id);
        }

        public static void AddMessageEvent(Func<SocketMessage, Task> task)
        {
            _client.MessageReceived += task;
        }

        public static void RemoveMessageEvent(Func<SocketMessage, Task> task)
        {
            _client.MessageReceived -= task;
        }
        
        public static async Task Shutdown()
        {
            await _client.StopAsync();
            Environment.Exit(0);
        }
    }
}