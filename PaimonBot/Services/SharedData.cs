using DSharpPlus.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using PaimonBot.Extensions;
using System;
using System.Collections.Generic;

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
        public static DiscordColor defaultColour = new DiscordColor("#2D325A"); // Paimon.moe Colour
        public static string TimedOutString = "Oopsie, you took too long to respond, Paimon isn't that patient! (〃´∀｀) Please try again!";
        #endregion SharedPaimonData
    }

    /// <summary>
    /// Static class filled with Emoji strings for the bot to use.
    /// </summary>
    public static class Emojis
    {
        public static string ResinEmote = "<:resin:889059442061082665>";
        public static string CurrencyEmote = "<:realm_currency:889065864924639252>";
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