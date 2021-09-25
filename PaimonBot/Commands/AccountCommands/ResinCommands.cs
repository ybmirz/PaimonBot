using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using PaimonBot.Extensions.DataModels;
using PaimonBot.Services;
using PaimonBot.Services.HelpFormatter;
using PaimonBot.Services.ResinHelper;
using Serilog;
using System;
using System.Text;
using System.Threading.Tasks;

namespace PaimonBot.Commands
{
    [Group("resin")]
    [Description("A set of commands to manipulate your current resin count! Make sure to create an account if you haven't!")]
    [Category(CategoryName.Account)]
    public class ResinCommands : BaseCommandModule
    {
        [GroupCommand]
        [Description("Default command to return back your current resin amount!")]
        [Cooldown(1, 3, CooldownBucketType.Channel)]
        public async Task ResinCall(CommandContext ctx)
        {
            var traveler = SharedData.PaimonDB.GetTravelerBy("DiscordID", ctx.User.Id);
            if (traveler != null)
            {
                var embed = ResinEmbed(traveler, ctx.User);
                var messagebuilder = new DiscordMessageBuilder().AddEmbed(embed);
                await ctx.Channel.SendMessageAsync(messagebuilder).ConfigureAwait(false);
            }
            else
                await PaimonServices.SendRespondAsync(ctx, $"It seems you're not in the database, please create an account using `{SharedData.prefixes[0]}acc create` or to simply use the resin counter," +
                    $" please set your resin using `{SharedData.prefixes[0]}resin set [amount]`.", TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        }

        [Command("set")]
        [Description("Sets your resin amount into the database. If you do not have an account, a new Traveler account will be created.")]
        [Cooldown(1, 3, CooldownBucketType.User)]
        public async Task ResinSet(CommandContext ctx, int amount)
        {
            var traveler = SharedData.PaimonDB.GetTravelerBy("DiscordID", ctx.User.Id);

            if (amount > 160)
                amount = 160;
            else if (amount < 0)
                amount = 0;

            // Removes an already existing timer
            if (SharedData.resinTimers.Exists(timer => timer._discordID == ctx.User.Id))
            {
                var timer = SharedData.resinTimers.Find(timer => timer._discordID == ctx.User.Id);
                timer.StopAndDispose();
                SharedData.resinTimers.Remove(timer);
                Log.Information($"Previous Resin timer for User {ctx.User.Id} has been removed.");
            }
            
            if (traveler != null)
            {
                var updateTraveler = traveler;
                updateTraveler.ResinAmount = amount;
                updateTraveler.ResinUpdatedTime = DateTime.UtcNow;
                // Starts a new ResinTimer
                var aTimer = new ResinTimer(ctx.User.Id);
                aTimer.Start();
                SharedData.resinTimers.Add(aTimer);
                await ctx.RespondAsync($"You now have **{updateTraveler.ResinAmount}** resin {Emojis.ResinEmote}. Please use `{SharedData.prefixes[0]}resin` to check your resin.").ConfigureAwait(false);
            }
            else
            {
                Traveler newTraveler = new Traveler()
                {
                    DiscordID = ctx.User.Id,
                    ResinAmount = amount,
                    ResinUpdatedTime = DateTime.UtcNow,
                    // Default Values for the 'empty' fields
                    ParaGadget = null,
                    RealmCurrency = int.MinValue,
                    CurrencyUpdated = null
                };
                SharedData.PaimonDB.InsertTraveler(newTraveler);
                // Starts a new ResinTimer
                var aTimer = new ResinTimer(ctx.User.Id);
                aTimer.Start();
                SharedData.resinTimers.Add(aTimer);
                Log.Information($"Resin Timer for {aTimer._discordID} just started!");
                await ctx.RespondAsync($"A new resin user was made, you now have **{newTraveler.ResinAmount}** resin {Emojis.ResinEmote}. Please use `{SharedData.prefixes[0]}resin` to check your resin.").ConfigureAwait(false);
            }
        }

        private DiscordEmbedBuilder ResinEmbed(Traveler traveler, DiscordUser user)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"You currently have {Formatter.Bold(traveler.ResinAmount.ToString())}/160 resin {Emojis.ResinEmote}");
            sb.AppendLine($"Resin was last updated/added at <t:{((DateTimeOffset)traveler.ResinUpdatedTime.ToUniversalTime()).ToUnixTimeSeconds()}>");
            TimeSpan timeToCap = TimeSpan.FromMinutes((160 - traveler.ResinAmount) * 8);
            DateTimeOffset capTime = (DateTimeOffset)DateTime.UtcNow + timeToCap;
            sb.AppendLine($"Your resin will be filled <t:{capTime.ToUnixTimeSeconds()}:R>");

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithTitle($"{user.Username}#{user.Discriminator}'s Resin Information")
                .WithColor(SharedData.defaultColour)
                .WithThumbnail(user.AvatarUrl)
                .WithFooter("Use p~resin set [amount] to set a new resin amount")
                .WithDescription(sb.ToString())
                .WithTimestamp(DateTime.Now);

            return embed;
        }
    }
}