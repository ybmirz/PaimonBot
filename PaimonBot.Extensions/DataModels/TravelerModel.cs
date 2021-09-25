using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PaimonBot.Extensions.Data;
using System;

namespace PaimonBot.Extensions.DataModels
{
    public class Traveler
    {
        /// <summary>
        /// BSON Id to Identify the Unique Document
        /// </summary>
        [BsonId]
        public BsonObjectId Id { get; set; }

        #region TravelerData
        /// <summary>
        /// Traveler's Discord ID
        /// </summary>
        [BsonElement("DiscordID")]
        public ulong DiscordID { get; set; }
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
        public int ResinAmount { get; set; } = int.MinValue;

        #endregion ResinData

        #region RealmCurrencyData
        /// <summary>
        /// Traveler's Current Realm Currency
        /// </summary>
        [BsonElement("RealmCurrency")]
        public int RealmCurrency { get; set; } = int.MinValue;
        /// <summary>
        /// When the Traveler set their Currency
        /// </summary>
        [BsonElement("RealmCurrencyUpdated")]
        public BsonDateTime CurrencyUpdated { get; set; } = null;

        #endregion RealmCurrencyData

        #region TeyvatData
        [BsonElement("ParaGadgetUsed")]
        public BsonDateTime ParaGadget { get; set; } = null;

        /// <summary>
        /// Traveler's Realm Trust Rank
        /// </summary>
        [BsonElement("RealmTrustRank")]
        public RealmTrustRank RealmTrustRank { get; set; } = RealmTrustRank.Rank1;

        /// <summary>
        /// Traveler's Genshin Server
        /// </summary>
        [BsonElement("TeyvatServer")]
        public TeyvatServer GenshinServer { get; set; } = TeyvatServer.NorthAmerica;

        /// <summary>
        /// Traveler's World Level
        /// </summary>
        [BsonElement("TeyvatLevel")]
        public WorldLevel WorldLevel { get; set; } = WorldLevel.WL0;
        #endregion TeyvatData
    }
}