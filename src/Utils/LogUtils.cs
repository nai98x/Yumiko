using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands.EventArgs;
using Newtonsoft.Json;

namespace Yumiko.Utils
{
    public static class LogUtils
    {
        public static DiscordEmbedBuilder LogSlashCommand(SlashCommandExecutedEventArgs e)
        {
            var builder = new DiscordEmbedBuilder()
            {
                Title = "Slash Command executed",
                Color = DiscordColor.Green
            };

            string options = string.Empty;
            var args = e.Context.Interaction.Data.Options;
            if (args != null)
            {
                options = GetCommandArgs(args);
            }

            builder.AddField("Command", $"/{e.Context.CommandName} {options}", false);

            return builder;
        }
        
        public static DiscordEmbedBuilder LogSlashCommandError(SlashCommandErrorEventArgs e)
        {
            var builder = new DiscordEmbedBuilder()
            {
                Title = "Slash Command error",
                Description = GetErrorString(e),
                Color = DiscordColor.Red
            };

            string options = string.Empty;
            var args = e.Context.Interaction.Data.Options;
            if (args != null)
            {
                options = GetCommandArgs(args);
            }

            builder.AddField("Command", $"/{e.Context.CommandName} {options}", false);

            return builder;
        }

        private static string GetCommandArgs(IEnumerable<DiscordInteractionDataOption> args)
        {
            string options = string.Empty;

            foreach (var arg in args)
            {
                if (arg.Type is ApplicationCommandOptionType.SubCommand)
                {
                    options += $"{arg.Name} ";
                    foreach (var arg2 in arg.Options)
                    {
                        options += $"{Formatter.InlineCode($"{arg2.Name}: {arg2.Value}")} ";
                    }
                }
                else
                {
                    options += $"{Formatter.InlineCode($"{arg.Name}: {arg.Value}")} ";
                }
            }

            return options;
        }

        private static string GetErrorString(SlashCommandErrorEventArgs e)
        {
            string desc = $"{e.Exception.Message}\n{Formatter.BlockCode(e.Exception.StackTrace)}";
            switch (e.Exception)
            {
                case BadRequestException br:
                    dynamic? parsedJson = JsonConvert.DeserializeObject(br.Errors);
                    desc += $"\n{Formatter.BlockCode($"{br.JsonMessage}\n{JsonConvert.SerializeObject(parsedJson, Formatting.Indented)}")}";
                    break;
                case NotFoundException nf:
                    desc += $"\n{Formatter.BlockCode(nf.JsonMessage)}";
                    break;
                case RateLimitException rl:
                    desc += $"\n{Formatter.BlockCode(rl.JsonMessage)}";
                    break;
                case RequestSizeException rz:
                    desc += $"\n{Formatter.BlockCode(rz.JsonMessage)}";
                    break;
                case ServerErrorException se:
                    desc += $"\n{Formatter.BlockCode(se.JsonMessage)}";
                    break;
                case UnauthorizedException ue:
                    desc += $"\n{Formatter.BlockCode(ue.JsonMessage)}";
                    break;
            }
            return desc;
        }
    }
}
