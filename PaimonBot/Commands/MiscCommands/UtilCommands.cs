using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using PaimonBot.Services;
using PaimonBot.Services.HelpFormatter;
using System;
using System.Threading.Tasks;

namespace PaimonBot.Commands
{
    public class UtilCommands : BaseCommandModule
    {
        [Command("ping"), Description("Checks your ping")]
        [Cooldown(1, 3, CooldownBucketType.Channel)]
        [Category(CategoryName.Misc)]
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

        [Command("uptime"), Description("How long PaimonBot's been running!")]
        [Cooldown(1, 3, CooldownBucketType.Channel)]
        [Category(CategoryName.Misc)]
        public async Task uptime(CommandContext ctx)
        {
            var timespan = DateTime.Now - SharedData.startTime;
            string desc = $"**{timespan.Days}** days, **{timespan.Hours}** hours, **{timespan.Minutes}** minutes, and **{timespan.Seconds}** seconds.";
            var embed = new DiscordEmbedBuilder()
                .WithTitle("Uptime " + Emojis.HappyEmote)
                .WithDescription(desc)
                .WithColor(SharedData.defaultColour)
                .WithFooter($"Requested by {ctx.User.Username}")
                .WithTimestamp(DateTime.Now);
            await ctx.RespondAsync(embed).ConfigureAwait(false);
        }

        [Command("info"), Description("Returns back information on PaimonBot and the bot developer.")]
        [Cooldown(1, 5, CooldownBucketType.Channel)]
        [Category(CategoryName.Misc)]
        public async Task info(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
                .WithAuthor($"{SharedData.botName} Information")
                .WithColor(SharedData.defaultColour)
                .WithTimestamp(DateTime.Now)
                .WithDescription($"Here's a little bit of information! If you need help with commands, use `{SharedData.prefixes[0]}help`");

            embed.AddField("Current Guild", Formatter.BlockCode($"<RequestorID: {ctx.User.Id}>\n<ChannelID: {ctx.Channel.Id}>\n<GuildID: {ctx.Guild.Id}>", "xml"));
            embed.AddField("Bot Information", Formatter.BlockCode($"<Bot-UserID: {ctx.Client.CurrentUser.Id}>\n<ShardID: {ctx.Client.ShardId}>\n" +
                $"<Ping: {ctx.Client.Ping} ms>\n<Uptime: {(DateTime.Now - SharedData.startTime).TotalMinutes} minutes>\n<CreationTimestamp: {ctx.Client.CurrentUser.CreationTimestamp:D}>", "xml"));
            var dev = await ctx.Client.GetUserAsync(574558925224017920).ConfigureAwait(false);
            string desc = "Thank you for using my bot! This was a fun project to work on, please check out my github page for my portfolio. https://github.com/ybmirz";
            embed.AddField("Developer Information", Formatter.BlockCode($"<Username: {dev.Username}#{dev.Discriminator}>\n<UserID: {dev.Id}>\n<Note: {desc}>", "xml"));

            await ctx.RespondAsync(embed.Build()).ConfigureAwait(false);
        }

        [Command("choose"), Aliases("pick")]
        [Description("Lets Paimon choose between options as the string arguments, seperated using spaces.")]
        [Cooldown(1, 3, CooldownBucketType.User)]
        [Category(CategoryName.Misc)]
        public async Task choose(CommandContext ctx, [RemainingText] string options)
        {
            var inputs = options.Split(" ");
            await ctx.RespondAsync($"Paimon chose **{inputs[SharedData.Random.Next(0, inputs.Length)]}**.").ConfigureAwait(false);
        }
    }
}