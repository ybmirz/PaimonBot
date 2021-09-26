using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using PaimonBot.Commands;
using PaimonBot.Models;
using PaimonBot.Services;
using PaimonBot.Services.HelpFormatter;
using PaimonBot.Services.ResinHelper;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PaimonBot
{
    public class PaimonBot
    {
        // Private Paimon parts
        private static string _BotName { set; get; }
        private string _token { set; get; }
        private List<string> _prefixes { set; get; } = new List<string>();        
        public DiscordShardedClient _Client { get; private set; }
        public InteractivityExtension _Interactivity { get; private set; }
        public  IReadOnlyDictionary<int, CommandsNextExtension> _Commands;

        public PaimonBot(BotConfigurationModel BotConfig
            ,IServiceProvider services)
        {
            _BotName = BotConfig.BotName;
            _token = BotConfig.token;
            foreach (var prefix in BotConfig.prefixes) { _prefixes.Add(prefix); }
            SharedData.logoURL = BotConfig.LogoURL;            

            _ = PaimonWoke(services);           
        }

        private async Task PaimonWoke(IServiceProvider services)
        { 
            var serilogFactory = new LoggerFactory().AddSerilog();
            var config = new DiscordConfiguration
            {
                Token = _token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                Intents = DiscordIntents.All,
                LoggerFactory = serilogFactory,
                MinimumLogLevel = LogLevel.Debug,
                LargeThreshold = 500
            };
            _Client = new DiscordShardedClient(config);            

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = _prefixes,
                EnableDefaultHelp = true,
                EnableMentionPrefix = true,
                EnableDms = true,
                DmHelp = false,
                Services = services
            };
            _Commands = await _Client.UseCommandsNextAsync(commandsConfig);

            var interactivityConfig = new InteractivityConfiguration
            {
                PaginationBehaviour = DSharpPlus.Interactivity.Enums.PaginationBehaviour.WrapAround,
                PaginationDeletion = DSharpPlus.Interactivity.Enums.PaginationDeletion.DeleteEmojis,
                Timeout = TimeSpan.FromMinutes(1),
                PaginationButtons = new PaginationButtons() {
                    Left = new DiscordButtonComponent(ButtonStyle.Secondary, "pagination_left","",emoji: new DiscordComponentEmoji("◀")),
                    Right = new DiscordButtonComponent(ButtonStyle.Secondary, "pagination_right","",emoji: new DiscordComponentEmoji("▶")),
                    Stop = new DiscordButtonComponent(ButtonStyle.Secondary, "pagination_stop","",emoji: new DiscordComponentEmoji("🛑"))                    
                },
                ButtonBehavior = DSharpPlus.Interactivity.Enums.ButtonPaginationBehavior.DeleteButtons,                
            };
            await _Client.UseInteractivityAsync(interactivityConfig);

            // Start the bot
            await StartAsync();
            // Disconnect the bot when closing program
            Console.CancelKeyPress += Console_CancelKeyPress; 
        }


        /// <summary>
        /// Registering Commands and HelpFormatter for each shard.
        /// </summary>
        /// <returns></returns>
        private async Task InitializeCommandsNext()
        {
            Log.Information("Initializing DSharpPlus Commands Framework");
            #region Registering
            var t = Stopwatch.StartNew();
            IReadOnlyDictionary<int, CommandsNextExtension> cnext = await _Client.GetCommandsNextAsync();
            foreach (var cmdShard in cnext.Values)
            {
                cmdShard.RegisterCommands<ResinCommands>();
                cmdShard.RegisterCommands<AccountCommands>();
                cmdShard.RegisterCommands<DevCommands>();
                cmdShard.SetHelpFormatter<DefaultHelpFormatter>();               
            }
            #endregion Registering          
            t.Stop();
            int registeredCommands = cnext.Values.Sum(x => x.RegisteredCommands.Count);           
            Log.Debug("Paimon has registered {Commands} main commands for {Shards} shards in {Time} ms"
                , registeredCommands, _Client.ShardClients.Count, t.ElapsedMilliseconds);
        }

        private Task InitiliazeClientEventHandlers()
        {
            _Client.Ready += ExceptionEventHandlers._Client_Ready;
            _Client.GuildAvailable += ExceptionEventHandlers._Client_GuildAvailable;
            _Client.GuildCreated += ExceptionEventHandlers._Client_GuildCreated;
            _Client.ClientErrored += ExceptionEventHandlers._Client_ClientErrored;
            _Client.SocketClosed += ExceptionEventHandlers._client_SocketClosed;
            return Task.CompletedTask;
        }

        private async Task InitializeActivity()
        {
            int TravelersNum = (int)SharedData.PaimonDB.GetTravelersCount(); 
            DiscordActivity act = new DiscordActivity()
            {
                ActivityType = ActivityType.Playing,
                Name = $"with {TravelersNum} Travelers! | p~help"
            };
            await _Client.UpdateStatusAsync(act);
            SharedData.PaimonDB.TravelerAdded += e_TravelerAdded;
        }

        async void e_TravelerAdded(object sender, EventArgs e)
        {
            int TravelersNum = (int)SharedData.PaimonDB.GetTravelersCount();
            DiscordActivity act = new DiscordActivity()
            {
                ActivityType = ActivityType.Playing,
                Name = $"with {TravelersNum} Travelers! | p~help"
            };
            await _Client.UpdateStatusAsync(act);
        }

        private async Task StartAsync()
        {
            Log.Information("[INIT] Paimon is entering Teyvat!");

            InitializeSharedData();           
            await InitiliazeClientEventHandlers();
            await InitializeCommandsNext();           
            // await InitializeSlashCommandsAsync();            
            await ExceptionEventHandlers.SubscribeToEventsAsnc(_Client);

            Log.Debug("[INIT] Paimon is connecting to Teyvat (Discord)...");
            await _Client.StartAsync();
            await InitializeActivity();
            Log.Debug("[INIT] Paimon connected to Teyvat (Discord)!");

            // Restart all the Timers
            var TravelerIDs = await SharedData.PaimonDB.GetTravelerIDs();
            foreach (var id in TravelerIDs)
            {
                var aTimer = new ResinTimer(id);
                aTimer.Start();
                SharedData.resinTimers.Add(aTimer);
            }
            Log.Information($"Started ResinTimer for {TravelerIDs.Count} Users: {string.Join(",", TravelerIDs)}");

        }

        private async void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            // Disable all the Timers
            SharedData.resinTimers.ForEach(x => x.StopAndDispose());
            Log.Information($"Stopped and Disposed ResinTimer for {SharedData.resinTimers.Count} Users: {string.Join(",", SharedData.resinTimers.Select(x => x._discordID))}");
            Log.Information("[SHUTDOWN] Paimon is now leaving Teyvat!");
            Log.Debug("[SHUTDOWN] Disconnecting from Teyvat (Discord) Gateway");
            await _Client.StopAsync();
            Log.Debug("[SHUTDOWN] Disconnected from Teyvat (Discord) Gateway");            
        }

        /*
        private Task InitializeSlashCommandsAsync()
        {
            Log.Information("Initializing Slash Commands");
            return Task.CompletedTask;
        }
        */

        private void InitializeSharedData()
        {
            SharedData.startTime = DateTime.Now;
            SharedData.prefixes = _prefixes;
            SharedData.botName = _BotName;
        }

    }
}
