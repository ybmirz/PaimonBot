using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using PaimonBot.Services;
using PaimonBot.Services.HelpFormatter;
using System;
using System.Threading.Tasks;

namespace PaimonBot.Commands
{
    [Group("account"), Aliases("acc", "dashboard")]
    [Description("A set of Traveler Account Commands for PaimonBot Travelers!")]
    [Category(CategoryName.Account)]
    public class AccountCommands : BaseCommandModule
    {
        
    }
}
