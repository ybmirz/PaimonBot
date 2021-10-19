using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using PaimonBot.Services;
using PaimonBot.Services.HelpFormatter;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaimonBot.Commands
{
    public class WorldInfoCommand : BaseCommandModule
    {
        [Command("dailies")]
        [Description("Sends back embeds with items you could farm during the week!")]
        [Cooldown(1, 3, CooldownBucketType.Channel)]
        [Category(Services.CategoryName.Info)]
        public async Task Dailies(CommandContext ctx)
        {            
            List<Page> pages = new List<Page>();              
            var startDate = DateTime.UtcNow;            
            DateTime endDate = DateTime.UtcNow.AddDays(6);
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1)) 
            {
                var day = date.DayOfWeek;
                DiscordEmbedBuilder embed = null;                
                switch (day)
                {
                    case DayOfWeek.Monday:
                        embed = Monday();                        
                        break;
                    case DayOfWeek.Tuesday:
                        embed = Tuesday();
                        break;
                    case DayOfWeek.Wednesday:
                        embed = Wednesday();
                        break;
                    case DayOfWeek.Thursday:
                        embed = Thursday();
                        break;
                    case DayOfWeek.Friday:
                        embed = Friday();
                        break;
                    case DayOfWeek.Saturday:
                        embed = Saturday();
                        break;
                    case DayOfWeek.Sunday:
                        embed = Sunday();
                        break;
                }
                Page page = new Page(embed: embed);
                pages.Add(page);
            }
            await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages);
        }

        [Command("daily")]
        [Description("Sends back what items you can grind today!")]
        [Cooldown(1, 3, CooldownBucketType.Channel)]
        [Category(Services.CategoryName.Info)]
        public async Task Daily(CommandContext ctx)
        {
            DiscordEmbedBuilder embed = null;
            var day = DateTime.Now.DayOfWeek;
            switch (day)
            {
                case DayOfWeek.Monday:
                    embed = Monday();
                    break;
                case DayOfWeek.Tuesday:
                    embed = Tuesday();
                    break;
                case DayOfWeek.Wednesday:
                    embed = Wednesday();
                    break;
                case DayOfWeek.Thursday:
                    embed = Thursday();
                    break;
                case DayOfWeek.Friday:
                    embed = Friday();
                    break;
                case DayOfWeek.Saturday:
                    embed = Saturday();
                    break;
                case DayOfWeek.Sunday:
                    embed = Sunday();
                    break;
            }
            await ctx.RespondAsync(embed).ConfigureAwait(false);
        }

        #region DailyEmbeds
        private DiscordEmbedBuilder Monday()
        {
            var embed = new DiscordEmbedBuilder()
                .WithAuthor("PaimonBot", null, SharedData.logoURL)
                .WithTitle("Daily Grindable Items: Monday")
                .WithUrl("https://paimon.moe/items/")
                .WithColor(SharedData.defaultColour)
                .WithImageUrl("https://i.imgur.com/bJXMOMZ.png") // RIP I wanted to use attachment
                .WithFooter("Image and Data taken from https://paimon.moe/items/")
                .WithTimestamp(DateTime.Now);
            return embed;
        }
        private DiscordEmbedBuilder Tuesday()
        {
            var embed = new DiscordEmbedBuilder()
                .WithAuthor("PaimonBot", null, SharedData.logoURL)
                .WithTitle("Daily Grindable Items: Tuesday")
                .WithUrl("https://paimon.moe/items/")
                .WithColor(SharedData.defaultColour)
                .WithImageUrl("https://i.imgur.com/pWsHzkJ.png")
                .WithFooter("Image and Data taken from https://paimon.moe/items/")
                .WithTimestamp(DateTime.Now);
            return embed;
        }
        private DiscordEmbedBuilder Wednesday()
        {
            var embed = new DiscordEmbedBuilder()
                .WithAuthor("PaimonBot", null, SharedData.logoURL)
                .WithTitle("Daily Grindable Items: Wednesday")
                .WithUrl("https://paimon.moe/items/")
                .WithColor(SharedData.defaultColour)
                .WithImageUrl("https://i.imgur.com/HQg2NUk.png")
                .WithFooter("Image and Data taken from https://paimon.moe/items/")
                .WithTimestamp(DateTime.Now);
            return embed;
        }
        private DiscordEmbedBuilder Thursday()
        {
            var embed = new DiscordEmbedBuilder()
               .WithAuthor("PaimonBot", null, SharedData.logoURL)
               .WithTitle("Daily Grindable Items: Thursday")
               .WithUrl("https://paimon.moe/items/")
               .WithColor(SharedData.defaultColour)
               .WithImageUrl("https://i.imgur.com/bJXMOMZ.png") // RIP I wanted to use attachment
               .WithFooter("Image and Data taken from https://paimon.moe/items/")
               .WithTimestamp(DateTime.Now);
            return embed;
        }
        private DiscordEmbedBuilder Friday()
        {
            var embed = new DiscordEmbedBuilder()
                .WithAuthor("PaimonBot", null, SharedData.logoURL)
                .WithTitle("Daily Grindable Items: Friday")
                .WithUrl("https://paimon.moe/items/")
                .WithColor(SharedData.defaultColour)
                .WithImageUrl("https://i.imgur.com/pWsHzkJ.png")
                .WithFooter("Image and Data taken from https://paimon.moe/items/")
                .WithTimestamp(DateTime.Now);
            return embed;
        }
        private DiscordEmbedBuilder Saturday()
        {
            var embed = new DiscordEmbedBuilder()
                .WithAuthor("PaimonBot", null, SharedData.logoURL)
                .WithTitle("Daily Grindable Items: Saturday")
                .WithUrl("https://paimon.moe/items/")
                .WithColor(SharedData.defaultColour)
                .WithImageUrl("https://i.imgur.com/HQg2NUk.png")
                .WithFooter("Image and Data taken from https://paimon.moe/items/")
                .WithTimestamp(DateTime.Now);
            return embed;
        }
        private DiscordEmbedBuilder Sunday()
        {
            var embed = new DiscordEmbedBuilder()
                .WithAuthor("PaimonBot", null, SharedData.logoURL)
                .WithTitle("Daily Grindable Items: Sunday")
                .WithUrl("https://paimon.moe/items/")
                .WithColor(SharedData.defaultColour)
                .WithDescription("All Items are Grindable on a Sunday! " + Emojis.HappyEmote)
                .WithFooter("Image and Data taken from https://paimon.moe/items/")
                .WithTimestamp(DateTime.Now);
            return embed;
        }
        #endregion DailyEmbeds

    }
}