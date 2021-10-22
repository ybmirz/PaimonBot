using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaimonBot.Services
{
    public static class ExceptionEventHandlers
    {
        #region CommandEventHandlers
        public static Task EventHandlers_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            if (e.Context.Guild == null)
                Log.Information($"Command {e.Command?.QualifiedName} has been executed successfully by {e.Context.User.Id} through DMs. Using: {e.Context.Message.Content}");            
            else
                Log.Information($"Command {e.Command?.QualifiedName} has been executed succesfully by {e.Context.User.Id} in {e.Context.Guild?.Name} ({e.Context.Guild?.Id}). " +
                    $"Using: {e.Context.Message.Content}");
            return Task.CompletedTask;
        }
        public static async Task EventHandlers_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            int secondsDelay = 8;
            switch (e.Exception)
            {
                case CommandNotFoundException x:
                    await PaimonServices.SendEmbedToChannelAsync(e.Context.Channel, "Command Not Found", e.Exception.Message,
                        TimeSpan.FromSeconds(secondsDelay), ResponseType.Missing)
                        .ConfigureAwait(false);
                    Log.Warning("{User} tried to look for a command that doesn't exist! Command: '{CommandName}' | Message: '{Message}'",
                        e.Context.User, x.CommandName, e.Context.Message.Content);
                    break;
                case InvalidOperationException:
                    await PaimonServices.SendEmbedToChannelAsync(e.Context.Channel, "Invalid Operation Exception:", e.Exception.Message + $"| Please contact developer through {SharedData.prefixes[0]}contact dev",
                        TimeSpan.FromSeconds(secondsDelay), ResponseType.Warning)
                        .ConfigureAwait(false);
                    Log.Error("An Invalid Operation Exception was thrown: {ExceptionType} {ExceptionMsg} {StackTrace}",
                        e.Exception.GetType(), e.Exception.Message, e.Exception.StackTrace);
                    break;
                case ArgumentNullException:
                case ArgumentException x:
                    await PaimonServices.SendEmbedToChannelAsync(e.Context.Channel,
                        "Argument Exception",
                        $"Invalid or Missing Arguments. `{SharedData.prefixes[0]}help {/*(e.Command.Parent is CommandGroup ? e.Command.Parent.Name + " " + e.Command.QualifiedName :*/ e.Command?.QualifiedName}`",
                        TimeSpan.FromSeconds(secondsDelay), ResponseType.Warning).ConfigureAwait(false);
                    Log.Warning("{User} had Invalid or Missing Arguments for Command {CommandName} in {Channel} ({Guild}) | {StackTrace}",
                        e.Context.User, e.Command?.QualifiedName, e.Context.Channel, e.Context.Guild, e.Exception.StackTrace);
                    break;
                case UnauthorizedException:
                    await PaimonServices.SendEmbedToChannelAsync(e.Context.Channel,
                        "Unauthorized Exception",
                        $"One of us does not have the required permissions to process that command. Please contact developer through {SharedData.prefixes[0]}contact dev.",
                        TimeSpan.FromSeconds(secondsDelay), ResponseType.Warning).ConfigureAwait(false);
                    Log.Warning("{User} started a command/process but one of us is not authorized.{Command} {Guild} {StackTrace}"
                        , e.Context.User, e.Command?.QualifiedName, e.Context.Guild, e.Exception.StackTrace);
                    break;
                case ChecksFailedException cfe: //attribute check from the cmd failed
                    string title = "Check Failed Error";
                    foreach (var check in cfe.FailedChecks)
                        switch (check)
                        {
                            case RequirePermissionsAttribute perms:
                                await PaimonServices.SendEmbedToChannelAsync(e.Context.Channel, title,
                                    $"One of us does not have the following required permissions: ({perms.Permissions.ToPermissionString()})",
                                    TimeSpan.FromSeconds(secondsDelay), ResponseType.Error).ConfigureAwait(false);
                                break;
                            case RequireUserPermissionsAttribute perms:
                                await PaimonServices.SendEmbedToChannelAsync(e.Context.Channel, title,
                                    $"You do not have the following sufficient permissions: ({perms.Permissions.ToPermissionString()})",
                                    TimeSpan.FromSeconds(secondsDelay), ResponseType.Error).ConfigureAwait(false);
                                break;
                            case RequireBotPermissionsAttribute perms:
                                await PaimonServices.SendEmbedToChannelAsync(e.Context.Channel, title,
                                    $"I do not have the following sufficient permissions: ({perms.Permissions.ToPermissionString()})",
                                    TimeSpan.FromSeconds(secondsDelay), ResponseType.Error).ConfigureAwait(false);
                                break;
                            case RequireNsfwAttribute:
                                await PaimonServices.SendEmbedToChannelAsync(e.Context.Channel, title,
                                    $"This command is only bound to NSFW Channels",
                                    TimeSpan.FromSeconds(secondsDelay), ResponseType.Error).ConfigureAwait(false);
                                break;
                            case CooldownAttribute x:
                                await PaimonServices.SendEmbedToChannelAsync(e.Context.Channel,
                                    title, $"Calm down there mate! Please wait a few more seconds. This command is still in cooldown.",
                                    TimeSpan.FromSeconds(secondsDelay), ResponseType.Warning)
                                .ConfigureAwait(false);
                                break;
                            case RequireOwnerAttribute:
                                await PaimonServices.SendEmbedToChannelAsync(e.Context.Channel,
                                    title, $"This command is only applicable to my Owner.", TimeSpan.FromSeconds(secondsDelay),
                                    ResponseType.Warning).ConfigureAwait(false);
                                break;
                            case RequireDirectMessageAttribute:
                                await PaimonServices.SendEmbedToChannelAsync(e.Context.Channel, title, "Command Usage Error. This command can only be done through **Direct Messages** (DM) Channels. Please try again there. " +
                                    "Paimon's waiting for you! " +Emojis.HappyEmote, TimeSpan.FromSeconds(secondsDelay),
                                    ResponseType.Warning).ConfigureAwait(false);
                                break;
                            case RequireGuildAttribute:
                                await PaimonServices.SendEmbedToChannelAsync(e.Context.Channel, title, "Command Usage Error. This command can only be done in a Guild. Please try again in a guild that Paimon and you are in. " +
                                    "Paimon's waiting for you!", TimeSpan.FromSeconds(secondsDelay), ResponseType.Warning)
                                    .ConfigureAwait(false);
                                break;
                            default:
                                await PaimonServices.SendEmbedToChannelAsync(e.Context.Channel, title,
                                    $"Unknown Check triggered. Please contact the developer by `{SharedData.prefixes[0]}contact dev`",
                                    TimeSpan.FromSeconds(secondsDelay), ResponseType.Error).ConfigureAwait(false);
                                break;
                        }
                    Log.Warning("{User} has triggered the following Check Failed Exception(s): {Exceptions} in {Channel}",
                            e.Context.User, string.Join(",", cfe.FailedChecks), e.Context.Channel);
                    break;
                default:
                    //await PaimonServices.SendEmbedToChannelAsync(e.Context.Channel, "Unknown Exception Handle", $"Exception Message: {e.Exception.Message} | Type: {e.Exception.GetType()}", TimeSpan.FromSeconds(6)
                    //    , ResponseType.Warning);
                    Log.Warning("{User} has failed a command with the following: {Exception} in {Channel} StackTrace: {StackTrace}",
                        e.Context.User, e.Exception.Message, e.Context.Channel, e.Exception.StackTrace);
                    break;
            }
        }
        #endregion CommandEventHandlers


        #region ClientEventHandlers
        public static Task _Client_GuildCreated(DiscordClient sender, DSharpPlus.EventArgs.GuildCreateEventArgs e)
        {
            Log.Information($"PaimonBot was fished by {e.Guild.Name} ({e.Guild.Id})");
            return Task.CompletedTask;
        }

        public static Task _Client_ClientErrored(DiscordClient sender, DSharpPlus.EventArgs.ClientErrorEventArgs e)
        {
            Log.Error($"PaimonBot suddenly felt sleepy... zzz Event: {e.EventName} | Exception: {e.Exception.Message} {e.Exception.StackTrace}");
            return Task.CompletedTask;
        }

        public static Task _Client_GuildAvailable(DiscordClient sender, DSharpPlus.EventArgs.GuildCreateEventArgs e)
        {
            Log.Information($"PaimonBot sees a traveler's guild! Name:{e.Guild.Name} ({e.Guild.Id})");
            _ = Task.Run(async () => GetDMs(e));
            return Task.CompletedTask;
        }

        private static async Task GetDMs(DSharpPlus.EventArgs.GuildCreateEventArgs e)
        {
            var membersFound = e.Guild.Members.Keys.Where(x => SharedData.ParaRemindedUsers.Keys.Contains(x));
            foreach (var memberID in membersFound)
            {
                var member = await e.Guild.GetMemberAsync(memberID);
                try {
                    var DMChannel = await member.CreateDmChannelAsync().ConfigureAwait(false);
                    SharedData.ParaReminderUsersDMs.Add(memberID, DMChannel);
                }                
                catch (Exception ex)
                {
                    if (ex is UnauthorizedException)
                    {
                        Log.Warning("Member {Id} was found in Remindlist and Guild, however DM channel was unable to be created. {ExceptionMsg}", memberID, ex.Message);
                    }
                    else
                        Log.Error("This is Exception E {Exception} , Message: {E.Message} Stacktrace: {E.StackTrace}");
                }
            }
            Log.Information($"Cached {SharedData.ParaReminderUsersDMs.Count} DMs for Parametric Gadget Reminding.");
        }

        public static Task _Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            Log.Information("PaimonBot is now connected and awake!");
            return Task.CompletedTask;
        }
        public static Task _client_SocketClosed(DiscordClient sender, DSharpPlus.EventArgs.SocketCloseEventArgs e)
        {
            if (e.CloseCode is 4014)
                Log.Error("Missing intents! Enable them on the developer dashboard (discord.com/developers/applications/{AppId})", sender.CurrentApplication.Id);
            return Task.CompletedTask;
        }

        public static async Task _Client_ComponentInteraction(DiscordClient sender, DSharpPlus.EventArgs.ComponentInteractionCreateEventArgs e)
        {
        }
        #endregion ClientEventHandlers

        public static async Task SubscribeToEventsAsnc(DiscordShardedClient _Client)
        {
            Log.Information("Hooking tasks and exception events");
            // CommandExceptions
            IEnumerable<CommandsNextExtension> cNext = (await _Client.GetCommandsNextAsync()).Values;
            foreach (CommandsNextExtension c in cNext)
            {
                c!.CommandErrored += EventHandlers_CommandErrored;
                c!.CommandExecuted += EventHandlers_CommandExecuted;
            }
            Log.Debug("Regitered Command Exception-Handlers for {Shards} shard(s)", cNext.Count());
        }
    }
}