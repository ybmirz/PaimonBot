using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using PaimonBot.Services.CurrencyHelper;
using PaimonBot.Services.ResinHelper;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaimonBot.Services
{
    /// <summary>
    /// A class with methods and services to help Paimon function
    /// </summary>
    public static class PaimonServices
    {
        /// <summary>
        /// Default Embed sending to a Channel Method
        /// </summary>
        /// <param name="channel">DiscordChannel to send the Embed to</param>
        /// <param name="title">Title of the Embed</param>
        /// <param name="desc">Description in the Embed</param>
        /// <param name="delay">Delay to wait before deleting Embed [If set to Zero, will not delete Embed]</param>
        /// <param name="error">Emoji and Specified colors for the Embed [Defaults to Default]</param>
        /// <returns>Completed Task</returns>
        public static async Task SendEmbedToChannelAsync(DiscordChannel channel,
            string title, string desc,
            TimeSpan delay,ResponseType type = ResponseType.Default)
        {
            var titleEmote = type switch
            {
                ResponseType.Warning => "❗",
                ResponseType.Error => "⛔",
                ResponseType.Missing => "🔎",
                ResponseType.Default => "💡",
                _ => "💡"
            };
            var ErrorColour = type switch
            {
                ResponseType.Default =>SharedData.defaultColour,
                ResponseType.Warning => new DiscordColor("#ffcc00"), //orange-ish warning colour
                ResponseType.Error => new DiscordColor("#cc3300"), //red error colour
                ResponseType.Missing => new DiscordColor("#999999"), //gray missing colour
                _ => SharedData.defaultColour
            };


            var embed = CreateEmbed(title + " " + titleEmote,desc, ErrorColour);
            embed.WithFooter($"{SharedData.prefixes[0]}help for more Info", SharedData.logoURL)
                .WithTimestamp(DateTime.Now);

            var msg = await channel.SendMessageAsync(embed: embed)
                .ConfigureAwait(false);
            if (delay != TimeSpan.Zero)
            { await Task.Delay(delay); await msg.DeleteAsync().ConfigureAwait(false); }
        }

        /// <summary>
        /// Context Respond Async and delete after Delay
        /// </summary>
        /// <param name="ctx">CommandContext to respond to</param>
        /// <param name="message">Message to send as response</param>
        /// <param name="delay">Delay to wait before deleting the message [Zero will not delete the message]]</param>
        /// <returns></returns>
        public static async Task SendRespondAsync(CommandContext ctx,
            string message, TimeSpan delay)
        {
            var msg = await ctx.RespondAsync(message)
                .ConfigureAwait(false);
            if (delay != TimeSpan.Zero)
            {
                await Task.Delay(delay);
                await msg.DeleteAsync().ConfigureAwait(false);
            }
        }
        /// <summary>
        /// Default EmbedBuilder with DefaultColor
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static DiscordEmbedBuilder CreateEmbed(string title, string description, DiscordColor color = default)
        {
            color = SharedData.defaultColour;
            return new DiscordEmbedBuilder().WithTitle(title).WithDescription(description).WithColor(color);
        }

        /// <summary>
        /// EmbedBuilder with Member as Author [Default Colour as well]
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="Title"></param>
        /// <param name="Description"></param>
        /// <returns></returns>
        public static DiscordEmbedBuilder CreateEmbed(CommandContext ctx, string Title, string Description)
        {
            return new DiscordEmbedBuilder()
                .WithAuthor(ctx.Member.DisplayName,iconUrl: ctx.Member.AvatarUrl)                
                .WithColor(SharedData.defaultColour)
                .WithTitle(Title)
                .WithDescription(Description);
        }

        /// <summary>
        /// EmbedBuild with Member as Author [Custom Colour]
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static DiscordEmbedBuilder CreateEmbed(
            CommandContext ctx, string title, string description,
            DiscordColor color)
        {
            return new DiscordEmbedBuilder()
                .WithAuthor(ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl)
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(color);     
        }

        /// <summary>
        /// EmbedBuild with Member as Author, Custom Colour, Timestamp and FooterValues
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="description"></param>
        /// <param name="color"></param>
        /// <param name="dateTime"></param>
        /// <param name="footerText"></param>
        /// <param name="footerUrl"></param>
        /// <returns></returns>
        public static DiscordEmbedBuilder CreateEmbed(CommandContext ctx, string description, DiscordColor color, DateTime dateTime, string footerText, string footerUrl)
        {            
            return new DiscordEmbedBuilder()
                .WithAuthor(ctx.User.Username, iconUrl: ctx.User.AvatarUrl)
                .WithDescription(description)
                .WithColor(color)
                .WithTimestamp(dateTime)
                .WithFooter(footerText, footerUrl);
        }

        /// <summary>
        /// When resin caps, disable and dispose the timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void ATimer_ResinCapped(object sender, ResinCappedEventArgs e)
        {
            Log.Information("Resin for User {Id} has capped. Disposing timer...", e.DiscordId);
            e.resinTimer.Stop();
            e.resinTimer.Dispose();
            SharedData.resinTimers.Remove(SharedData.resinTimers.Find(x => x._discordID == e.DiscordId));
            Log.Information("Resin for User {Id} has capped. Timer Disposed.", e.DiscordId);
        }

        /// <summary>
        /// When Currency caps, disable and dispose the timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void ATimer_CurrencyCapped(object sender, CurrencyCappedEventArgs e)
        {
            Log.Information("Currency for User {Id} has capped. Disposing timer...", e.DiscordId);
            e.currencyTimer.Stop();
            e.currencyTimer.Dispose();
            SharedData.currencyTimer.Remove(SharedData.currencyTimer.Find(x => x.DiscordId == e.DiscordId));
            Log.Information("Currency for User {Id} has capped. Timer Disposed.", e.DiscordId);
        }
        
    }    
}
