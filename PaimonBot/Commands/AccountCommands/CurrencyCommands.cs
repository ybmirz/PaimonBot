using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using PaimonBot.Extensions.Data;
using PaimonBot.Extensions.DataModels;
using PaimonBot.Services;
using PaimonBot.Services.CurrencyHelper;
using PaimonBot.Services.HelpFormatter;
using Serilog;
using System;
using System.Text;
using System.Threading.Tasks;

namespace PaimonBot.Commands
{
    [Group("currency")]
    [Description("A set of commands to manipulate your current curency amount! Please make sure to create " +
        "an account before using any of the commands.")]
    [Cooldown(1, 3, CooldownBucketType.Channel)]
    [Category(CategoryName.Account)]
    public class CurrencyCommands : BaseCommandModule
    {
        [GroupCommand]
        [Description("Default command to return back your current currency amount. Please ensure you've created an account.")]
        [Cooldown(1,3, CooldownBucketType.Channel)]
        public async Task CurrencyCall(CommandContext ctx)
        {
            var traveler = SharedData.PaimonDB.GetTravelerBy("DiscordID", ctx.User.Id);
            if (traveler != null)
            {
                // If the traveler does not have realm currency information.
                if (traveler.RealmCurrency == int.MinValue && traveler.CurrencyUpdated == null) // Traveler does not have Realm Information, meaning a strictly resin user
                {
                    await PaimonServices.SendEmbedToChannelAsync(ctx.Channel, "Account Error",
                        $"It seems that your account does not have any Realm Currency Information. Please make sure you have an account created using `{SharedData.prefixes[0]}acc create`. Use the command as well to overwrite your data."
                        , TimeSpan.FromSeconds(6), ResponseType.Warning);
                    Log.Warning("[MISSING] User {Id}'s Realm Information was called, but did not exist. in {Channel}", ctx.User.Id, ctx.Channel);
                    return;
                }
                var embed = CurrencyEmbed(traveler, ctx.User);
                var msgBuilder = new DiscordMessageBuilder().AddEmbed(embed);
                await ctx.Channel.SendMessageAsync(msgBuilder).ConfigureAwait(false);
            }
            else
                await PaimonServices.SendRespondAsync(ctx, $"It seems that you're not in the database, please create an account using `{SharedData.prefixes[0]}acc create`. " +
                    $"This command group will not function without an account. {Emojis.CryEmote}", TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        }

        [Command("set")]
        [Description("Sets your realm currency amount into the database. If you do not have an account, this command will not work. Please do p~acc create to create one.")]
        [Cooldown(1, 3, CooldownBucketType.Channel)]
        public async Task CurrencySet(CommandContext ctx, int amount)
        {
            var traveler = SharedData.PaimonDB.GetTravelerBy("DiscordID", ctx.User.Id);
            if (traveler != null)
            {
                if (traveler.RealmCurrency == int.MinValue && traveler.CurrencyUpdated == null) // Traveler does not have Realm Information, meaning a strictly resin user
                {
                    await PaimonServices.SendEmbedToChannelAsync(ctx.Channel, "Account Error",
                        $"It seems that your account does not have any Realm Currency Information. Please make sure you have an account created using `{SharedData.prefixes[0]}acc create`. Use the command as well to overwrite your data."
                        , TimeSpan.FromSeconds(6), ResponseType.Warning);
                    Log.Warning("User {Id}'s Realm Information was called, but did not exist. in {Channel}", ctx.User.Id, ctx.Channel);
                    return;
                }
                var realmTrustRank = traveler.RealmTrustRank;
                var currencyCap = realmTrustRank.GetTrustRankCurrencyCap();
                var adeptalEnergyLvl = CurrencyServices.ParseAdeptalFromInt(traveler.AdeptalEnergy);

                // Amount > Cap, make it equal to Cap 
                if (amount > currencyCap)
                    amount = currencyCap;
                else if (amount < 0)
                    amount = 0;

                if (SharedData.currencyTimer.Exists(timer => timer.DiscordId == ctx.User.Id))
                {
                    var timer = SharedData.currencyTimer.Find(timer => timer.DiscordId == ctx.User.Id);
                    timer.StopAndDispose();
                    SharedData.currencyTimer.Remove(timer);
                    Log.Information($"Previous Currency timer for User {ctx.User.Id} has been removed.");
                }

                var updateTraveler = traveler;
                updateTraveler.RealmCurrency = amount;
                updateTraveler.CurrencyUpdated = DateTime.UtcNow;
                // Starting a new CurrencyTimer
                var aTimer = new RealmCurrencyTimer(ctx.User.Id, realmTrustRank, adeptalEnergyLvl);
                aTimer.Start();
                SharedData.PaimonDB.ReplaceTraveler(updateTraveler);
                SharedData.currencyTimer.Add(aTimer);
                aTimer.CurrencyCapped += PaimonServices.ATimer_CurrencyCapped;
                Log.Information($"Currency Timer has been started for {aTimer.DiscordId}!");
                await ctx.RespondAsync($"You now have **{updateTraveler.RealmCurrency}** realm currency {Emojis.AdeptalEmote}. Please use `{SharedData.prefixes[0]}currency` to check your current realm currency. {Emojis.HappyEmote}").ConfigureAwait(false);
            }
            else
                await PaimonServices.SendRespondAsync(ctx, $"It seems that you're not in the database, please create an account using `{SharedData.prefixes[0]}acc create`. " +
                    $"This command group will not function without an account. {Emojis.CryEmote}", TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        }

        [Command("reset")]
        [Description("Instantly resets your current realm currency amount to 0. If you do not have an account, this command will not work. Please use p~acc create to create one.")]
        [Cooldown(1, 3, CooldownBucketType.User)]
        public async Task ResinReset(CommandContext ctx)
        {
            await CurrencySet(ctx, 0);
        }

        private DiscordEmbedBuilder CurrencyEmbed(Traveler traveler, DiscordUser user)
        {            
            var currentRC = traveler.RealmCurrency;
            var cap = traveler.RealmTrustRank.GetTrustRankCurrencyCap();
            var updatedOffset = (DateTimeOffset)traveler.CurrencyUpdated.ToUniversalTime();
            var adeptalEnergy = CurrencyServices.ParseAdeptalFromInt(traveler.AdeptalEnergy);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Current Realm Trust Rank: **{traveler.RealmTrustRank}**");
            sb.AppendLine($"Current Adeptal Energy amount and level: {Formatter.Bold(traveler.AdeptalEnergy.ToString())} {Emojis.AdeptalEmote} ({adeptalEnergy})");
            sb.AppendLine($"You currently have {Formatter.Bold(currentRC.ToString())}/{cap} realm currency {Emojis.CurrencyEmote}");
            sb.AppendLine($"You last updated your currency <t:{updatedOffset.ToUnixTimeSeconds()}:R>");

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithTitle($"{user.Username}#{user.Discriminator}'s Realm Currency Information")
                .WithColor(SharedData.defaultColour)
                .WithThumbnail(user.AvatarUrl)
                .WithFooter($"User {SharedData.prefixes[0]}currency set [amount] to set a new currency amount")
                .WithDescription(sb.ToString())
                .WithTimestamp(DateTime.Now);

            return embed;
        }
        
    }
}