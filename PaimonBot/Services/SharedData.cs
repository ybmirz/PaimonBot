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

    /// <summary>
    /// Traveler's Realm Trust Rank
    /// </summary>
    public enum RealmTrustRank // Might want to use A Proper Class later on {Containing Rank xp, etc}
    {
        [BsonRepresentation(BsonType.Int32)]
        Rank1 = 1,
        [BsonRepresentation(BsonType.Int32)]
        Rank2 = 2,
        [BsonRepresentation(BsonType.Int32)]
        Rank3 = 3,
        [BsonRepresentation(BsonType.Int32)]
        Rank4 = 4,
        [BsonRepresentation(BsonType.Int32)]
        Rank5 = 5,
        [BsonRepresentation(BsonType.Int32)]
        Rank6 = 6,
        [BsonRepresentation(BsonType.Int32)]
        Rank7 = 7,
        [BsonRepresentation(BsonType.Int32)]
        Rank8 = 8,
        [BsonRepresentation(BsonType.Int32)]
        Rank9 = 9,
        [BsonRepresentation(BsonType.Int32)]
        Rank10 = 10
    }

    /// <summary>
    /// Traveler's Teyvat Server (Genshin Server)
    /// </summary>
    public enum TeyvatServer
    {
        [BsonRepresentation(BsonType.String)]
        NorthAmerica = 1,
        [BsonRepresentation(BsonType.String)]
        Europe = 2,
        [BsonRepresentation(BsonType.String)]
        Asia = 3,
        [BsonRepresentation(BsonType.String)]
        TWHKMO = 4
    }

    /// <summary>
    /// Traveler's World level
    /// </summary>
    public enum WorldLevel // Might want to use a proper class as well {Containing Ascension Level, etc}
    {
        [BsonRepresentation(BsonType.Int32)]
        WL0 = 0,
        [BsonRepresentation(BsonType.Int32)]
        WL1 = 1,
        [BsonRepresentation(BsonType.Int32)]
        WL2 = 2,
        [BsonRepresentation(BsonType.Int32)]
        WL3 = 3,
        [BsonRepresentation(BsonType.Int32)]
        WL4 = 4,
        [BsonRepresentation(BsonType.Int32)]
        WL5 = 5,
        [BsonRepresentation(BsonType.Int32)]
        WL6 = 6,
        [BsonRepresentation(BsonType.Int32)]
        WL7 = 7,
        [BsonRepresentation(BsonType.Int32)]
        WL8 = 8
    }
}