using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PaimonBot.Services
{
    public sealed class DefaultHelpFormatter : BaseHelpFormatter
    {
        private readonly DiscordEmbedBuilder _output;
        private string description;
        public DefaultHelpFormatter(CommandContext ctx) : base(ctx)
        {
            _output = new DiscordEmbedBuilder()
                .WithColor(SharedData.defaultColour)
                .WithTimestamp(DateTime.Now)
                .WithFooter($"Requested by {ctx.User.Username}");
        }

        public override CommandHelpMessage Build()
        {
            _output.WithDescription(Formatter.Bold(description));
            return new CommandHelpMessage(embed: _output);
        }

        //first method it will look to i!help <cmd> // might want to make a non embed help for this
        public override BaseHelpFormatter WithCommand(Command cmd)
        {
            description = $"Command Description and Usage.\n[..] = Needed Argument(s)\n<..> = Optional Argument(s)";
            _output.WithAuthor("Command Help Page", null, SharedData.logoURL);
            _output.ClearFields();

            StringBuilder sb = new StringBuilder();

            foreach (var overload in cmd.Overloads)
            {
                sb.Append($"{SharedData.prefixes[0]}{cmd.Name}"); // default just a name
                if (overload.Arguments.Count >= 1)
                {
                    sb.Append(" " + string.Join(" ", overload.Arguments.Select(xarg => xarg.IsOptional ? $"<{xarg.Name}>" : $"[{xarg.Name}]")));
                }
                sb.Append("\n");
            }
            _output.AddField(Formatter.Bold("# Usage"), Formatter.BlockCode(sb.ToString()));
            if (cmd.Aliases?.Any() ?? false) //needs changing
                _output.AddField("# Aliases", Formatter.BlockCode("<Aliases: " + string.Join(" ", cmd.Aliases.Select(Formatter.InlineCode)) + ">", "xml"));
            _output.AddField(Formatter.Bold("# Description"), Formatter.BlockCode("<Description: " + cmd.Description + ">", "xml"));

            return this;
        }

        //second method it looks to (used as main help formatter) i!help
        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            List<Category> module = new List<Category>();
            var enumerable = subcommands.ToList();
            if (enumerable.Any())
            {
                foreach (var cmd in enumerable)
                {
                    if (cmd.Parent is not CommandGroup)
                    {
                        description = $"Below is the list of commands!\nFor more info on specific commands and its usage, use `{SharedData.prefixes[0]}help <command>`\nSomething went wrong? Contact dev through `{SharedData.prefixes[0]}contact dev`";
                        _output.WithAuthor("Command List", null, SharedData.logoURL);
                        _output.ClearFields();
                        // getting the cmd's group through their attributes
                        string groupName = string.Empty;
                        string groupEmoji = string.Empty;
                        foreach (var attr in cmd.CustomAttributes)
                        {
                            if (attr is CategoryAttribute)
                            {
                                CategoryAttribute a = (CategoryAttribute)attr;
                                groupName = a.name;
                                groupEmoji = a.emoji;
                            }
                        }
                        bool exists = false;
                        foreach (var group in module)
                        {
                            if (group.GroupName == groupName)
                                exists = true;
                        }
                        // put outside to ensure it is one once per cmd
                        if (exists) //gets that a field already exists
                        {
                            module.Find(x => x.GroupName == groupName).CmdList.Add(cmd); //finds the groupname by name, and then adds the cmd
                        }
                        else // else it makes a new group and adds in the main module (filled with groups)
                        {
                            Category group = new Category(groupName, groupEmoji);
                            group.CmdList.Add(cmd);
                            module.Add(group);
                        }
                        //where we output all the groups
                        foreach (var group in module)
                        {
                            if (group.CmdList.Exists(cmd => cmd.Name.ToLower() != "help"))
                                _output.AddField(group.GroupEmoji + " " + Formatter.Bold(group.GroupName), string.Join(" ", group.CmdList.Select(c => Formatter.InlineCode(c.Name))));
                        }
                    }
                    else
                    {
                        description = $"Command Description and Usage.\n[..] = Needed Argument(s)\n<..> = Optional Argument(s)";
                        _output.WithAuthor("Command Help Page", null, SharedData.logoURL);
                        _output.ClearFields();
                        StringBuilder sb = new StringBuilder();
                        foreach (var overload in cmd.Overloads)
                        {
                            sb.Append($"{SharedData.prefixes[0]}{cmd.Parent.Name} \n" + string.Join("|", cmd.Parent.Children.Select(c => c.Name)) + "\n"); // default just a name
                        }
                        _output.AddField(Formatter.Bold("# Usage"), Formatter.BlockCode(sb.ToString()));
                        if (cmd.Aliases?.Any() ?? false) //needs changing
                            _output.AddField("# Aliases", Formatter.BlockCode("<Aliases: " + string.Join(" ", cmd.Aliases.Select(Formatter.InlineCode)) + ">", "xml"));
                        _output.AddField(Formatter.Bold("# Description"), Formatter.BlockCode("<Description: " + cmd.Description + ">", "xml"));
                    }
                }
            }
            return this;
        }
    }
}
