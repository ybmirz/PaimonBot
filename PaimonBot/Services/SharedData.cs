using DSharpPlus.Entities;
using PaimonBot.Extensions;
using PaimonBot.Services.CurrencyHelper;
using PaimonBot.Services.ResinHelper;
using System;
using System.Collections.Generic;
using System.Timers;

namespace PaimonBot.Services
{
    /// <summary>
    /// A class consisting of basically static global/shared variables between classes.
    /// </summary>
    public static class SharedData
    {
        #region SharedPaimonData
        public static List<string> prefixes; // Initialized in Bot class
        public static string logoURL = "https://cdn.discordapp.com/embed/avatars/0.png"; // Default logo URL
        public static string botName; // Initialized in Bot class
        public static PaimonDb PaimonDB;
        public static DateTime startTime;
        public static DiscordColor defaultColour = new DiscordColor("#6AE5F7"); // Paimon.moe Colour
        public static string TimedOutString = "Oopsie, you took too long to respond, Paimon isn't that patient! (〃´∀｀) Please try again!";
        #endregion SharedPaimonData

        #region SharedResinData
        public static List<ResinTimer> resinTimers = new List<ResinTimer>();
        #endregion SharedResinData

        #region SharedCurrencyData
        public static List<RealmCurrencyTimer> currencyTimer = new List<RealmCurrencyTimer>();
        #endregion SharedCurrencyData

        #region SharedGadgetUsers
        public static Dictionary<ulong, DiscordChannel> ParaReminderUsersDMs = new Dictionary<ulong, DiscordChannel>();
        public static Dictionary<ulong, DateTimeOffset> ParaRemindedUsers = new Dictionary<ulong, DateTimeOffset>();
        public static Timer GadgetTimer = new Timer();
        #endregion SharedGadgetUsers

        #region Reminders
        public static Dictionary<ulong, Dictionary<string,Timer>> Reminders = new Dictionary<ulong, Dictionary<string, Timer>>();
        public static int MaxReminder = 5;
        public static Dictionary<string, DateTimeOffset> ReminderEndTimes = new Dictionary<string, DateTimeOffset>();
        #endregion Reminders

        #region GlobalRandomization
        public static Random Random = new Random();
        #endregion GlobalRandomization
    }

    /// <summary>
    /// Static class filled with Emoji strings for the bot to use.
    /// </summary>
    public static class Emojis
    {
        public static string ResinEmote = "<:resin:889059442061082665>";
        public static string CurrencyEmote = "<:realm_currency:889065864924639252>";
        public static string HappyEmote = "<:paimon_happy:891661694823170058>";
        public static string CryEmote = "<:paimon_cry:891661694558949416>";
        public static string BlurpEmote = "<:paimon_blurp:891662650507948052>";
        public static string AdeptalEmote = "<:adeptal_energy:892438618164039680>";
    }

    /// <summary>
    /// CommandGrouping enum to beautify Help
    /// </summary>
    public enum CategoryName
    {
        Account,
        Info,
        Misc
    }

    /// <summary>
    /// Enum of Response Type for Discord Responses/Embeds
    /// </summary>
    public enum ResponseType
    {
        Warning,
        Error,
        Missing,
        Default
    }
    
}