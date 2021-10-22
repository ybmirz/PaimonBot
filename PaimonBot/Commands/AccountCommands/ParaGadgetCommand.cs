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
using System.Linq;
using DSharpPlus.Exceptions;

namespace PaimonBot.Commands
{
    [Group("gadget"), Aliases("parametric", "para")]
    [Description("A set of commands to use, enable or disable remind for your parametric gadget.")]
    [Category(CategoryName.Account)]
    public class ParaGadgetCommand : BaseCommandModule
    {
        [GroupCommand]
        [Description("Default command to return back when you used your Parametric Gadget and when you can next use it")]
        [Cooldown(1, 3, CooldownBucketType.Channel)]
        public async Task GadgetCall(CommandContext ctx)
        {
            var traveler = SharedData.PaimonDB.GetTravelerBy("DiscordID", ctx.User.Id);
            if (traveler != null)
            {
                if (traveler.ParaGadgetNextUse == null)
                {
                    await PaimonServices.SendEmbedToChannelAsync(ctx.Channel, "Gadget Data Unavailable", $"It seems that you do not have any GadgetData set. Please set when you used your Gadget in-game using " +
                              $"`{SharedData.prefixes[0]}gadget use`. {Emojis.BlurpEmote}", TimeSpan.FromSeconds(6), ResponseType.Warning);
                    return;
                }
                var embed = ParaGadgetEmbed(traveler, ctx.User);
                var messageBuilder = new DiscordMessageBuilder().AddEmbed(embed);
                await ctx.Channel.SendMessageAsync(messageBuilder).ConfigureAwait(false);
            } else
                await PaimonServices.SendRespondAsync(ctx, $"It seems you're not in the database, please create an account using `{SharedData.prefixes[0]}acc create`. {Emojis.CryEmote}",
                    TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        }

        [Command("use")]
        [Description("Use this command to enable a parametric gadget")]
        [Cooldown(1, 3, CooldownBucketType.User)]
        public async Task ParametricGadgetUpdate(CommandContext ctx)
        {
            var traveler = SharedData.PaimonDB.GetTravelerBy("DiscordID", ctx.User.Id);
            if (traveler != null)
            {
                var remindedUsers = SharedData.ParaRemindedUsers.Keys.ToList();
                if (remindedUsers.Exists(x => x == ctx.User.Id))
                    SharedData.ParaRemindedUsers.Remove(ctx.User.Id);
                var updateTraveler = traveler;
                updateTraveler.ParaGadgetNextUse = DateTime.UtcNow.AddDays(7);
                updateTraveler.ParaGadgetRemind = false;
                // Saves to the Traveler's database data.
                SharedData.PaimonDB.ReplaceTraveler(updateTraveler);
                await ctx.RespondAsync($"Succesfully recorded the use of Parametric Gadget! Please use `{SharedData.prefixes[0]}gadget` to check its data.");
                Log.Information("Succesfully recorded Traveler {Id}'s Gadget Next Use.", ctx.User.Id);
            }
            else
                await PaimonServices.SendRespondAsync(ctx, $"It seems you're not in the database, please create an account using `{SharedData.prefixes[0]}acc create`. {Emojis.CryEmote}",
                    TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        }

        [Command("remind")]
        [Description("Enables for Paimon to remind you when you can next use your parametric gadget ")]
        [RequireGuild]
        [Cooldown(1, 3, CooldownBucketType.User)]
        public async Task ParametricGadgetRemind(CommandContext ctx)
        {
            var traveler = SharedData.PaimonDB.GetTravelerBy("DiscordID", ctx.User.Id);
            if (traveler != null)
            {
                if (traveler.ParaGadgetNextUse == null)
                {
                    await PaimonServices.SendEmbedToChannelAsync(ctx.Channel, "Gadget Data Unavailable", $"It seems that you do not have any GadgetData set. Please set when you used your Gadget in-game using " +
                            $"`{SharedData.prefixes[0]}gadget use`. {Emojis.BlurpEmote}", TimeSpan.FromSeconds(6), ResponseType.Warning);
                    Log.Warning($"Invalid Operation: ParaGadgetRemind was called, but User {ctx.User.Id}'s Traveler data does not seem to have Gadget data.");
                    return;
                }
                var nextUse = (DateTimeOffset)traveler.ParaGadgetNextUse.ToUniversalTime();
                if (SharedData.ParaRemindedUsers.ContainsKey(ctx.User.Id))
                {                    
                    await PaimonServices.SendEmbedToChannelAsync(ctx.Channel, "Gadget Remind Error", $"It seems that you already will be reminded on your next Gadget use at {nextUse.ToUnixTimeSeconds()}. To disable, please use " +
                            $"`{SharedData.prefixes[0]}gadget unremind`. {Emojis.BlurpEmote}", TimeSpan.FromSeconds(6), ResponseType.Warning);                    
                    return;
                }
                try
                {
                    SharedData.ParaRemindedUsers.Add(ctx.User.Id, nextUse);
                    var DmChannel = await ctx.Member.CreateDmChannelAsync().ConfigureAwait(false);
                    SharedData.ParaReminderUsersDMs.Add(ctx.User.Id, DmChannel);

                    // Sets the traveler's data to be reminded
                    traveler.ParaGadgetRemind = true;
                    SharedData.PaimonDB.UpdateTraveler(traveler, "ParaGadgetRemind", traveler.ParaGadgetRemind);

                    Log.Information("Gadget Remind for User {ID} has been added into the dict. RemindTime: {Time}", ctx.User.Id, nextUse);
                    await ctx.RespondAsync($"{Emojis.HappyEmote} Gagdet Remind Succesful ").ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    if (e is UnauthorizedException)
                    {
                        await PaimonServices.SendEmbedToChannelAsync(ctx.Channel, "Gadget Remind Error", "It seems that you do not have DMs from Server Members Enabled. " +
                                "Please enable to let Paimon talk to you! Once Enabled, please try again. Learn more [here](https://support.discord.com/hc/en-us/articles/217916488-Blocking-Privacy-Settings-)!"
                                , TimeSpan.FromSeconds(5), ResponseType.Warning);
                        Log.Warning("User {Id} tried to enable Gadget Remind, however does not have DMs Enabled. {ExceptionMessage}", ctx.User.Id, e.Message);
                    }
                    else
                        Log.Error("This is Exception E {Exception} , Message: {E.Message} Stacktrace: {E.StackTrace}");
                }
            } 
            else
                await PaimonServices.SendRespondAsync(ctx, $"It seems you're not in the database, please create an account using `{SharedData.prefixes[0]}acc create`. {Emojis.CryEmote}",
                    TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        }

        [Command("unremind")]
        [Description("Disables Paimon to remind you on your next gadget use, if exists.")]
        [RequireGuild]
        [Cooldown(1, 3, CooldownBucketType.User)]
        public async Task ParametricGadgetUnremind(CommandContext ctx)
        {
            var traveler = SharedData.PaimonDB.GetTravelerBy("DiscordID", ctx.User.Id);
            if (traveler != null)
            {
                if (SharedData.ParaRemindedUsers.ContainsKey(ctx.User.Id))
                {
                    // Removing from both lists
                    SharedData.ParaRemindedUsers.Remove(ctx.User.Id);
                    SharedData.ParaReminderUsersDMs.Remove(ctx.User.Id);

                    // Updating Traveler data
                    traveler.ParaGadgetRemind = false;
                    SharedData.PaimonDB.UpdateTraveler(traveler, "ParaGadgetRemind", traveler.ParaGadgetRemind);

                    Log.Information("Gagdet Remind for User {Id} has been deleted from the list, and disabled.");
                    await ctx.RespondAsync($"{Emojis.BlurpEmote} Your Gadget Remind has been disabled. To re-enable, use `{SharedData.prefixes[0]}gadget remind`").ConfigureAwait(false);
                }
                else
                    await PaimonServices.SendEmbedToChannelAsync(ctx.Channel, "Gadget Remind Error", $"It seems that you do not have any Gadget Reminder active. Please use " +
                            $"`{SharedData.prefixes[0]}gadget remind` to be reminded when you can next use your gadget. {Emojis.BlurpEmote}", TimeSpan.FromSeconds(6), ResponseType.Warning);
            } else
                await PaimonServices.SendRespondAsync(ctx, $"It seems you're not in the database, please create an account using `{SharedData.prefixes[0]}acc create`. {Emojis.CryEmote}",
                    TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        }

        [Command("delete"), Aliases("remove", "clear")]
        [Description("Deletes the Parametric Gadget Information from your Traveler's Account. Disables Gadget reminding as well.")]
        [Cooldown(1, 3, CooldownBucketType.User)]
        public async Task GadgetDelete(CommandContext ctx)
        {
            var traveler = SharedData.PaimonDB.GetTravelerBy("DiscordID", ctx.User.Id);
            if (traveler != null)
            {
                if (traveler.ParaGadgetNextUse == null)
                {
                    await PaimonServices.SendEmbedToChannelAsync(ctx.Channel, "Gadget Data Unavailable", $"It seems that you do not have any GadgetData set. Please set when you used your Gadget in-game using " +
                                $"`{SharedData.prefixes[0]}gadget use`. {Emojis.BlurpEmote}", TimeSpan.FromSeconds(6), ResponseType.Warning);
                    return;
                }
                traveler.ParaGadgetNextUse = null;
                traveler.ParaGadgetRemind = false;
                SharedData.PaimonDB.ReplaceTraveler(traveler);                
                SharedData.ParaRemindedUsers.Remove(ctx.User.Id); // doesn't throw
                SharedData.ParaReminderUsersDMs.Remove(ctx.User.Id);

                await ctx.RespondAsync($"Parametric Gadget Usage Data has been deleted. Thanks for using PaimonBot! " + Emojis.HappyEmote).ConfigureAwait(false);
                Log.Information("Traveler {id}'s Parametric Gadget Data has been succesfully deleted.", ctx.User.Id);
            }
            else
                await PaimonServices.SendRespondAsync(ctx, $"It seems you're not in the database, please create an account using `{SharedData.prefixes[0]}acc create`. {Emojis.CryEmote}",
                    TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        }
        private DiscordEmbedBuilder ParaGadgetEmbed(Traveler traveler, DiscordUser user)
        {
            DateTimeOffset offset = (DateTimeOffset)traveler.ParaGadgetNextUse.ToUniversalTime() - TimeSpan.FromDays(7);
            DateTimeOffset remindedTime = (DateTimeOffset)traveler.ParaGadgetNextUse.ToUniversalTime();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"You used your Parametric Gadget on <t:{offset.ToUnixTimeSeconds()}>");
            sb.AppendLine($"You can next use your Parametric Gadget <t:{remindedTime.ToUnixTimeSeconds()}:R> on <t:{remindedTime.ToUnixTimeSeconds()}>");
            sb.AppendLine($"Use `{SharedData.prefixes[0]}gadget remind` or `unremind` to be reminded or unreminded on when you can next use your gadget.");
            sb.AppendLine($"You could also use `{SharedData.prefixes[0]}gadget delete` to delete/clear the data.");

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithTitle($"{user.Username}#{user.Discriminator}'s Parametric Gadget Information")
                .WithColor(SharedData.defaultColour)
                .WithThumbnail(user.AvatarUrl)
                .WithDescription(sb.ToString())
                .WithFooter($"Use {SharedData.prefixes[0]}gadget use to use your gadget, once you've used it in game.")
                .WithTimestamp(DateTime.Now);

            return embed;
        }
    }
}
