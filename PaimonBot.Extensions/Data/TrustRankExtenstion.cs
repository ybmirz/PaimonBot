using System;
using System.Collections.Generic;
using System.Text;

namespace PaimonBot.Extensions.Data
{
    public static class TrustRankExtenstion
    {
        public static int GetTrustRankCurrencyCap(this Enum value)
        {
            var type = value.GetType();

            string name = Enum.GetName(type, value);
            if (name == null) { return int.MinValue; }

            var field = type.GetField(name);
            if (field == null) { return int.MinValue; }

            var attr = Attribute.GetCustomAttribute(field, typeof(TrustCurrencyCapacityAttr)) as TrustCurrencyCapacityAttr;
            if (attr == null) { return int.MinValue; }

            return attr.CurrencyCapacity;
        }

        public static int GetTrustRankXp(this Enum value)
        {
            var type = value.GetType();

            string name = Enum.GetName(type, value);
            if (name == null) { return int.MinValue; }

            var field = type.GetField(name);
            if (field == null) { return int.MinValue; }

            var attr = Attribute.GetCustomAttribute(field, typeof(TrustXPAttr)) as TrustXPAttr;
            if (attr == null) { return int.MinValue; }

            return attr.TrustXP;
        }
    }
}
