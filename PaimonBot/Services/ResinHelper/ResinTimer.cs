using DSharpPlus.Entities;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace PaimonBot.Services.ResinHelper
{
    public class ResinTimer : IDisposable
    {   public ulong _discordID { get; private set; }
        private Timer _timer { get; set; }
        public bool _remind { get; private set; }
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

                // Notifies if it reached the specified amount and true     
                if (traveler.ResinAmount == _remindAt && _remind)
                {
                    await NotifyUser();
                    _remind = false;
                    _remindAt = int.MinValue;
                }

                SharedData.PaimonDB.UpdateTraveler(traveler, "ResinAmount", traveler.ResinAmount);
                SharedData.PaimonDB.UpdateTraveler(traveler, "ResinUpdated", traveler.ResinUpdatedTime);
                Log.Information($"Succesfully added resin to Traveler {_discordID} at {DateTime.UtcNow} (UTC)");

                // If Resin Capped, throw Invoke the Event
                if (traveler.ResinAmount >= 160)
                {
                    ResinCappedEventArgs args = new ResinCappedEventArgs();
                    args.DiscordId = _discordID;
                    args.CappedTime = DateTime.UtcNow;
                    args.resinTimer = _timer;
                    OnResinCapped(args);
                    return;
                }

            }
            else
                throw new InvalidOperationException($"Traveler {_discordID} does not seem to exist whilst trying to update resin!");
        }

        private async Task NotifyUser()
        {
            try
            {
                string msg = string.Empty;
                if (_remindAt == 160)
                    msg = $"Traveler! Paimon's here to remind you that your Resin is now full (**{_remindAt}**/160 {Emojis.ResinEmote})! You can now grind as much as you'd like! {Emojis.HappyEmote}";
                else
                    msg = $"Traveler! Paimon's here to remind you that your Resin has now reached **{_remindAt}**/160 {Emojis.ResinEmote}! Use it soon enough! {Emojis.BlurpEmote}";
                await _notifyChannel.SendMessageAsync(msg).ConfigureAwait(false);
                Log.Information("Succesfully reminded traveler {Id} at {Resin} resin.", _discordID, _remindAt);
            }
            catch (Exception e)
            { Log.Warning("Oopsie, a Resin Reminder failed to remind User {Id}. Exception: {Msg} {StackTrace}", _discordID, e.Message, e.StackTrace); }
        }

        /// <summary>
        /// Enables the Remind Function of the Timer
        /// </summary>
        /// <param name="remindAt">At how much resin to remind User</param>
        /// <param name="channel">Channel to send the Notification</param>
        public void EnableRemind(int remindAt, DiscordChannel channel)
        {
            _remind = true;
            _remindAt = remindAt;
            _notifyChannel = channel;
        }

        /// <summary>
        /// Disables the Remind Function of the Timer
        /// </summary>
        public void DisableRemind()
        {
            _remind = false;
            _remindAt = int.MinValue;
            _notifyChannel = null;
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

        protected virtual void OnResinCapped(ResinCappedEventArgs e)
        {
            EventHandler<ResinCappedEventArgs> handler = ResinCapped;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<ResinCappedEventArgs> ResinCapped;
    }

    public class ResinCappedEventArgs : EventArgs
    {
        public Timer resinTimer { get; set; }
        public DateTime CappedTime { get; set; }
        public ulong DiscordId { get; set; }
    }
}