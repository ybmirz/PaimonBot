using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Text;
using MongoDB.Driver;
using Newtonsoft.Json;
using Serilog;
using PaimonBot.Models;
using PaimonBot.Services;
using Microsoft.AspNetCore.Builder;
using PaimonBot.Extensions;
using PaimonBot.Extensions.DataModels;

namespace PaimonBot
{
    class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Debug()
                .WriteTo.File("./Logs/Log.txt", shared: true, 
                    rollingInterval: RollingInterval.Hour, retainedFileCountLimit: null,
                    flushToDiskInterval: TimeSpan.FromMinutes(1), restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)                
                .CreateLogger();
            Log.Information("Thank you for starting PaimonBot v1.0! " +
                "Starting Host application...");
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// WebHost Builder to startup the console application (bot)
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseUrls(""); // Add listening ports here
            });
    }

    public class Startup
    {
        /// <summary>
        /// The startup class when starting the Bot Application
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services) 
        {
            var serviceProvider = services.BuildServiceProvider();

            // Read from CelestiaConfig.json (Bot Configuration)         
            Log.Information("Getting Celestia Config...");
            var Celestiajson = string.Empty;
            using (var fs = File.OpenRead("./Resources/Celestia/BotConfig.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false))) // Read as IS
            {
                Celestiajson = sr.ReadToEnd();
            }
            var CelestiaConfig = JsonConvert.DeserializeObject<BotConfigurationModel>(Celestiajson);

            // A few processes that wants to be done when starting or when ending.
            CurrentDomain_ProcessStart();
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            // Connecting to mongodb
            Log.Information("Connecting to PaimonDb...");
            string dbJson = string.Empty;
            using (var fs = File.OpenRead("./Resources/Celestia/DbCredentials.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
            {
                dbJson = sr.ReadToEnd();
            }
            var dbCred = JsonConvert.DeserializeObject<DbConfigModel>(dbJson);
            SharedData.PaimonDB = new PaimonDb($"mongodb://{dbCred.Username}:{dbCred.Password}@{dbCred.Host}/{dbCred.database}", dbCred.database);            
            Log.Information($"[SUCCESS] Connected to PaimonDb! " +
                $"Database: \"{SharedData.PaimonDB.GetDBInstance().DatabaseNamespace.DatabaseName}\" on \"{SharedData.PaimonDB.GetDBInstance().Client.Settings.Server}\"");
            // ^ Ensuring that database is connected properly

            // Creating PaimonBot and running it as a service
            var Paimonbot = new PaimonBot(CelestiaConfig, serviceProvider);
            services.AddSingleton(Paimonbot);           
        }

        /// <summary>
        /// Void to be called when bot is closing down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {        
            Log.Information("[SHUTDOWN] PaimonBot is now going to sleep! Thanks for the fun!");            
        }

        /// <summary>
        /// Void called when the application is starting up
        /// </summary>
        private void CurrentDomain_ProcessStart()
        {
            Log.Information("[INIT] PaimonBot is waking up...");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }
    }
}
