using DSharpPlus.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PaimonBot.Services.ResinHelper
{
    public class ResinTimer : IDisposable
    {   public ulong _discordID { get; private set; }
        private Timer _timer { get; set; }
        public bool _remind { get; set; }
        public int _remindAt { get; private set; }
        private DiscordChannel _notifyChannel { get; set; } = null;

        /// <summary>
        /// A new Instance of a ResinTimer
        /// </summary>
        /// <param name="discordID">DiscordId attributed to the Timer</param>
        /// <param name="remind">Whether to remind at a specific resin, defaults to False</param>
        /// <param name="remindAt">At which amount to remind user, defaults to Null/MinVAL</param>
        public ResinTimer(ulong discordID, bool remind = false, int remindAt = int.MinValue)
        {
            _discordID = discordID;
            _remind = remind;
            _remindAt = remindAt;
            _timer = new Timer(480000);
            _timer.AutoReset = true;
            _timer.Elapsed += _timer_Elapsed;
        }

        private async void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var traveler = SharedData.PaimonDB.GetTravelerBy("DiscordID", _discordID);
            if (traveler != null)
            {
                traveler.ResinAmount++;
                if (traveler.ResinAmount > 160)
                traveler.ResinAmount = 160;
                if (traveler.ResinAmount < 0)
                traveler.ResinAmount = 0;

                // No need to update Time when Resin = 160
                if (traveler.ResinAmount != 160)
                    traveler.ResinUpdatedTime = DateTime.UtcNow;

                // Notifies if it reached the specified amount
                if (traveler.ResinAmount == _remindAt && _remind)
                {
                    await NotifyUser();
                    _remind = false;
                    _remindAt = int.MinValue;
                }

                SharedData.PaimonDB.UpdateTraveler(traveler, "ResinAmount", traveler.ResinAmount);
                Log.Information($"Succesfully added resin to Traveler {_discordID} at {DateTime.UtcNow} (UTC)");
            }
            else
                throw new InvalidOperationException($"Traveler {_discordID} does not seem to exist whilst trying to update resin!");
        }

        private async Task NotifyUser()
        {
            try
            {

            }
            catch (Exception e)
            { }
        }

        /// <summary>
        /// Sets the DM Channel to send the notification to
        /// </summary>
        /// <param name="channel">Member's DM Channel</param>
        public void SetChannel(DiscordChannel channel)
        {
            _notifyChannel = channel;
        }
       
        public void Start()
        {
            _timer.Start();
        }

        public void StopAndDispose()
        {           
            _timer.Stop();
            _timer.Dispose();
            this.Dispose();
        }

        public void Dispose()
        {
            _timer.Dispose();
            this.Dispose();
        }
    }
}