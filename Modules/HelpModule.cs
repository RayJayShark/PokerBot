using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace PokerBot.Modules
{
    //TODO: Check for empty module (currently gives null reference exception)
    [Name("Help Commands")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;

        public HelpModule(CommandService service)
        {
            _service = service;
        }
        
        [Command("help")]
        [Summary("Lists all commands powered by this bot.")] 
        [Alias("list")]
        public async Task ListCommands()
        {
            if ((_service.Commands).Any())
            {
                var builder = new EmbedBuilder {Title = "Currently loaded commands:"};
                foreach (var module in _service.Modules)
                {
                    if (module.Remarks != null && module.Remarks.Contains("admin") && Context.User.Id != (await Context.Client.GetApplicationInfoAsync((RequestOptions) null).ConfigureAwait(false)).Owner.Id)
                    {
                        continue;
                    }

                    var field = new EmbedFieldBuilder {Name = module.Name + ":"};
                    foreach (var command in module.Commands)
                    {
                        field.Value += $"**{command.Name}** {command.Remarks ?? ""}: {command.Summary ?? "No description provided."}\n";
                        field.Value +=
                            $"Aliases: {string.Join(", ", command.Aliases.Where(x => x != command.Name).ToArray())}\n";
                    }

                    builder.AddField(field);
                }
                await ReplyAsync("", false, builder.Build());
            }
            else
            {
                await ReplyAsync("No commands are currently loaded, what? This is a command 🤔.");
            }
        }
        
        [Command("help")]
        [Summary("Gives a description of the specified command.")]
        public async Task ListCommands(string command)
        {
            var found = false;
            var embed = new EmbedBuilder();
            foreach (var c in _service.Commands)
            {
                if (c.Name == command.ToLower() || c.Aliases.Contains(command.ToLower()))
                {
                    embed.Title = c.Name + " " + (c.Remarks ?? "");
                    embed.Description = (c.Summary ?? "No description provided.") + "\n";
                    embed.Description += $"Aliases: {string.Join(", ", c.Aliases.Where(x => x != c.Name).ToArray())}";
                    found = true;
                    break;
                }
            }

            if (found)
            {
                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                await ReplyAsync("There is no \"" + command + "\" command.");
            }
        }
    }
}