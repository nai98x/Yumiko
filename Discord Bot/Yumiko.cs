using Discord_Bot.Modulos;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Lavalink;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

namespace Discord_Bot
{
    public class Yumiko
    {
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }

        public async Task RunAsync()
        {
            var Config = new DiscordConfiguration
            {
                Token = ConfigurationManager.AppSettings["DiscordAPIKey"],
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Information
            };
            Client = new DiscordClient(Config);

            Client.Ready += OnClientReady;
            Client.GuildAvailable += Client_GuildAvailable;
            Client.ClientErrored += Client_ClientError;

            Client.UseInteractivity(new InteractivityConfiguration());
            Client.UseVoiceNext();

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { ConfigurationManager.AppSettings["Prefix"], "yumiko" },
                EnableMentionPrefix = true,
                EnableDms = false,
                DmHelp = false,
                EnableDefaultHelp = false
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.CommandExecuted += Commands_CommandExecuted;
            Commands.CommandErrored += Commands_CommandErrored;

            Commands.RegisterCommands<Interactuar>();
            Commands.RegisterCommands<Fun>();
            Commands.RegisterCommands<Anilist>();
            Commands.RegisterCommands<Juegos>();
            Commands.RegisterCommands<Otros>();
            Commands.RegisterCommands<Help>();

            await Client.ConnectAsync();

            await Task.Delay(1000); // esperar a que autentifique
            await Client.UpdateStatusAsync(new DiscordActivity { ActivityType = ActivityType.Playing, Name = ConfigurationManager.AppSettings["Prefix"]  + "help | yumiko.uwu.ai | Desarrollado con <3 por Nai" }, UserStatus.Online);
            await Task.Delay(-1);
        }

        private Task OnClientReady(ReadyEventArgs e)
        {
            e.Client.Logger.LogInformation("El cliente esta listo para procesar eventos.", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task Client_GuildAvailable(GuildCreateEventArgs e)
        {
            e.Client.Logger.LogInformation($"Servidor disponible: {e.Guild.Name}", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task Client_ClientError(ClientErrorEventArgs e)
        {
            e.Client.Logger.LogError($"Ha ocurrido una excepcion: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            e.Context.Client.Logger.LogInformation($"{e.Context.User.Username} ejecuto el comando '{e.Command.QualifiedName}'", DateTime.Now);
            return Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            if (e.Exception.Message == "Specified command was not found." || e.Exception.Message == "Could not find a suitable overload for the command.")
            {
                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Comando mal escrito",
                    Description = $"{emoji} Pone el comando bien, " + e.Context.User.Username + " baka.",
                    Color = new DiscordColor(0xFF0000)
                };
                var mensajeErr = await e.Context.RespondAsync("", embed: embed);
                await Task.Delay(3000);
                await e.Context.Message.DeleteAsync("Auto borrado de yumiko");
                await mensajeErr.DeleteAsync("Auto borrado de yumiko");
            }
            else
            {
                e.Context.Client.Logger.LogInformation($"{e.Context.User.Username} trato de ejecutar '{e.Command?.QualifiedName ?? "<comando desconocido>"}' pero falló: {e.Exception.GetType()}: {e.Exception.Message ?? "<sin mensaje>"}", DateTime.Now);
                if (e.Exception is ChecksFailedException ex)
                {
                    List<DiscordMessage> mensajes = new List<DiscordMessage>();
                    foreach (var exep in ex.FailedChecks)
                    {
                        string exepcion = exep.ToString();
                        string titulo, descripcion;
                        switch (exepcion)
                        {
                            case "DSharpPlus.CommandsNext.Attributes.CooldownAttribute":
                                titulo = "Cooldown";
                                descripcion = "Debes esperar para volver a ejecutar este comando.";
                                break;
                            case "DSharpPlus.CommandsNext.Attributes.RequirePermissions":
                                titulo = "Acceso denegado";
                                descripcion = "No tienes los suficientes permisos para ejecutar este comando.";
                                break;
                            case "DSharpPlus.CommandsNext.Attributes.RequireOwnerAttribute":
                                titulo = "Acceso denegado";
                                descripcion = "Solo el dueño del bot puede ejecutar este comando.";
                                break;
                            case "DSharpPlus.CommandsNext.Attributes.RequireNsfwAttribute":
                                titulo = "Requiere NSFW";
                                descripcion = "Este comando debe ser invocado en un canal NSFW.";
                                break;
                            default:
                                titulo = "Error inesperado";
                                descripcion = "Ha ocurrido un error que no puedo manejar.";
                                break;
                        }
                        var miembro = e.Context.Member;
                        EmbedFooter footer = new EmbedFooter()
                        {
                            Text = "Invocado por " + miembro.DisplayName + " (" + miembro.Username + "#" + miembro.Discriminator + ")",
                            IconUrl = miembro.AvatarUrl
                        };
                        DiscordMessage msg = await e.Context.RespondAsync("", embed: new DiscordEmbedBuilder
                        {
                            Title = titulo,
                            Description = descripcion,
                            Color = new DiscordColor(0xFF0000),
                            Footer = footer
                        });
                        mensajes.Add(msg);
                    }
                    await Task.Delay(3000);
                    await e.Context.Message.DeleteAsync("Auto borrado de yumiko");
                    foreach (DiscordMessage mensaje in mensajes)
                    {
                        await mensaje.DeleteAsync("Auto borrado de Yumiko");
                    }
                }
                else
                {
                    var miembro = e.Context.Member;
                    EmbedFooter footer = new EmbedFooter()
                    {
                        Text = "Invocado por " + miembro.DisplayName + " (" + miembro.Username + "#" + miembro.Discriminator + ")",
                        IconUrl = miembro.AvatarUrl
                    };
                    DiscordMessage msg = await e.Context.RespondAsync("", embed: new DiscordEmbedBuilder
                    {
                        Title = "Error desconocido",
                        Description = "Ha ocurrido un error que no puedo manejar",
                        Color = new DiscordColor(0xFF0000),
                        Footer = footer
                    });
                    await Task.Delay(3000);
                    await e.Context.Message.DeleteAsync("Auto borrado de yumiko");
                    await msg.DeleteAsync("Auto borrado de Yumiko");
                }
            }
        }
    }
}
