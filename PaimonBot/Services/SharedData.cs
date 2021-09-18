using DSharpPlus.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace PaimonBot.Services
{
    /// <summary>
    /// A class consisting of basically static global/shared variables between classes.
    /// </summary>
    public class SharedData
    {
        #region SharedPaimonData
        public static List<string> prefixes; // Initialized in Bot class
        public static string logoURL = "https://cdn.discordapp.com/embed/avatars/0.png"; // Default logo URL
        public static string botName; // Initialized in Bot class
        public static IMongoDatabase PaimonDb;
        public static DateTime startTime;
        public static DiscordColor defaultColour = new DiscordColor("#2D325A"); // Paimon.moe Colour
        #endregion SharedPaimonData
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