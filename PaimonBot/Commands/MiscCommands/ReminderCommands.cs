using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using PaimonBot.Services;
using PaimonBot.Services.HelpFormatter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace PaimonBot.Commands
{
    [Group("reminder"), Aliases("remind")]
    [Description("A set of Reminder Commands for PaimonBot Travelers! Create general reminders to help you in your journeys")]
    [Category(CategoryName.Misc)]
    public class ReminderCommands : BaseCommandModule
    {
        [GroupCommand]
        [Description("Instantly starts a new reminder for the user. Max amount of 5 reminders per user.")]
        [Cooldown(1,3 ,CooldownBucketType.User)]
        public async Task Default(CommandContext ctx, string time, [RemainingText] string reason)
        {
            if (SharedData.Reminders.ContainsKey(ctx.User.Id)) // User already has a reminder list
            {
                
            }
            else
            {
                // Not in Cached Reminder List so create new dictionary of timers.
                Dictionary<string, Timer> reminders = new Dictionary<string, Timer>();
                Regex rx = new Regex(@"((?<number>\d+(?:[.,]\d+)?)(?<letter>[wdhms]))+", RegexOptions.IgnoreCase);
                if (!rx.IsMatch(time))
                {
                    await PaimonServices.SendEmbedToChannelAsync(ctx.Channel, "Invalid Input Error", "Sorry, Paimon can't seem to understand that time input, please try again!",
                        TimeSpan.FromSeconds(5), ResponseType.Warning);
                    return;
                }
                var matches = rx.Matches(time)
                    .Cast<Match>()
                    .Where(m => m.Groups["number"].Success && m.Groups["letter"].Success)
                    .ToList();
                TimeSpan day = TimeSpan.Zero, hours = TimeSpan.Zero, minutes = TimeSpan.Zero, seconds = TimeSpan.Zero;
                foreach (Match match in matches)
                {
                    switch (match.Groups["letter"].ToString().ToLower())
                    {
                        case "w": // Basically just adds days to it from number of weeks
                            day += TimeSpan.FromDays(double.Parse(match.Groups["number"].ToString()) * 7);
                            break;
                        case "d":
                            day += TimeSpan.FromDays(double.Parse(match.Groups["number"].ToString()));
                            break;
                        case "h":
                            hours += TimeSpan.FromHours(double.Parse(match.Groups["number"].ToString()));
                            break;
                        case "m":
                            minutes += TimeSpan.FromMinutes(double.Parse(match.Groups["number"].ToString()));
                            break;
                        case "s":
                            seconds += TimeSpan.FromSeconds(double.Parse(match.Groups["number"].ToString()));
                            break;
                        default:
                            break;
                    }
                }       
                TimeSpan totalTime = day.Add(hours.Add(minutes.Add(seconds)));
                DateTimeOffset remindTime = DateTime.UtcNow + totalTime;
                // Caching Data
                string guid = Guid.NewGuid().ToString().Substring(0, 6);
                Timer timer = new Timer(totalTime.TotalMilliseconds);
                timer.Elapsed += (sender, e) => PaimonServices.ATimer_ReminderEndAsync(sender, e, ctx.User, guid, ctx.Channel, reason);
                timer.AutoReset = false;                
                timer.Start();
                reminders.Add(guid,timer);
                SharedData.Reminders.Add(ctx.User.Id, reminders);
                await ctx.RespondAsync($"{ctx.User.Mention} A new Reminder ID#{guid} has been made! You will be reminded in this channel after {totalTime.Days} day(s), {totalTime.Hours} hour(s), " +
                    $"{totalTime.Minutes} min(s) and {totalTime.Seconds} second(s), at <t:{remindTime.ToUnixTimeSeconds()}>. You will be reminded of: {reason}.").ConfigureAwait(false);
            }
        }
    }
}