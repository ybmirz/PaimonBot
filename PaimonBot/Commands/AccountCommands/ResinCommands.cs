using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
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
                    $" please set your resin using `{SharedData.prefixes[0]}resin set [amount]`. {Emojis.CryEmote}", TimeSpan.FromSeconds(5)).ConfigureAwait(false);
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
                aTimer.ResinCapped += PaimonServices.ATimer_ResinCapped;
                Log.Information($"Resin Timer for {aTimer._discordID} just started!");
                SharedData.PaimonDB.ReplaceTraveler(updateTraveler);
                await ctx.RespondAsync($"You now have **{updateTraveler.ResinAmount}** resin {Emojis.ResinEmote}. Please use `{SharedData.prefixes[0]}resin` to check your resin. {Emojis.HappyEmote}").ConfigureAwait(false);
            }
            else
            {
                Traveler newTraveler = new Traveler()
                {
                    DiscordID = ctx.User.Id,
                    ResinAmount = amount,
                    ResinUpdatedTime = DateTime.UtcNow,
                    // Default Values for the 'empty' fields
                    ParaGadgetNextUse = null,
                    RealmCurrency = int.MinValue,
                    CurrencyUpdated = null
                };
                SharedData.PaimonDB.InsertTraveler(newTraveler);
                // Starts a new ResinTimer
                var aTimer = new ResinTimer(ctx.User.Id);
                aTimer.Start();
                SharedData.resinTimers.Add(aTimer);
                aTimer.ResinCapped += PaimonServices.ATimer_ResinCapped;
                Log.Information($"Resin Timer for {aTimer._discordID} just started!");
                await ctx.RespondAsync($"A new resin user was made, you now have **{newTraveler.ResinAmount}** resin {Emojis.ResinEmote}. Please use `{SharedData.prefixes[0]}resin` to check your resin. {Emojis.HappyEmote}").ConfigureAwait(false);
            }
        }

        [Command("reset")]
        [Description("Instantly resets your current resin amount to 0. If you do not have an account, a new Traveler account will be created.")]
        [Cooldown(1, 3, CooldownBucketType.User)]
        public async Task ResinReset(CommandContext ctx)
        {
            await ResinSet(ctx, 0);
        }

        [Command("remind")]
        [Description("Sets PaimonBot to remind you through DMs once your resin has reached that specific amount, defaults to 160 (Full) resin.")]
        [Cooldown(1, 3, CooldownBucketType.User)]
        [RequireGuild]
        public async Task ResinRemind(CommandContext ctx, int amount = 160)
        {
            if (amount > 160 || amount < 0)
            { await PaimonServices.SendRespondAsync(ctx, "Please enter a resin amount between `0-160`. Please try again.", TimeSpan.FromSeconds(3)); return; }
            var traveler = SharedData.PaimonDB.GetTravelerBy("DiscordID", ctx.User.Id);
            if (traveler != null)
            {
                if (SharedData.resinTimers.Exists(x => x._discordID == ctx.User.Id))
                {                  
                    var resinTimer = SharedData.resinTimers.Find(x => x._discordID == ctx.User.Id);
                    if (resinTimer._remind) // Already to be reminded.
                    {
                        await PaimonServices.SendEmbedToChannelAsync(ctx.Channel, "Resin Remind Error", $"It seems that you already have a Resin Remind at **{resinTimer._remindAt}** {Emojis.ResinEmote} **active**. To disable, please use " +
                            $"`{SharedData.prefixes[0]}resin unremind`. {Emojis.BlurpEmote}", TimeSpan.FromSeconds(6), ResponseType.Warning);
                        return;
                    }
                    try
                    {
                        var DmChannel = await ctx.Member.CreateDmChannelAsync().ConfigureAwait(false);
                        resinTimer.EnableRemind(amount, DmChannel);
                        Log.Information("Resin Remind for User {Id} Enabled, at {DateTime}; Resin: {Resin}", ctx.User.Id, DateTime.Now, resinTimer._remindAt);
                        await ctx.RespondAsync($"{Emojis.HappyEmote} Resin Remind Successful! Paimon will now remind you when your resin hits {resinTimer._remindAt} {Emojis.ResinEmote}. Be sure to keep DMs from server members enabled!").ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        if (e is UnauthorizedException)
                        {
                            await PaimonServices.SendEmbedToChannelAsync(ctx.Channel, "Resin Remind Error", "It seems that you do not have DMs from Server Members Enabled. " +
                                "Please enable to let Paimon talk to you! Once Enabled, please try again. Learn more [here](https://support.discord.com/hc/en-us/articles/217916488-Blocking-Privacy-Settings-)!"
                                , TimeSpan.FromSeconds(5), ResponseType.Warning);
                            Log.Warning("User {Id} tried to enable Resin Remind, however does not have DMs Enabled. {ExceptionMessage}", ctx.User.Id, e.Message);
                        }
                        else
                            throw e;
                    }
                }
                else
                    Log.Warning($"Invalid Operation: ResinRemind was called, User {ctx.User.Id} exists in the database but does not have a ResinTimer started in the List.");
            }
            else
                await PaimonServices.SendRespondAsync(ctx, $"It seems you're not in the database, please create an account using `{SharedData.prefixes[0]}acc create` or to simply use the resin counter," +
                    $" please set your resin using `{SharedData.prefixes[0]}resin set [amount]`.", TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        }

        [Command("unremind")]
        [Description("Tells PaimonBot to disable your Resin Reminder if active.")]
        [Cooldown(1, 3, CooldownBucketType.User)]
        [RequireGuild]
        public async Task ResinUnremind(CommandContext ctx)
        {
            var traveler = SharedData.PaimonDB.GetTravelerBy("DiscordID", ctx.User.Id);
            if (traveler != null)
            {
                if (SharedData.resinTimers.Exists(x => x._discordID == ctx.User.Id))
                {
                    var resinTimer = SharedData.resinTimers.Find(x => x._discordID == ctx.User.Id);
                    if (!resinTimer._remind) // Resin Remind not active.
                    {
                        await PaimonServices.SendEmbedToChannelAsync(ctx.Channel, "Resin Remind Error", $"It seems that you do not have Resin Remind currently active. Use `{SharedData.prefixes[0]}resin remind [amount]` to enable a resin remind."
                            ,TimeSpan.FromSeconds(6), ResponseType.Warning);
                        return;
                    }
                    resinTimer.DisableRemind();
                    Log.Information("Resin Remind for User {Id} Disabled, at {DateTime};", ctx.User.Id, DateTime.Now);
                    await ctx.RespondAsync($"{Emojis.BlurpEmote} Your resin remind has been disabled. To re-enable, use `{SharedData.prefixes[0]}resin remind [amount]`.").ConfigureAwait(false);
                }
                else
                    Log.Warning($"Invalid Operation: ResinRemind was called, User {ctx.User.Id} exists in the database but does not have a ResinTimer started in the List.");
            }
            else
                await PaimonServices.SendRespondAsync(ctx, $"It seems you're not in the database, please create an account using `{SharedData.prefixes[0]}acc create` or to simply use the resin counter," +
                    $" please set your resin using `{SharedData.prefixes[0]}resin set [amount]`.", TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        }

        private DiscordEmbedBuilder ResinEmbed(Traveler traveler, DiscordUser user)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"You currently have {Formatter.Bold(traveler.ResinAmount.ToString())}/160 resin {Emojis.ResinEmote}");
            sb.AppendLine($"Resin was last updated/added at <t:{((DateTimeOffset)traveler.ResinUpdatedTime.ToUniversalTime()).ToUnixTimeSeconds()}>");
            TimeSpan timeToCap = TimeSpan.FromMinutes((160 - traveler.ResinAmount) * 8);
            DateTimeOffset capTime = (DateTimeOffset)traveler.ResinUpdatedTime.ToUniversalTime() + timeToCap;
            sb.AppendLine($"Your resin will be filled <t:{capTime.ToUnixTimeSeconds()}:R>");

            if (SharedData.resinTimers.Exists(x => x._discordID == user.Id))
            {
                int resinRemind = SharedData.resinTimers.Find(x => x._discordID == traveler.DiscordID)._remindAt;
                if (resinRemind != int.MinValue)
                {
                    sb.AppendLine();
                    sb.AppendLine($"You will be reminded when your resin hits **{resinRemind}** {Emojis.ResinEmote}.");
                }              
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithTitle($"{user.Username}#{user.Discriminator}'s Resin Information")
                .WithColor(SharedData.defaultColour)
                .WithThumbnail(user.AvatarUrl)
                .WithFooter($"Use {SharedData.prefixes[0]}resin set [amount] to set a new resin amount")
                .WithDescription(sb.ToString())
                .WithTimestamp(DateTime.Now);

            return embed;
        }
    }
}