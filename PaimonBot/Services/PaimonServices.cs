using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaimonBot.Services
{
    /// <summary>
    /// A class with methods and services to help Paimon function
    /// </summary>
    public class PaimonServices
    {
        internal static async Task SendEmbedAsync(CommandContext context, string title, string v, ResponseType error)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Category module for the Help Embed
    /// </summary>
    public class Category
    {
        public string GroupName;
        public List<Command> CmdList;
        public string GroupEmoji;

        public Category(string name, string emoji)
        {
            this.GroupName = name;
            this.GroupEmoji = emoji;
            this.CmdList = new List<Command>();
        }
    }

    /// <summary>
    /// GroupName Attribute to put in for Command Method
    /// </summary>
    public class CategoryAttribute : Attribute
    {
        public string name;
        public string emoji;
        public CategoryAttribute(CategoryName group)
        {
            this.name = group switch
            {
                CategoryName.Info => "Info",
                CategoryName.Account => "Account",
                CategoryName.Misc => "Misc"
            };
            this.emoji = group switch
            {
                CategoryName.Info => "",
                CategoryName.Account => "",
                CategoryName.Misc => ""
            };
        }
    }
}
