using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using PaimonBot.Services;
using PaimonBot.Services.HelpFormatter;
using PaimonBot.Extensions.DataModels;
using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using Serilog;
using System.Linq;
using PaimonBot.Extensions.Data;
using DSharpPlus.Interactivity.Extensions;
using System.Text;

namespace PaimonBot.Commands
{
    [Group("account"), Aliases("acc", "dashboard")]
    [Description("A set of Traveler Account Commands for PaimonBot Travelers!")]
    [Category(CategoryName.Account)]
    public class AccountCommands : BaseCommandModule
    {
        [GroupCommand]
        [Description("Shows the Embed Dashboard consisting Traveler's Account Info.")]
        public async Task Dashboard(CommandContext ctx)
        {
            var traveler = SharedData.PaimonDB.GetTravelerBy<ulong>("DiscordID", ctx.User.Id);
            if (traveler != null) // Found the Document
            {
                var dashboardEmbed = TravelerDashboardEmbed(ctx.User, traveler);
                await ctx.RespondAsync(embed: dashboardEmbed.Build())
                    .ConfigureAwait(false);
            }
            else
                await PaimonServices.SendRespondAsync(ctx, $"It seems you're not in the database, please do `{SharedData.prefixes[0]}account create` or `{SharedData.prefixes[0]}resin set [amount]` first and try again."
                    , TimeSpan.FromSeconds(5));

        }

        #region AccountCreation
        [Command("create")]
        [Description("Creates a new Traveler Account, if already exists, will overwrite.")]
        public async Task Create(CommandContext ctx, int currentResin, int RealmCurrency, string GenshinServer, int RealmTrustRank,int WorldLevel)
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
                        Traveler traveler = new Traveler
                        {
                            DiscordID = ctx.User.Id,
                            GuildID = ctx.Guild.Id,
                            ResinUpdatedTime = DateTime.UtcNow,
                            ParaGadget = DateTime.UtcNow,
                            RealmCurrency = RealmCurrency,
                            CurrencyUpdated = DateTime.UtcNow,
                            RealmTrustRank = trustRank,
                            ResinAmount = currentResin,
                            GenshinServer = teyvatServer1,
                            WorldLevel = worldLevel
                        };
                        if (SharedData.PaimonDB.GetTravelerBy<ulong>("DiscordID", ctx.User.Id) == null)
                        { SharedData.PaimonDB.InsertTraveler(traveler); Log.Information("New {User} Traveler Added!", ctx.User); }
                        else
                        { SharedData.PaimonDB.ReplaceTraveler(traveler); Log.Information("Traveler {User} had their data updated!", ctx.User); }
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

        [Command("create")]
        [Description("Creates a new Traveler Account, if already exists, will overwrite.")]
        public async Task CreateAuto(CommandContext ctx)
        {
            var interactive = ctx.Client.GetInteractivity();
        }
        #endregion AccountCreation


        private DiscordEmbedBuilder TravelerDashboardEmbed(DiscordUser user, Traveler traveler)
        {
            StringBuilder desc = new StringBuilder();
            desc.AppendLine(Formatter.Bold("DiscordID: ") + Formatter.InlineCode(traveler.DiscordID.ToString()));
            desc.AppendLine(Formatter.Bold("GuildID: ") + Formatter.InlineCode(traveler.GuildID.ToString()));
            desc.AppendLine(Formatter.Bold("Parametric Gadget Used Date: ") + $"<t:{((DateTimeOffset)traveler.ParaGadget.ToUniversalTime()).ToUnixTimeSeconds()}>");
            desc.AppendLine();

            var embed = new DiscordEmbedBuilder()
              .WithAuthor("PaimonBot", null, SharedData.logoURL)
              .WithColor(SharedData.defaultColour)
              .WithTitle($"{user.Username}#{user.Discriminator}'s Traveler Profile")
              .WithThumbnail(user.AvatarUrl)
              .WithTimestamp(DateTime.UtcNow)
              .WithDescription(desc.ToString());

            desc.Clear();
            desc.AppendLine("Current Resin: " + traveler.ResinAmount + " :resin:");
            desc.AppendLine("Resin Last Updated: " + $"<t:{((DateTimeOffset)traveler.ResinUpdatedTime.ToUniversalTime()).ToUnixTimeSeconds()}>");
            desc.AppendLine();
            embed.AddField("Resin Information", desc.ToString());

            desc.Clear();
            desc.AppendLine("Current Realm Currency: " + traveler.RealmCurrency + " :currency:");
            desc.AppendLine("Currency Last Set: " + $"<t:{((DateTimeOffset)traveler.CurrencyUpdated.ToUniversalTime()).ToUnixTimeSeconds()}>");
            desc.AppendLine();
            embed.AddField("Realm Currency Information", desc.ToString());            

            desc.Clear();
            desc.AppendLine("Realm Trust Rank: " + traveler.RealmTrustRank);
            desc.AppendLine("Genshin Server: " + traveler.GenshinServer.ToString());
            desc.AppendLine("World Level: " + traveler.WorldLevel.ToString());
            embed.AddField("Teyvat Data", desc.ToString());
            

            return embed;
        }
    }
}
