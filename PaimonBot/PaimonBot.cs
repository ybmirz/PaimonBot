using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using PaimonBot.Commands;
using PaimonBot.Extensions.Data;
using PaimonBot.Models;
using PaimonBot.Services;
using PaimonBot.Services.CurrencyHelper;
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
                cmdShard.RegisterCommands<CurrencyCommands>();
                cmdShard.RegisterCommands<ResinCommands>();
                cmdShard.RegisterCommands<AccountCommands>();
                cmdShard.RegisterCommands<ParaGadgetCommand>();
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

            // Restart Resin and Currency Timers
            var travelersInDB = await SharedData.PaimonDB.GetTravelersAsync();
            List<ulong> travelerswCurrencyTimer = new List<ulong>();
            List<ulong> travelerswResinTimer = new List<ulong>();
            foreach (var traveler in travelersInDB)
            {
                // IF it's been more than 8 minutes and has not been added.
                if (((DateTime.Now - traveler.ResinUpdatedTime.ToLocalTime()) >= TimeSpan.FromMinutes(8)) && traveler.ResinAmount < 160)
                {
                    var resinCarry = (DateTime.Now - traveler.ResinUpdatedTime.ToLocalTime()).TotalMinutes / 8;
                    traveler.ResinAmount += Convert.ToInt32(Math.Floor(resinCarry));
                    traveler.ResinUpdatedTime = DateTime.UtcNow;
                    SharedData.PaimonDB.ReplaceTraveler(traveler);
                }
                if (traveler.ResinAmount < 160)
                {                    
                    var aTimer = new ResinTimer(traveler.DiscordID);
                    aTimer.Start();
                    aTimer.ResinCapped += PaimonServices.ATimer_ResinCapped;                    
                    SharedData.resinTimers.Add(aTimer);
                    travelerswResinTimer.Add(traveler.DiscordID);                    
                }              
                // If the traveler does have currency info
                if (traveler.RealmCurrency != int.MinValue && traveler.CurrencyUpdated != null)
                {
                    var maxCurrency = traveler.RealmTrustRank.GetTrustRankCurrencyCap();
                    if (traveler.RealmCurrency >= maxCurrency)
                        continue; 
                    var adpetalEnergy = CurrencyServices.ParseAdeptalFromInt(traveler.AdeptalEnergy);
                    var cTimer = new RealmCurrencyTimer(traveler.DiscordID, traveler.RealmTrustRank, adpetalEnergy);
                    cTimer.Start();
                    cTimer.CurrencyCapped += PaimonServices.ATimer_CurrencyCapped;
                    SharedData.currencyTimer.Add(cTimer);
                    travelerswCurrencyTimer.Add(traveler.DiscordID);
                    SharedData.PaimonDB.UpdateTraveler(traveler, "RealmCurrencyUpdated", DateTime.UtcNow);
                }
                // If the traveler has ParaGadget value = meaning they'll use it 
                if (traveler.ParaGadgetNextUse != null)
                {
                    if (traveler.ParaGadgetRemind)
                       SharedData.ParaRemindedUsers.Add(traveler.DiscordID, traveler.ParaGadgetNextUse.ToUniversalTime());
                }
            }
            Log.Information($"Started CurrencyTimers for {travelerswCurrencyTimer.Count} Users: {string.Join(",", travelerswCurrencyTimer)}");
            Log.Information($"Started ResinTimer for {travelerswResinTimer.Count} Users: {string.Join(",", travelerswResinTimer)}");            

            // Start ParametricGadget Timer    
            SharedData.GadgetTimer.Interval = TimeSpan.FromHours(1).TotalMilliseconds;
            SharedData.GadgetTimer.Elapsed += GadgetTimer_Elapsed;
            SharedData.GadgetTimer.AutoReset = true;
            SharedData.GadgetTimer.Start();
            Log.Information($"Started GadgetTimer for {SharedData.ParaRemindedUsers.Count} Members in list; {string.Join(",", SharedData.ParaRemindedUsers.Keys)}");
        }

        private async void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            // Disable all the Timers
            SharedData.resinTimers.ForEach(x => x.StopAndDispose());           
            Log.Information($"Stopped and Disposed ResinTimers for {SharedData.resinTimers.Count} Users: {string.Join(",", SharedData.resinTimers.Select(x => x._discordID))}");
            SharedData.currencyTimer.ForEach(x => x.StopAndDispose());
            Log.Information($"Stopped and Disposed CurrencyTimers for {SharedData.currencyTimer.Count} Users: {string.Join(",", SharedData.currencyTimer.Select(x => x.DiscordId))}");
            SharedData.GadgetTimer.Stop();
            SharedData.GadgetTimer.Dispose();
            Log.Information($"Stopped and Disposed Global GadgetReminderTimer. Users to be Reminded: {string.Join(",", SharedData.ParaRemindedUsers.Keys)}");
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

        private async void GadgetTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            foreach (var pair in SharedData.ParaRemindedUsers)
            {
                if (pair.Value.Date == DateTime.UtcNow.Date && pair.Value.Hour == DateTime.UtcNow.Hour)
                {
                    try
                    {
                        // Notifies the User
                        var dmChannel = SharedData.ParaReminderUsersDMs[pair.Key];
                        await dmChannel.SendMessageAsync($"Hi there Traveler! Paimon was told you can now use your **Parametric Gadget**! Be sure to use `{SharedData.prefixes[0]}gadget use` when you use your gadget in-game to tell Paimon about it too!").ConfigureAwait(false);

                        // Deletes from the list and updates the traveler                        
                        var traveler = SharedData.PaimonDB.GetTravelerBy("DiscordID", pair.Key);
                        if (traveler == null)
                        { Log.Error("Traveler {Id} does not seem to exist in the database, whilst their GadgetRemind exists in the listt", pair.Key); continue; }
                        traveler.ParaGadgetNextUse = null;
                        traveler.ParaGadgetRemind = false;
                        SharedData.PaimonDB.ReplaceTraveler(traveler);
                        SharedData.ParaRemindedUsers.Remove(pair.Key);
                        SharedData.ParaReminderUsersDMs.Remove(pair.Key);

                        // Logging
                        Log.Information("Succesfully reminded Traveler {Id} to use their Parametric Gadget!", pair.Value);
                    }
                    catch (Exception x)
                    { Log.Warning("Oopsie, a Gadget Reminder failed to remind User {id}. Exception: {Msg} {StackTrace}", pair.Key, x.Message, x.StackTrace); }
                }
            }
        }

    }
}
