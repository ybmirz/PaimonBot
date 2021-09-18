using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
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
            Log.Information($"Command {e.Command.Name} has been executed succesfully by {e.Context.User.Id} in {e.Context.Guild?.Name} ({e.Context.Guild?.Id})");
            return Task.CompletedTask;
        }
        public static async Task EventHandlers_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            switch (e.Exception)
            {
                case CommandNotFoundException:
                    await PaimonServices.SendEmbedToChannelAsync(e.Context.Channel, "Command Not Found", e.Exception.Message, ResponseType.Missing)
                        .ConfigureAwait(false);
                    break;
                case InvalidOperationException:
                    await PaimonServices.SendEmbedToChannelAsync(e.Context.Channel, "Invalid Operation Exception", e.Exception.Message + $"| Please contact developer through {SharedData.prefixes[0]}contact dev", ResponseType.Warning)
                        .ConfigureAwait(false);
                    break;
                case ArgumentNullException:
                case ArgumentException:
                    await PaimonServices.SendEmbedToChannelAsync(e.Context.Channel,
                        "Argument Exception",
                        $"Invalid or Missing Arguments. `{SharedData.prefixes[0]}help {e.Command?.QualifiedName}`",
                        ResponseType.Warning).ConfigureAwait(false);
                    break;
                case UnauthorizedException:
                    await PaimonServices.SendEmbedToChannelAsync(e.Context.Channel,
                        "Unauthorized Exception",
                        $"One of us does not have the required permissions. Please contact developer through {SharedData.prefixes[0]}contact dev.",
                        ResponseType.Warning).ConfigureAwait(false);                    
                    break;
                case ChecksFailedException cfe: //attribute check from the cmd failed
                    string title = "Check Failed Exception";
                    foreach (var check in cfe.FailedChecks)
                        switch (check)
                        {
                            case RequirePermissionsAttribute perms:
                                await PaimonServices.SendEmbedToChannelAsync(e.Context.Channel, title,
                                    $"One of us does not have the following required permissions: ({perms.Permissions.ToPermissionString()})",
                                    TimeSpan.FromSeconds(6),ResponseType.Error).ConfigureAwait(false);
                                break;
                            case RequireUserPermissionsAttribute perms:
                                await PaimonServices.SendEmbedToChannelAsync(e.Context.Channel, title,
                                    $"You do not have the following sufficient permissions: ({perms.Permissions.ToPermissionString()})",
                                    ResponseType.Error).ConfigureAwait(false);
                                break;
                            case RequireBotPermissionsAttribute perms:
                                await PaimonServices.SendEmbedToChannelAsync(e.Context.Channel, title,
                                    $"I do not have the following sufficient permissions: ({perms.Permissions.ToPermissionString()})",
                                    ResponseType.Error).ConfigureAwait(false);
                                break;
                            case RequireNsfwAttribute:
                                await PaimonServices.SendEmbedToChannelAsync(e.Context.Channel, title,
                                    $"This command is only bound to NSFW Channels",
                                    ResponseType.Error).ConfigureAwait(false);
                                break;
                            case CooldownAttribute:
                                    await PaimonServices.SendEmbedToChannelAsync(e.Context.Channel,
                                        title, $"Calm down there mate! Please wait a few more seconds.", ResponseType.Warning)
                                    .ConfigureAwait(false);
                                break;
                            default:
                                await PaimonServices.SendEmbedToChannelAsync(e.Context.Channel, title,
                                    $"Unknown Check triggered. Please contact the developer by `{SharedData.prefixes[0]}contact dev`",
                                    ResponseType.Error).ConfigureAwait(false);
                                break;
                        }
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
            return Task.CompletedTask;
        }

        public static Task _Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            Log.Information("PaimonBot is now connected and awake!");
            return Task.CompletedTask;
        }
        public static Task _client_SocketClosed(DiscordClient sender, DSharpPlus.EventArgs.SocketCloseEventArgs e)
        {
            if (e.CloseCode is 4014)
                Log.Error("Missing intents! Enable them on the developer dashboard (discord.com/developers/applications/{AppId})",sender.CurrentApplication.Id);
            return Task.CompletedTask;
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