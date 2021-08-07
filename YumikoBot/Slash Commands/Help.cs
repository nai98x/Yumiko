using Discord_Bot;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace YumikoBot
{
    public class HelpSlashCommands : SlashCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new();

        [SlashCommand("help", "Informacion y ayuda de Yumiko")]
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
            string comandosCNext = $"```Para ver otros comandos (como los NSFW) escribe el siguiente comando: {prefix}help```";

            comandos += "**Comandos Disponibles**\n";
            foreach(var cmdPairTopLevel in comandosTopLevel)
            {
                var cmdTopLevel = cmdPairTopLevel.Value;
                foreach(var cmd in cmdTopLevel)
                {
                    comandos += $"`/{cmd.Name}` {cmd.Description}\n";
                }
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = "Acerca de Yumiko",
                Description = descGeneral + comandos + comandosCNext,
                Color = funciones.GetColor()
            };

            string web = ConfigurationManager.AppSettings["Web"];
            DiscordLinkButtonComponent invite = new(ConfigurationManager.AppSettings["Invite"], "Invítame");
            DiscordLinkButtonComponent commands = new($"{web}#commands", "Comandos");
            DiscordLinkButtonComponent faq = new($"{web}#faq", "FAQ");
            DiscordLinkButtonComponent vote = new($"https://top.gg/bot/295182825521545218/vote", "Vótame");

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(invite, commands, faq, vote));
        }
    }
}
