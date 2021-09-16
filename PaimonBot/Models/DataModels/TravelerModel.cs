using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PaimonBot.Services;

namespace PaimonBot.DataModels
{
    public class Traveler
    {
        /// <summary>
        /// BSON Id to Identify the Unique Document
        /// </summary>
        [BsonId]
        public ObjectId Id { get; set; }

        #region TravelerData
        /// <summary>
        /// Traveler's Discord ID
        /// </summary>
        [BsonElement("DiscordID")]
        public ulong DiscordID { get; set; }
        /// <summary>
        /// The Discord Guild ID where the Traveler first initialized their data
        /// </summary>
        [BsonElement("GuildID")]
        public ulong GuildID { get; set; }
        #endregion TravelerData

        #region ResinData
        /// <summary>
        /// The Time in which the ResinAmount is updated in the database.
        /// </summary>
        [BsonElement("ResinUpdated")]
        public BsonDateTime ResinUpdatedTime { get; set; } = null;
        /// <summary>
        /// Traveler's Resin amount
        /// </summary>
        [BsonElement("ResinAmount")]
        public int ResinAmount { get; set; } = 0;

        #endregion ResinData

        #region RealmCurrencyData
        /*
         * Do not know whether to implement this or not yet, looking into it, in the near future
        public int RealmCurrency { get; set; } = 0;
        public BsonDateTime CurrencyUpdated { get; set; } = null;
        */
        #endregion RealmCurrencyData

        #region TeyvatData
        /// <summary>
        /// Traveler's Realm Trust Rank
        /// </summary>
        [BsonElement("RealmTrustRank")]
        public RealmTrustRank RealmTrustRank { get; set; } = RealmTrustRank.Rank1;

        /// <summary>
        /// Traveler's Genshin Server
        /// </summary>
        [BsonElement("TeyvatServer")]
        public TeyvatServer GenshinServer { get; set; }

        /// <summary>
        /// Traveler's World Level
        /// </summary>
        [BsonElement("TeyvatLevel")]
        public WorldLevel WorldLevel { get; set; } = WorldLevel.WL0;
        #endregion TeyvatData
    }
}