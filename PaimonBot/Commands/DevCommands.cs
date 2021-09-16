using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using PaimonBot.Services;
using System;
using System.Threading.Tasks;

namespace PaimonBot.Commands
{
    [Group("test")]
    [RequireOwner]
    [Description("Dev-built commands to test and develop PaimonBot")]
    [Hidden]
    public class DevCommands : BaseCommandModule
    {
        [Command("ping"), Description("Checks your ping")]                
        public async Task Ping(CommandContext ctx)
        {
            var ping = DateTime.Now - ctx.Message.CreationTimestamp;
            string desc = $"Latency is `{ping.Milliseconds}ms`\nAPI Latency is `{ctx.Client.Ping}ms`";

             var embed = new DiscordEmbedBuilder()
                 .WithColor(SharedData.defaultColour)
                 .WithTimestamp(DateTime.Now)
                 .WithTitle(":ping_pong: " + Formatter.Bold("Pong!"))
                 .WithFooter($"Requested by {ctx.User.Username}")
                 .WithDescription(desc);
                     
            await ctx.Channel.SendMessageAsync(embed.Build()).ConfigureAwait(false);

        }

        [Command("resin"), Description("Checks your resin")]
        public async Task Resin(CommandContext ctx)
        {

        }
    }
}
