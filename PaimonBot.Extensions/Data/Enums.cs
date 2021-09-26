using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace PaimonBot.Extensions.Data
{
    #region RealmTrustRankExtension
    public class TrustCurrencyCapacityAttr : Attribute
    {
        public int CurrencyCapacity;
        public TrustCurrencyCapacityAttr(int currencyCap)
        {
            CurrencyCapacity = currencyCap;
        }
    }

    public class TrustXPAttr : Attribute
    {
        public int TrustXP;
        public TrustXPAttr(int xp)
        {
            TrustXP = xp;
        }
    }

    #endregion RealmTrustRankExtension

    #region AdeptalEnergyLvlExtension
    public class AdeptalEnergyNeededAttr : Attribute
    {
        public int AdeptalEnergyNeeded;
        public AdeptalEnergyNeededAttr(int xp)
        {
            AdeptalEnergyNeeded = xp;
        }
    }

    public class CurrencyRechargeRateAttr : Attribute
    {
        /// <summary>
        /// Currency Recharge at that Adeptal Level per Hour
        /// </summary>
        public int CurrencyRechargeRate;
        public CurrencyRechargeRateAttr(int rate)
        {
            CurrencyRechargeRate = rate;
        }
    }

    #endregion AdeptalEnergyLvlExtension
    /// <summary>
    /// Traveler's Realm Trust Rank
    /// </summary>
    public enum RealmTrustRank // Might want to use A Proper Class later on {Containing Rank xp, etc}
    {
        [BsonRepresentation(BsonType.Int32)]
        [TrustCurrencyCapacityAttr(300)]
        [TrustXPAttr(0)]
        Rank1 = 1,

        [BsonRepresentation(BsonType.Int32)]
        [TrustCurrencyCapacityAttr(600)]
        [TrustXPAttr(300)]
        Rank2 = 2,

        [BsonRepresentation(BsonType.Int32)]
        [TrustCurrencyCapacityAttr(900)]
        [TrustXPAttr(600)]
        Rank3 = 3,

        [BsonRepresentation(BsonType.Int32)]
        [TrustCurrencyCapacityAttr(1200)]
        [TrustXPAttr(1000)]
        Rank4 = 4,

        [TrustCurrencyCapacityAttr(1400)]
        [TrustXPAttr(1500)]
        [BsonRepresentation(BsonType.Int32)]
        Rank5 = 5,

        [TrustCurrencyCapacityAttr(1600)]
        [TrustXPAttr(1500)]
        [BsonRepresentation(BsonType.Int32)]
        Rank6 = 6,

        [TrustCurrencyCapacityAttr(1800)]
        [TrustXPAttr(1500)]
        [BsonRepresentation(BsonType.Int32)]
        Rank7 = 7,

        [TrustCurrencyCapacityAttr(2000)]
        [TrustXPAttr(1500)]
        [BsonRepresentation(BsonType.Int32)]
        Rank8 = 8,

        [TrustCurrencyCapacityAttr(2200)]
        [TrustXPAttr(1500)]
        [BsonRepresentation(BsonType.Int32)]
        Rank9 = 9,

        [TrustCurrencyCapacityAttr(2400)]
        [TrustXPAttr(1500)]
        [BsonRepresentation(BsonType.Int32)]
        Rank10 = 10
    }

    /// <summary>
    /// Traveler's Realm Adeptal Energy (to determine Currency Recharge)
    /// </summary>
    public enum AdeptalEnergyLevel
    {
        [BsonRepresentation(BsonType.String)]   
        [CurrencyRechargeRateAttr(4)]
        [AdeptalEnergyNeededAttr(0)]
        BareBones = 0,
        [CurrencyRechargeRateAttr(8)]
        [AdeptalEnergyNeededAttr(2000)]
        [BsonRepresentation(BsonType.String)]
        HumbleAbode = 1,
        [CurrencyRechargeRateAttr(12)]
        [AdeptalEnergyNeededAttr(3000)]
        [BsonRepresentation(BsonType.String)]
        Cozy = 2,
        [CurrencyRechargeRateAttr(16)]
        [AdeptalEnergyNeededAttr(4500)]
        [BsonRepresentation(BsonType.String)]
        QueenSize = 3,
        [CurrencyRechargeRateAttr(20)]
        [AdeptalEnergyNeededAttr(6000)]
        [BsonRepresentation(BsonType.String)]
        Elegant = 4,
        [CurrencyRechargeRateAttr(22)]
        [AdeptalEnergyNeededAttr(8000)]
        [BsonRepresentation(BsonType.String)]
        Exquisite = 5,
        [CurrencyRechargeRateAttr(24)]
        [AdeptalEnergyNeededAttr(10000)]
        [BsonRepresentation(BsonType.String)]
        Extraordinary = 6,
        [CurrencyRechargeRateAttr(26)]
        [AdeptalEnergyNeededAttr(12000)]
        [BsonRepresentation(BsonType.String)]
        Stately = 7,
        [CurrencyRechargeRateAttr(28)]
        [AdeptalEnergyNeededAttr(15000)]
        [BsonRepresentation(BsonType.String)]
        Luxury = 8,
        [CurrencyRechargeRateAttr(30)]
        [AdeptalEnergyNeededAttr(20000)]
        [BsonRepresentation(BsonType.String)]
        FitForAKing = 9
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
