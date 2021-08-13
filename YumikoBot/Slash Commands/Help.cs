using Discord_Bot;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace YumikoBot
{
    public class HelpSlashCommands : ApplicationCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new();

        [SlashCommand("help", "Information and help from Yumiko")]
        public async Task Help(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            string prefix;
            IDebuggingService mode = new DebuggingService();
            if (mode.RunningInDebugMode())
            {
                prefix = ConfigurationManager.AppSettings["PrefixTest"];
            }
            else
            {
                prefix = ConfigurationManager.AppSettings["PrefixProd"];
            }

            var comandosTopLevel = ctx.SlashCommandsExtension.RegisteredCommands;
            string comandos = string.Empty;

            string descGeneral = "```Yumiko es un bot completamente en español enfocado a comandos de anime como juegos de adivina el personaje, anime o manga```\n";
            string comandosCNext = $"```To see other commands (as NSFW) type the following command: {prefix}help```";

            comandos += "**Available Commands**\n";
            foreach(var cmdPairTopLevel in comandosTopLevel)
            {
                var cmdTopLevel = cmdPairTopLevel.Value;
                foreach(var cmd in cmdTopLevel)
                {
                    if(cmd.Type == ApplicationCommandType.SlashCommand)
                    {
                        comandos += $"`/{cmd.Name}` {cmd.Description}\n";
                    }
                }
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = "About Yumiko",
                Description = descGeneral + comandos + comandosCNext,
                Color = funciones.GetColor()
            };

            string web = ConfigurationManager.AppSettings["Web"];
            DiscordLinkButtonComponent invite = new(ConfigurationManager.AppSettings["Invite"], "Invite me");
            DiscordLinkButtonComponent commands = new($"{web}#commands", "Commands");
            DiscordLinkButtonComponent faq = new($"{web}#faq", "FAQ");
            DiscordLinkButtonComponent vote = new($"https://top.gg/bot/295182825521545218/vote", "Vote me");

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(invite, commands, faq, vote));
        }
    }
}
