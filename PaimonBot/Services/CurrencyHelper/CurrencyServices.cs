using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using PaimonBot.Extensions.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaimonBot.Services.CurrencyHelper
{
    public static class CurrencyServices
    {
        public static AdeptalEnergyLevel ParseAdeptalFromInt(int energyInput)
        {
            if (BetweenUnder(energyInput, AdeptalEnergyLevel.BareBones.GetAdeptalEnergyNeeded(), AdeptalEnergyLevel.HumbleAbode.GetAdeptalEnergyNeeded()))
                return AdeptalEnergyLevel.BareBones;
            if (BetweenUnder(energyInput, AdeptalEnergyLevel.HumbleAbode.GetAdeptalEnergyNeeded(), AdeptalEnergyLevel.Cozy.GetAdeptalEnergyNeeded()))
                return AdeptalEnergyLevel.HumbleAbode;
            if (BetweenUnder(energyInput, AdeptalEnergyLevel.Cozy.GetAdeptalEnergyNeeded(), AdeptalEnergyLevel.QueenSize.GetAdeptalEnergyNeeded()))
                return AdeptalEnergyLevel.Cozy;
            if (BetweenUnder(energyInput, AdeptalEnergyLevel.QueenSize.GetAdeptalEnergyNeeded(), AdeptalEnergyLevel.Elegant.GetAdeptalEnergyNeeded()))
                return AdeptalEnergyLevel.QueenSize;
            if (BetweenUnder(energyInput, AdeptalEnergyLevel.Elegant.GetAdeptalEnergyNeeded(), AdeptalEnergyLevel.Exquisite.GetAdeptalEnergyNeeded()))
                return AdeptalEnergyLevel.Elegant;
            if (BetweenUnder(energyInput, AdeptalEnergyLevel.Exquisite.GetAdeptalEnergyNeeded(), AdeptalEnergyLevel.Extraordinary.GetAdeptalEnergyNeeded()))
                return AdeptalEnergyLevel.Exquisite;
            if (BetweenUnder(energyInput, AdeptalEnergyLevel.Extraordinary.GetAdeptalEnergyNeeded(), AdeptalEnergyLevel.Stately.GetAdeptalEnergyNeeded()))
                return AdeptalEnergyLevel.Extraordinary;
            if (BetweenUnder(energyInput, AdeptalEnergyLevel.Stately.GetAdeptalEnergyNeeded(), AdeptalEnergyLevel.Luxury.GetAdeptalEnergyNeeded()))
                return AdeptalEnergyLevel.Stately;
            if (BetweenUnder(energyInput, AdeptalEnergyLevel.Luxury.GetAdeptalEnergyNeeded(), AdeptalEnergyLevel.FitForAKing.GetAdeptalEnergyNeeded()))
                return AdeptalEnergyLevel.Luxury;
            if (energyInput >= AdeptalEnergyLevel.FitForAKing.GetAdeptalEnergyNeeded())
                return AdeptalEnergyLevel.FitForAKing;
            else
                return AdeptalEnergyLevel.BareBones;
        }

        private static bool BetweenUnder(this int source, int a, int b)
        {
            return source >= a && source < b;
        }
    }
}