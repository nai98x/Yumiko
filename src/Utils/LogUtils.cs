using DSharpPlus.Exceptions;
using Newtonsoft.Json;

namespace Yumiko.Utils
{
    public static class LogUtils
    {
        public static string GetSlashCommandArgs(SlashCommandExecutedEventArgs e, bool parameters)
        {
            string options = string.Empty;
            var args = e.Context.Interaction.Data.Options;
            if (args != null)
            {
                options = GetCommandArgs(args, false, parameters);
            }

            return $"/{e.Context.CommandName} {options}";
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
                options = GetCommandArgs(args, true, true);
            }

            builder.AddField("Command", $"/{e.Context.CommandName} {options}", false);

            return builder;
        }

        private static string GetCommandArgs(IEnumerable<DiscordInteractionDataOption> args, bool format, bool parameters)
        {
            string options = string.Empty;

            foreach (var arg in args)
            {
                if (arg.Type is ApplicationCommandOptionType.SubCommand)
                {
                    options += $"{arg.Name} ";
                    foreach (var arg2 in arg.Options)
                    {
                        if (parameters && arg2.Type is not ApplicationCommandOptionType.SubCommand && arg2.Type is not ApplicationCommandOptionType.SubCommandGroup)
                        {
                            if (format)
                            {
                                options += $"{Formatter.InlineCode($"{arg2.Name}: {arg2.Value}")} ";
                            }
                            else
                            {
                                options += $"[{arg2.Name}: {arg2.Value}] ";
                            }
                        }
                    }
                }
                else
                {
                    if (parameters && arg.Type is not ApplicationCommandOptionType.SubCommand && arg.Type is not ApplicationCommandOptionType.SubCommandGroup)
                    {
                        if (format)
                        {
                            options += $"{Formatter.InlineCode($"{arg.Name}: {arg.Value}")} ";
                        }
                        else
                        {
                            options += $"[{arg.Name}: {arg.Value}] ";
                        }
                    } 
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
