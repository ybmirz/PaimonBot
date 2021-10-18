using PaimonBot.Extensions.Data;
using PaimonBot.Extensions.DataModels;
using Serilog;
using System;
using System.Timers;

namespace PaimonBot.Services.CurrencyHelper
{
    public class RealmCurrencyTimer : IDisposable
    {
        public ulong DiscordId { get; private set; }
        private RealmTrustRank _trustRank { get; set; }
        private AdeptalEnergyLevel _adeptalenergy { get; set; }
        private Timer _timer { get; set; }

        public RealmCurrencyTimer(ulong discordId, RealmTrustRank trustRank, AdeptalEnergyLevel adeptalEnergy)
        {
            DiscordId = discordId;
            _trustRank = trustRank;
            _adeptalenergy = adeptalEnergy;
            _timer = new Timer(TimeSpan.FromHours(1).TotalMilliseconds);
            _timer.AutoReset = true;
            _timer.Elapsed += _timer_Elapsed;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var traveler = SharedData.PaimonDB.GetTravelerBy("DiscordID", DiscordId);
            if (traveler != null)
            {
                var maxCurrency = _trustRank.GetTrustRankCurrencyCap();
                var added = _adeptalenergy.GetAdeptalCurrencyRechargeRate();
                traveler.RealmCurrency += added;
                if (traveler.RealmCurrency > maxCurrency)
                    traveler.RealmCurrency = maxCurrency;
                SharedData.PaimonDB.UpdateTraveler(traveler, "RealmCurrency", traveler.RealmCurrency);
                Log.Information($"Succesfully added `{added}` realm currency ({_adeptalenergy}) to Traveler {DiscordId} at {DateTime.UtcNow} (UTC)");
                if (traveler.RealmCurrency == _trustRank.GetTrustRankCurrencyCap()) // There's no need to do, as it has capped.
                {
                    CurrencyCappedEventArgs args = new CurrencyCappedEventArgs();
                    args.cappedTime = DateTime.UtcNow;
                    args.currencyTimer = _timer;
                    args.DiscordId = DiscordId;
                    OnCurrencyCapped(args);
                    return;
                }
            }
            else
                throw new InvalidOperationException($"Traveler {DiscordId} does not seem to exist whilst trying to update realm currency!");
        }

        public void Start()
        {
            _timer.Start();
        }

        public void StopAndDispose()
        {
            _timer.Stop();
            _timer.Dispose();
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        protected virtual void OnCurrencyCapped(CurrencyCappedEventArgs e)
        {
            EventHandler<CurrencyCappedEventArgs> handler = CurrencyCapped;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<CurrencyCappedEventArgs> CurrencyCapped;
    }

    public class CurrencyCappedEventArgs : EventArgs
    {
        public Timer currencyTimer { get; set; }
        public DateTime cappedTime { get; set; }
        public ulong DiscordId { get; set; }
    }
}