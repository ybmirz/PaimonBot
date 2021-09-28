using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using PaimonBot.Extensions.Data;
using PaimonBot.Extensions.DataModels;
using PaimonBot.Services;
using PaimonBot.Services.CurrencyHelper;
using PaimonBot.Services.HelpFormatter;
using PaimonBot.Services.ResinHelper;
using Serilog;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PaimonBot.Commands
{
    [Group("account"), Aliases("acc", "dashboard")]
    [Description("A set of Traveler Account Commands for PaimonBot Travelers! Set your Genshin Data so Paimon can help you on your journey!")]
    [Category(CategoryName.Account)]
    public class AccountCommands : BaseCommandModule
    {
        [GroupCommand]
        [Description("Shows the Embed Dashboard consisting Traveler's Account Info.")]     
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Dashboard(CommandContext ctx)
        {
            var traveler = SharedData.PaimonDB.GetTravelerBy<ulong>("DiscordID", ctx.User.Id);
            if (traveler != null) // Found the Document
            {
                var dashboardEmbed = TravelerDashboardEmbed(ctx.User, traveler);
                var messageBuilder = new DiscordMessageBuilder().WithEmbed(dashboardEmbed.Build());
                FileStream fs = new FileStream("./Resources/Images/acc.png", FileMode.Open);
                messageBuilder.WithFile(fs);     
                await ctx.RespondAsync(messageBuilder)
                        .ConfigureAwait(false);
            }
            else
                await PaimonServices.SendRespondAsync(ctx, $"It seems you're not in the database, please do `{SharedData.prefixes[0]}account create` through DM to create your account first and try again!"
                    , TimeSpan.FromSeconds(5));

        }

        #region AccountCreation
        [Command("create")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        [Description("Creates a new Traveler Account, if already exists, will overwrite.")]
        [RequireDirectMessage]
        public async Task CreateAuto(CommandContext ctx)
        {
            var interactive = ctx.Client.GetInteractivity();
            int currentResin = int.MinValue;
            int realmCurrency = int.MinValue;
            string GenshinServer = string.Empty;
            int RealmTrustRank = int.MinValue;
            int worldLevel = int.MinValue;
            int adeptalEnergy = int.MinValue;
            string inputBuffer = null;

            var Traveler = new Traveler();
            Traveler.DiscordID = ctx.User.Id;

            #region ResinAmountAccCreate
            string msg = $"Oh hi, **{ctx.User.Username}**#**{ctx.User.Discriminator}**! Remember when you fished me out of the ocean? {Emojis.BlurpEmote}" +
                $"Paimon promised that paimon will do her best to be a great guide! Paimon can help you track your resin, Realm Currency, and more! " +
                $"To start off (づ ◕‿◕ )づ, please enter your current Resin Amount...\n\n" +
                $"`Disclaimer: PaimonBot will only save information that you set, such as ResinAmount, WorldLevel, GenshinServer, RealmCurrency, AdeptalEnergy and RealmTrustRank." +
                $" Your Discord ID will be saved to attribute the above data to you. To delete your profile, simply do {SharedData.prefixes[0]}account delete.`\n" +
                $"Type `cancel` anywhere throughout this conversation to cancel account creation.";
            await ctx.Channel.SendMessageAsync(msg).ConfigureAwait(false);            
            do
            {
                var result = await interactive.WaitForMessageAsync(x => x.Author == ctx.User && x.Channel == ctx.Channel).ConfigureAwait(false);
                if (result.TimedOut)
                {
                    await ctx.Channel.SendMessageAsync(SharedData.TimedOutString).ConfigureAwait(false);
                    return; }
                inputBuffer = result.Result.Content;
                if (inputBuffer.ToLower().Contains("cancel"))
                    break;
                if (int.TryParse(inputBuffer, out currentResin))
                { currentResin = int.Parse(inputBuffer); break;}
                if (currentResin == int.MinValue)
                {
                    var m = await ctx.Channel.SendMessageAsync("Paimon can't seem to get the resin amount from that, please input a number between `0-160`!").ConfigureAwait(false);
                    await Task.Delay(3000);
                    await result.Result.DeleteAsync();
                    await m.DeleteAsync();
                }

            } while (currentResin == int.MinValue);
            if (inputBuffer.ToLower().Contains("cancel"))
                return;
            Traveler.ResinAmount = currentResin;
            if (Traveler.ResinAmount > 160)
                Traveler.ResinAmount = 160;
            if (Traveler.ResinAmount < 0)
                Traveler.ResinAmount = 0;
            Traveler.ResinUpdatedTime = DateTime.UtcNow;
            #endregion ResinAmountAccCreate

            #region WorldLevelAccCreate
            // World Level
            msg = $"Next up （＾ω＾）, Paimon would like to know your current World Level in Teyvat! Please enter a number between `0-8`...";
            await ctx.Channel.SendMessageAsync(msg).ConfigureAwait(false);
            do
            {
                var result = await interactive.WaitForMessageAsync(x => x.Author == ctx.User && x.Channel == ctx.Channel).ConfigureAwait(false);
                if (result.TimedOut)
                { await ctx.Channel.SendMessageAsync(SharedData.TimedOutString).ConfigureAwait(false); return; }
                inputBuffer = result.Result.Content;
                if (inputBuffer.ToLower().Contains("cancel"))
                    break;
                if (int.TryParse(inputBuffer, out worldLevel))
                {
                    worldLevel = int.Parse(inputBuffer);
                    if (Enum.IsDefined(typeof(WorldLevel), worldLevel))
                        Traveler.WorldLevel = (WorldLevel)worldLevel;
                    else
                    {
                        var m = await ctx.Channel.SendMessageAsync("Paimon can't seem to get your World Level from that, please input a number between `0-8`!").ConfigureAwait(false);
                        await Task.Delay(3000);
                        await result.Result.DeleteAsync();
                        await m.DeleteAsync();
                        worldLevel = int.MinValue;
                    }
                }
                else
                {
                    var m = await ctx.Channel.SendMessageAsync("Paimon can't seem to get your World Level from that, please input a number between `0-8`!").ConfigureAwait(false);
                    await Task.Delay(3000);
                    await result.Result.DeleteAsync();
                    await m.DeleteAsync();
                    worldLevel = int.MinValue;
                }

            } while (worldLevel == int.MinValue);
            if (inputBuffer.ToLower().Contains("cancel"))
                return;
            #endregion WorldLevelAccCreate

            #region GenshinServerAccCreate
            msg = $"Interesting (〃▽〃), Paimon would like to know which Teyvat server you reside in...\n\n" +
                $"Please enter between the following (case sensitive): `NorthAmerica` `Europe` `Asia` `TWHKMO`";
            await ctx.Channel.SendMessageAsync(msg).ConfigureAwait(false);
            do
            {
                var result = await interactive.WaitForMessageAsync(x => x.Author == ctx.User && x.Channel == ctx.Channel).ConfigureAwait(false);
                if (result.TimedOut)
                { await ctx.Channel.SendMessageAsync(SharedData.TimedOutString).ConfigureAwait(false); return; }
                inputBuffer = result.Result.Content;
                if (inputBuffer.ToLower().Contains("cancel"))
                    break;
                GenshinServer = inputBuffer;
                if (Enum.TryParse(GenshinServer, out TeyvatServer teyvatServer))
                    Traveler.GenshinServer = teyvatServer;
                else
                {
                    var m = await ctx.Channel.SendMessageAsync("Paimon can't seem to get your Genshin Server from that, please try again!").ConfigureAwait(false);
                    await Task.Delay(3000);
                    await result.Result.DeleteAsync();
                    await m.DeleteAsync();
                    GenshinServer = string.Empty;
                }
            } while (GenshinServer == string.Empty);
            if (inputBuffer.ToLower().Contains("cancel"))
                return;
            #endregion GenshinServerAccCreate

            #region RealmTrustRankAccCreate
            msg = $"Sounds like a big place! Next (*＾▽＾)／, Paimon would like to know your current Realm Trust Rank...\n" +
                $"Please enter a number between `1-10` as your Trust Rank. This is to determine your Currency capacity.";
            await ctx.Channel.SendMessageAsync(msg).ConfigureAwait(false);
            do
            {
                var result = await interactive.WaitForMessageAsync(x => x.Author == ctx.User && x.Channel == ctx.Channel).ConfigureAwait(false);
                if (result.TimedOut)
                { await ctx.Channel.SendMessageAsync(SharedData.TimedOutString).ConfigureAwait(false); return; }
                inputBuffer = result.Result.Content;
                if (inputBuffer.ToLower().Contains("cancel"))
                    break;
                if (int.TryParse(inputBuffer, out RealmTrustRank))
                {
                    RealmTrustRank = int.Parse(inputBuffer);
                    if (Enum.IsDefined(typeof(RealmTrustRank), RealmTrustRank))
                        Traveler.RealmTrustRank = (RealmTrustRank)RealmTrustRank;
                    else
                    {
                        var m = await ctx.Channel.SendMessageAsync("Paimon can't seem to get your Realm Trust Rank from that, please input a number between `1-10`!").ConfigureAwait(false);
                        await Task.Delay(3000);
                        await result.Result.DeleteAsync();
                        await m.DeleteAsync();
                        RealmTrustRank = int.MinValue;
                    }
                }
                else
                {
                    var m = await ctx.Channel.SendMessageAsync("Paimon can't seem to get your Realm Trust Rank from that, please input a number between `1-10`!").ConfigureAwait(false);
                    await Task.Delay(3000);
                    await result.Result.DeleteAsync();
                    await m.DeleteAsync();
                    RealmTrustRank = int.MinValue;
                }
            } while(RealmTrustRank == int.MinValue );
            if (inputBuffer.ToLower().Contains("cancel"))
                return;
            #endregion RealmTrustRankAccCreate

            #region AdeptalEnergyAccCreate
            msg = $"Big grinder we have here! Next, Paimon would like to know your current Adeptal Energy level....\n" +
                $"Please enter a number such as `21240`. This is to determine your Currency Recharge Rate.";
            await ctx.Channel.SendMessageAsync(msg).ConfigureAwait(false);
            do
            {
                var result = await interactive.WaitForMessageAsync(x => x.Author == ctx.User && x.Channel == ctx.Channel).ConfigureAwait(false);
                if (result.TimedOut)
                { await ctx.Channel.SendMessageAsync(SharedData.TimedOutString).ConfigureAwait(false); return; }
                inputBuffer = result.Result.Content;
                if (inputBuffer.ToLower().Contains("cancel"))
                    return;
                if (int.TryParse(inputBuffer, out adeptalEnergy))
                {
                    adeptalEnergy = int.Parse(inputBuffer);
                    Traveler.AdeptalEnergy = adeptalEnergy;
                }
                else
                {
                    var m = await ctx.Channel.SendMessageAsync("Paimon can't seem to get your Adeptal Energy from that, please only input your current adeptal energy amount!").ConfigureAwait(false);
                    await Task.Delay(3000);
                    await result.Result.DeleteAsync();
                    await m.DeleteAsync();
                    adeptalEnergy = int.MinValue;
                }
            } while (adeptalEnergy == int.MinValue);
            if (inputBuffer.ToLower().Contains("cancel"))
                return;

            #endregion AdeptalEnergyAccCreate

            #region RealmCurrencyAccCreate
            msg = $"That's awesome (/・0・)! Lastly, Paimon would like to know how much Realm Currency is built up in Tubby right now (Don't tell him about this!)...\n" +
                $"Please enter your current Currency amount, this will be cross-checked with the Trust Rank you've entered before.";
            await ctx.Channel.SendMessageAsync(msg).ConfigureAwait(false);
            do
            {
                var result = await interactive.WaitForMessageAsync(x => x.Author == ctx.User && x.Channel == ctx.Channel).ConfigureAwait(false);
                if (result.TimedOut)
                { await ctx.Channel.SendMessageAsync(SharedData.TimedOutString).ConfigureAwait(false); return; }
                inputBuffer = result.Result.Content;
                if (inputBuffer.ToLower().Contains("cancel"))
                    break;
                if (int.TryParse(inputBuffer, out realmCurrency))
                {
                    realmCurrency = int.Parse(inputBuffer);
                    if (realmCurrency <= Traveler.RealmTrustRank.GetTrustRankCurrencyCap())
                    {
                        Traveler.RealmCurrency = realmCurrency;
                        Traveler.CurrencyUpdated = DateTime.UtcNow;
                    }
                    else
                    {
                        var m = await ctx.Channel.SendMessageAsync($"Paimon caught you! That amount is above the Trust Rank Capacity that was set; Your trust rank is `{Traveler.RealmTrustRank}` " +
                            $"with a capacity of `{Traveler.RealmTrustRank.GetTrustRankCurrencyCap()}` Realm Currency. Please input a number below this!").ConfigureAwait(false);
                        await Task.Delay(3000);
                        await result.Result.DeleteAsync();
                        await m.DeleteAsync();
                    }
                }
                else
                {
                    var m = await ctx.Channel.SendMessageAsync("Paimon can't seem to get your Currency Amount from that, please input a number!").ConfigureAwait(false);
                    await Task.Delay(3000);
                    await result.Result.DeleteAsync();
                    await m.DeleteAsync();
                }
            } while (realmCurrency == int.MinValue || realmCurrency > Traveler.RealmTrustRank.GetTrustRankCurrencyCap());
            if (inputBuffer.ToLower().Contains("cancel"))
                return;
            #endregion RealmCurrencyAccCreate                

            if (SharedData.resinTimers.Exists(timer => timer._discordID == ctx.User.Id))
            {
                var timer = SharedData.resinTimers.Find(timer => timer._discordID == ctx.User.Id);
                timer.StopAndDispose();
                SharedData.resinTimers.Remove(timer);
                Log.Information($"Previous Resin timer for User {ctx.User.Id} has been removed.");
            }           
               

            if (SharedData.PaimonDB.TravelerExists(ctx.User.Id))
            { SharedData.PaimonDB.ReplaceTraveler(Traveler); }
            else
            { SharedData.PaimonDB.InsertTraveler(Traveler); }

            // Sets a new Timer to start.
            var aTimer = new ResinTimer(ctx.User.Id);
            aTimer.Start();
            SharedData.resinTimers.Add(aTimer);
            Log.Information($"Resin Timer for {aTimer._discordID} just started!");

            var embed = TravelerDashboardEmbed(ctx.User, Traveler);
            msg = $"Perfect O(〃＾▽＾〃)o! Paimon is now ready to help your on your journey! Use `{SharedData.prefixes[0]}account` to look at your profile, or if you'd just like to check " +
                $"your current resin or currency, use `{SharedData.prefixes[0]}resin` and `{SharedData.prefixes[0]}currency` respectively. Paimon hopes you enjoy our journey ahead!";
            await ctx.Channel.SendMessageAsync(msg).ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync(content: $"Here is your traveler data, **{ctx.User.Username}**#**{ctx.User.Discriminator}**! {Emojis.HappyEmote}", embed: embed.Build())
                .ConfigureAwait(false);
        }

        [Command("create")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        [Description("Creates a new Traveler Account, if already exists, will overwrite.")]
        public async Task Create(CommandContext ctx, int currentResin, int RealmCurrency, string GenshinServer, int AdeptalEnergy,int RealmTrustRank,int WorldLevel)
        {
            if (Enum.TryParse(GenshinServer, out TeyvatServer teyvatServer))
            {
                TeyvatServer teyvatServer1 = teyvatServer;
                if (Enum.IsDefined(typeof(WorldLevel), WorldLevel))
                {                    
                    WorldLevel worldLevel = (WorldLevel)WorldLevel;
                    if (Enum.IsDefined(typeof(RealmTrustRank), RealmTrustRank))
                    {
                        RealmTrustRank trustRank = (RealmTrustRank)RealmTrustRank;

                        if (AdeptalEnergy < 0)
                            AdeptalEnergy = 0;

                        // Removes the Previous ResinTimer
                        if (SharedData.resinTimers.Exists(timer => timer._discordID == ctx.User.Id))
                        {
                            var timer = SharedData.resinTimers.Find(timer => timer._discordID == ctx.User.Id);
                            timer.StopAndDispose();
                            SharedData.resinTimers.Remove(timer);
                            Log.Information($"Previous Resin timer for User {ctx.User.Id} has been removed.");  
                        }


                        Traveler traveler = new Traveler
                        {
                            DiscordID = ctx.User.Id,
                            ResinUpdatedTime = DateTime.UtcNow,
                            ParaGadget = DateTime.UtcNow,
                            RealmCurrency = RealmCurrency,
                            CurrencyUpdated = DateTime.UtcNow,
                            AdeptalEnergy = AdeptalEnergy,
                            RealmTrustRank = trustRank,
                            ResinAmount = currentResin,
                            GenshinServer = teyvatServer1,
                            WorldLevel = worldLevel
                        };
                        if (traveler.ResinAmount > 160)
                            traveler.ResinAmount = 160;
                        if (traveler.ResinAmount < 0)
                            traveler.ResinAmount = 0;
                        if (SharedData.PaimonDB.GetTravelerBy<ulong>("DiscordID", ctx.User.Id) == null)
                        { SharedData.PaimonDB.InsertTraveler(traveler); Log.Information("New {User} Traveler Added!", ctx.User); }
                        else
                        { SharedData.PaimonDB.ReplaceTraveler(traveler); Log.Information("Traveler {User} had their data updated!", ctx.User); }

                        // Starts a new ResinTimer 
                        var aTimer = new ResinTimer(ctx.User.Id);   
                        aTimer.Start();
                        SharedData.resinTimers.Add(aTimer);
                        Log.Information($"Resin Timer for {aTimer._discordID} just started!");
                        await ctx.RespondAsync($"Successfully created an account! Use `{SharedData.prefixes[0]}account` to check your data.").ConfigureAwait(false);
                    }
                    else
                        await PaimonServices.SendRespondAsync(ctx, "It seems that I cannot interpret that Realm Trust Rank value. Please input an integer between `1-10`.", TimeSpan.FromMinutes(5))
                            .ConfigureAwait(false);
                }
                else
                    await PaimonServices.SendRespondAsync(ctx, "It seems that I cannot interpret that WorldLevel value. Please input an integer between `1-8`.", TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            }
            else
                await PaimonServices.SendRespondAsync(ctx, "It seems that I cannot interpret that Genshin server, please input one of the following: [`NorthAmerica`, `Europe`, `Asia`,`TWHKMO`]"
                    , TimeSpan.FromSeconds(5));
        }
        #endregion AccountCreation

        #region AccountDelete
        [Command("delete")]
        [Description("Deletes Traveler that called this command's Account")]
        [Cooldown(1,5, CooldownBucketType.User)]
        public async Task Delete(CommandContext ctx)
        {
            var Traveler = SharedData.PaimonDB.GetTravelerBy<ulong>("DiscordID", ctx.User.Id);
            if (Traveler != null)
            {
                var interactive = ctx.Client.GetInteractivity();
                var msgBuilder = new DiscordMessageBuilder()
                    .WithContent("Are you sure you would like to delete your account?");
                var deleteBtn = new DiscordButtonComponent(ButtonStyle.Danger, "acc_delete_yes", "Delete");
                var cancelBtn = new DiscordButtonComponent(ButtonStyle.Secondary, "acc_delete_cancel", "Cancel");
                msgBuilder.AddComponents(deleteBtn, cancelBtn);
                var msg = await ctx.RespondAsync(msgBuilder).ConfigureAwait(false);

                var result = await interactive.WaitForButtonAsync(msg).ConfigureAwait(false);
                if (result.TimedOut)
                {
                    await msg.DeleteAsync().ConfigureAwait(false);
                    return;
                }
                switch (result.Result.Id)
                {
                    case "acc_delete_yes":
                        await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        SharedData.PaimonDB.DeleteTraveler(Traveler);
                        await msg.DeleteAsync().ConfigureAwait(false);
                        await result.Result.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder(){ IsEphemeral = true, Content = "Your Account has been successfully deleted! Thanks for using PaimonBot!" });
                        break;
                    case "acc_delete_cancel":                        
                        await msg.DeleteAsync().ConfigureAwait(false);
                        break;
                }
            }
            else
                await PaimonServices.SendRespondAsync(ctx, $"Paimon can't find your traveler data, are you sure you have an account with me?", TimeSpan.FromSeconds(5));
        }
        #endregion AccountDelete

        #region AccountUpdate
        [Command("update")]
        [Description("Updates one of a Traveler's data value. FieldName options: `resin` `currency` `server` `worldlevel` `trustrank`")]
        [Cooldown(1, 3, CooldownBucketType.User)]
        public async Task Update(CommandContext ctx, [RemainingText] string FieldName)
        {
            if (FieldName == null)
                throw new ArgumentException();
            if (SharedData.PaimonDB.TravelerExists(ctx.User.Id))
            {
                var Traveler = SharedData.PaimonDB.GetTravelerBy("DiscordID", ctx.User.Id);
                FieldName = FieldName.Trim().ToLower();                
                switch (FieldName)
                {
                    case string a when a.Contains("resin"):
                        if (!await UpdateResin(ctx, Traveler).ConfigureAwait(false))                        
                            return;
                        await ctx.RespondAsync("Your resin has been successfully updated!").ConfigureAwait(false);
                        break;
                   case string a when a.Contains("currency"):
                        if (!await UpdateCurrency(ctx, Traveler).ConfigureAwait(false))
                            return;
                        await ctx.RespondAsync("Your currency has been successfully updated!").ConfigureAwait(false);
                        break;
                    case string a when a.Contains("server"):
                        if (!await UpdateServer(ctx, Traveler).ConfigureAwait(false))
                            return;
                        await ctx.RespondAsync("Your server has been successfully updated!").ConfigureAwait(false);
                        break;
                    case string a when a.Contains("worldlevel"):
                        if (!await UpdateWL(ctx, Traveler).ConfigureAwait(false))
                            return;
                        await ctx.RespondAsync("Your World Level has been succesfully updated!").ConfigureAwait(false);
                        break;
                    case string a when a.Contains("trustrank"):
                        if (!await UpdateTrustRank(ctx, Traveler).ConfigureAwait(false))
                            return;
                        await ctx.RespondAsync("Your Trust Rank has been succesfully updated!").ConfigureAwait(false);
                        break;
                    default:
                        await PaimonServices.SendRespondAsync(ctx, "Paimon seem to not understand what you would like to update there. Please make sure you have one of the following phrases: `resin` `currency` `server` `worldlevel` `trustrank`", TimeSpan.FromSeconds(5));
                        break;
                }
            } 
            else
                await PaimonServices.SendRespondAsync(ctx, $"Paimon can't find your traveler data, are you sure you have an account with me? Use `{SharedData.prefixes[0]}account create` to create one!", TimeSpan.FromSeconds(5));
        }

        private async Task<bool> UpdateResin(CommandContext ctx, Traveler traveler)
        {
            var inputBuffer = string.Empty;
            var interactive = ctx.Client.GetInteractivity();
            var m = await ctx.Channel.SendMessageAsync("Please enter your new resin amount below (an integer):\nEnter `cancel` to cancel the operation.").ConfigureAwait(false);
            do
            {
                var result = await interactive.WaitForMessageAsync(c => c.Author == ctx.User && c.Channel == ctx.Channel);
                if (result.TimedOut)
                { await m.DeleteAsync(); await ctx.Channel.SendMessageAsync(SharedData.TimedOutString); return false; }
                inputBuffer = result.Result.Content;
                if (inputBuffer.ToLower().Contains("cancel"))
                    break;
                if (int.TryParse(inputBuffer, out int t))
                {
                    traveler.ResinAmount = t;
                    traveler.ResinUpdatedTime = DateTime.UtcNow;
                }
                else
                {
                    var warn = await ctx.Channel.SendMessageAsync("Paimon can't seem to get the resin amount from that, please input a number between `0-160`!").ConfigureAwait(false);
                    await Task.Delay(3000);
                    await result.Result.DeleteAsync();
                    await warn.DeleteAsync();
                }
            } while (!int.TryParse(inputBuffer, out int i));
            if (inputBuffer.ToLower().Contains("cancel"))
            {
                await m.DeleteAsync().ConfigureAwait(false);
                return false;
            }
            if (traveler.ResinAmount > 160)
                traveler.ResinAmount = 160;
            if (traveler.ResinAmount < 0)
                traveler.ResinAmount = 0;
            SharedData.PaimonDB.ReplaceTraveler(traveler);
            // Doesn't work :<
            //SharedData.PaimonDB.UpdateTraveler(traveler, "ResinAmount", traveler.ResinAmount);
            //SharedData.PaimonDB.UpdateTraveler(traveler, "ResinUpdated", DateTime.UtcNow);
            return true;
        }

        private async Task<bool> UpdateCurrency(CommandContext ctx, Traveler traveler)
        {
            var inputBuffer = string.Empty;
            var interactive = ctx.Client.GetInteractivity();
            var realmCurrency = int.MinValue;
            var m = await ctx.Channel.SendMessageAsync("Please enter your new Realm Currency amount below (an integer):\nEnter `cancel` to cancel the operation.").ConfigureAwait(false);
            do
            {
                var result = await interactive.WaitForMessageAsync(x => x.Author == ctx.User && x.Channel == ctx.Channel).ConfigureAwait(false);
                if (result.TimedOut)
                { await ctx.Channel.SendMessageAsync(SharedData.TimedOutString).ConfigureAwait(false); return false; }
                inputBuffer = result.Result.Content;
                if (inputBuffer.ToLower().Contains("cancel"))
                    break;
                if (int.TryParse(inputBuffer, out realmCurrency))
                {
                    realmCurrency = int.Parse(inputBuffer);
                    if (realmCurrency <= traveler.RealmTrustRank.GetTrustRankCurrencyCap())
                    {
                        traveler.RealmCurrency = realmCurrency;
                        traveler.CurrencyUpdated = DateTime.UtcNow;
                    }
                    else
                    {
                        var s = await ctx.Channel.SendMessageAsync($"Paimon caught you! That amount is above the Trust Rank Capacity that was set; Your trust rank is `{traveler.RealmTrustRank}` " +
                            $"with a capacity of `{traveler.RealmTrustRank.GetTrustRankCurrencyCap()}` Realm Currency. Please input a number below this!").ConfigureAwait(false);
                        await Task.Delay(3000);
                        await result.Result.DeleteAsync();
                        await s.DeleteAsync();
                    }
                }
                else
                {
                    var s = await ctx.Channel.SendMessageAsync("Paimon can't seem to get your Currency Amount from that, please input a number!").ConfigureAwait(false);
                    await Task.Delay(3000);
                    await result.Result.DeleteAsync();
                    await s.DeleteAsync();
                }
            } while (realmCurrency == int.MinValue || realmCurrency > traveler.RealmTrustRank.GetTrustRankCurrencyCap());
            if (inputBuffer.ToLower().Contains("cancel"))
            {
                await m.DeleteAsync().ConfigureAwait(false);
                return false;
            }
            SharedData.PaimonDB.ReplaceTraveler(traveler);
            //SharedData.PaimonDB.UpdateTraveler(traveler, "RealmCurrency", traveler.RealmCurrency);
            //SharedData.PaimonDB.UpdateTraveler(traveler, "RealmCurrencyUpdated", DateTime.UtcNow);
            return true;
        }

        private async Task<bool> UpdateServer(CommandContext ctx, Traveler traveler)
        {
            var inputBuffer = string.Empty;
            var GenshinServer = string.Empty;
            var interactive = ctx.Client.GetInteractivity();
            var m = await ctx.Channel.SendMessageAsync("Please enter your new Genshin Server below. Please choose between (case sensitive): [`NorthAmerica` `Europe` `Asia` `TWHKMO`]\nEnter `cancel` to cancel the operation.").ConfigureAwait(false);
            do
            {
                var result = await interactive.WaitForMessageAsync(x => x.Author == ctx.User && x.Channel == ctx.Channel).ConfigureAwait(false);
                if (result.TimedOut)
                { await ctx.Channel.SendMessageAsync(SharedData.TimedOutString).ConfigureAwait(false); return false; }
                inputBuffer = result.Result.Content;
                if (inputBuffer.ToLower().Contains("cancel"))
                    break;
                GenshinServer = inputBuffer;
                if (Enum.TryParse(GenshinServer, out TeyvatServer teyvatServer))
                    traveler.GenshinServer = teyvatServer;
                else
                {
                    var s = await ctx.Channel.SendMessageAsync("Paimon can't seem to get your Genshin Server from that, please try again!").ConfigureAwait(false);
                    await Task.Delay(3000);
                    await result.Result.DeleteAsync();
                    await s.DeleteAsync();
                    GenshinServer = string.Empty;
                }
            } while (GenshinServer == string.Empty);
            if (inputBuffer.ToLower().Contains("cancel"))
            {
                await m.DeleteAsync().ConfigureAwait(false);
                return false;
            }
            SharedData.PaimonDB.ReplaceTraveler(traveler);
            //SharedData.PaimonDB.UpdateTraveler(traveler, "TeyvatServer", traveler.GenshinServer);
            return true;
        }

        private async Task<bool> UpdateWL(CommandContext ctx, Traveler traveler)
        {
            var inputBuffer = string.Empty;
            var interactive = ctx.Client.GetInteractivity();
            var worldLevel = int.MinValue;
            var m = await ctx.Channel.SendMessageAsync("Please enter your new World Level. Please enter a number between `0-8`.\nEnter `cancel` to cancel the operation.").ConfigureAwait(false);
            do
            {
                var result = await interactive.WaitForMessageAsync(x => x.Author == ctx.User && x.Channel == ctx.Channel).ConfigureAwait(false);
                if (result.TimedOut)
                { await ctx.Channel.SendMessageAsync(SharedData.TimedOutString).ConfigureAwait(false); return false; }
                inputBuffer = result.Result.Content;
                if (inputBuffer.ToLower().Contains("cancel"))
                    break;
                if (int.TryParse(inputBuffer, out worldLevel))
                {
                    worldLevel = int.Parse(inputBuffer);
                    if (Enum.IsDefined(typeof(WorldLevel), worldLevel))
                        traveler.WorldLevel = (WorldLevel)worldLevel;
                    else
                    {
                        var s = await ctx.Channel.SendMessageAsync("Paimon can't seem to get your World Level from that, please input a number between `0-8`!").ConfigureAwait(false);
                        await Task.Delay(3000);
                        await result.Result.DeleteAsync();
                        await s.DeleteAsync();
                        worldLevel = int.MinValue;
                    }
                }
                else
                {
                    var s = await ctx.Channel.SendMessageAsync("Paimon can't seem to get your World Level from that, please input a number between `0-8`!").ConfigureAwait(false);
                    await Task.Delay(3000);
                    await result.Result.DeleteAsync();
                    await s.DeleteAsync();
                    worldLevel = int.MinValue;
                }

            } while (worldLevel == int.MinValue);
            if (inputBuffer.ToLower().Contains("cancel"))
            {
                await m.DeleteAsync().ConfigureAwait(false);
                return false;
            }
            SharedData.PaimonDB.ReplaceTraveler(traveler);
            //SharedData.PaimonDB.UpdateTraveler(traveler, "TeyvatLevel", traveler.WorldLevel);
            return true;
        }

        private async Task<bool> UpdateTrustRank(CommandContext ctx, Traveler traveler)
        {
            var inputBuffer = string.Empty;
            var interactive = ctx.Client.GetInteractivity();
            var RealmTrustRank = int.MinValue;
            var m = await ctx.Channel.SendMessageAsync("Please enter your new Trust Rank. Please enter a number between `1-8`.\nEnter `cancel` to cancel the operation.").ConfigureAwait(false);
            do
            {
                var result = await interactive.WaitForMessageAsync(x => x.Author == ctx.User && x.Channel == ctx.Channel).ConfigureAwait(false);
                if (result.TimedOut)
                { await ctx.Channel.SendMessageAsync(SharedData.TimedOutString).ConfigureAwait(false); return false; }
                inputBuffer = result.Result.Content;
                if (inputBuffer.ToLower().Contains("cancel"))
                    break;
                if (int.TryParse(inputBuffer, out RealmTrustRank))
                {
                    RealmTrustRank = int.Parse(inputBuffer);
                    if (Enum.IsDefined(typeof(RealmTrustRank), RealmTrustRank))
                        traveler.RealmTrustRank = (RealmTrustRank)RealmTrustRank;
                    else
                    {
                        var s = await ctx.Channel.SendMessageAsync("Paimon can't seem to get your Realm Trust Rank from that, please input a number between `1-10`!").ConfigureAwait(false);
                        await Task.Delay(3000);
                        await result.Result.DeleteAsync();
                        await s.DeleteAsync();
                        RealmTrustRank = int.MinValue;
                    }
                }
                else
                {
                    var s = await ctx.Channel.SendMessageAsync("Paimon can't seem to get your Realm Trust Rank from that, please input a number between `1-10`!").ConfigureAwait(false);
                    await Task.Delay(3000);
                    await result.Result.DeleteAsync();
                    await s.DeleteAsync();
                    RealmTrustRank = int.MinValue;
                }
            } while (RealmTrustRank == int.MinValue);
            if (inputBuffer.ToLower().Contains("cancel"))
            {
                await m.DeleteAsync().ConfigureAwait(false);
                return false;
            }
            SharedData.PaimonDB.ReplaceTraveler(traveler);
            //SharedData.PaimonDB.UpdateTraveler(traveler, "RealmTrustRank", traveler.RealmTrustRank);
            return true;
        }
        #endregion AccountUpdate
        private DiscordEmbedBuilder TravelerDashboardEmbed(DiscordUser user, Traveler traveler)
        {
            StringBuilder desc = new StringBuilder();
            desc.AppendLine(Formatter.Bold("DiscordID: ") + Formatter.InlineCode(traveler.DiscordID.ToString()));            
            if (traveler.ParaGadget != null)
                desc.AppendLine(Formatter.Bold("Parametric Gadget Used Date: ") + $"<t:{((DateTimeOffset)traveler.ParaGadget.ToUniversalTime()).ToUnixTimeSeconds()}>");
            desc.AppendLine();

            var embed = new DiscordEmbedBuilder()
              .WithAuthor("PaimonBot", null, SharedData.logoURL)
              .WithColor(SharedData.defaultColour)
              .WithTitle($"{user.Username}#{user.Discriminator}'s Traveler Profile")
              .WithThumbnail(user.AvatarUrl)
              .WithTimestamp(DateTime.UtcNow)
              .WithDescription(desc.ToString())
              .WithImageUrl("attachment://acc.png")
              .WithFooter($"If any of the information above is wrong, please update said information using {SharedData.prefixes[0]}acc update [resin/currency/server/worldlevel/trustrank]");

            if (traveler.ResinAmount != int.MinValue && traveler.ResinUpdatedTime != null)
            {
                desc.Clear();
                desc.AppendLine("Current Resin: " + traveler.ResinAmount + " " + Emojis.ResinEmote);
                desc.AppendLine("Resin Last Updated: " + $"<t:{((DateTimeOffset)traveler.ResinUpdatedTime.ToUniversalTime()).ToUnixTimeSeconds()}>");
                desc.AppendLine();
                embed.AddField("Resin Information", desc.ToString());
            }

            if (traveler.RealmCurrency != int.MinValue && traveler.CurrencyUpdated != null)
            {
                var adeptal = CurrencyServices.ParseAdeptalFromInt(traveler.AdeptalEnergy);
                desc.Clear();
                desc.AppendLine("Current Adeptal Energy: " + $"{traveler.AdeptalEnergy} " + Emojis.AdeptalEmote + $"{adeptal}");
                desc.AppendLine("Current Realm Currency: " + traveler.RealmCurrency + " " + Emojis.CurrencyEmote);
                desc.AppendLine("Currency Last User Set: " + $"<t:{((DateTimeOffset)traveler.CurrencyUpdated.ToUniversalTime()).ToUnixTimeSeconds()}>");                
                desc.AppendLine();
                embed.AddField("Realm Currency Information", desc.ToString());
            }

            desc.Clear();
            desc.AppendLine("Realm Trust Rank: " + traveler.RealmTrustRank);
            desc.AppendLine("Genshin Server: " + traveler.GenshinServer.ToString());
            desc.AppendLine("World Level: " + traveler.WorldLevel.ToString());
            embed.AddField("Teyvat Data", desc.ToString());
            

            return embed;
        }
    }
}
