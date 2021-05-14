using Discord_Bot.Modulos;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YumikoBot;
using YumikoBot.DAL;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

namespace Discord_Bot
{
    public class Yumiko
    {
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }

        private DiscordChannel LogChannelGeneral;

        private DiscordChannel LogChannelServers;

        private DiscordChannel LogChannelErrores;

        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        private bool Debug;

        public async Task RunAsync()
        {
            var json = string.Empty;
            using (var fs = File.OpenRead("config.json"))
            {
                using var sr = new StreamReader(fs, new UTF8Encoding(false));
                json = await sr.ReadToEndAsync().ConfigureAwait(false);
            }

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            string token, prefix;
            IDebuggingService mode = new DebuggingService();
            Debug = mode.RunningInDebugMode();
            if (Debug)
            {
                token = configJson.TokenTest;
                prefix = ConfigurationManager.AppSettings["PrefixTest"];
            }
            else
            {
                token = configJson.TokenProd;
                prefix = ConfigurationManager.AppSettings["PrefixProd"];
            }

            var Config = new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                ReconnectIndefinitely = true,
                MinimumLogLevel = LogLevel.Information,
                Intents = DiscordIntents.All
            };
            Client = new DiscordClient(Config);

            Client.Ready += OnClientReady;
            Client.ClientErrored += Client_ClientError;
            Client.GuildCreated += Client_GuildCreated;
            Client.GuildDeleted += Client_GuildDeleted;
            Client.Resumed += Client_Resumed;

            Client.UseInteractivity(new InteractivityConfiguration());
            Client.UseVoiceNext();

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { prefix },
                EnableMentionPrefix = true,
                EnableDms = false,
                DmHelp = false,
                EnableDefaultHelp = false,
                CaseSensitive = false,
                IgnoreExtraArguments = true
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            Commands.SetHelpFormatter<CustomHelpFormatter>();

            Commands.CommandExecuted += Commands_CommandExecuted;
            Commands.CommandErrored += Commands_CommandErrored;

            Commands.RegisterCommands<Interactuar>();
            Commands.RegisterCommands<Fun>();
            Commands.RegisterCommands<Anilist>();
            Commands.RegisterCommands<Juegos>();
            Commands.RegisterCommands<NSFW>();
            Commands.RegisterCommands<Usuarios>();
            Commands.RegisterCommands<Otros>();
            Commands.RegisterCommands<Help>();

            await Client.ConnectAsync(new DiscordActivity { ActivityType = ActivityType.Playing, Name = prefix + "help | yumiko.uwu.ai" }, UserStatus.Online);

            var LogGuild = await Client.GetGuildAsync(713809173573271613);
            if (Debug)
            {
                LogChannelGeneral = LogGuild.GetChannel(820711607796891658);
                LogChannelServers = LogGuild.GetChannel(840440818921897985);
                LogChannelErrores = LogGuild.GetChannel(840440877565739008);
            }
            else
            {
                LogChannelGeneral = LogGuild.GetChannel(781679685838569502);
                LogChannelServers = LogGuild.GetChannel(840437931847974932);
                LogChannelErrores = LogGuild.GetChannel(840439731011452959);
            }
                
            await RotarEstado(prefix);
        }

        private async Task RotarEstado(string prefix)
        {
            while (true)
            {
                await Task.Delay(30000);
                await Client.UpdateStatusAsync(new DiscordActivity { ActivityType = ActivityType.Playing, Name = "yumiko.uwu.ai" }, UserStatus.Online);
                await Task.Delay(10000);
                await Client.UpdateStatusAsync(new DiscordActivity { ActivityType = ActivityType.Playing, Name = prefix + "help" }, UserStatus.Online);
            }
        }

        private Task OnClientReady(DiscordClient c, ReadyEventArgs e)
        {
            e.Handled = true;
            c.Logger.LogInformation("El cliente esta listo para procesar eventos.", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task Client_Resumed(DiscordClient c, ReadyEventArgs e)
        {
            e.Handled = true;
            c.Logger.LogInformation("El cliente vuelve a estar listo para procesar eventos.", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task Client_GuildCreated(DiscordClient c, GuildCreateEventArgs e)
        {
            e.Handled = true;
            LogChannelServers.SendMessageAsync(embed: new DiscordEmbedBuilder()
            {
                Author = new EmbedAuthor()
                {
                    IconUrl = e.Guild.IconUrl,
                    Name = $"{e.Guild.Name}"
                },
                Title = "Nuevo servidor",
                Description =
                $"   **Id**: {e.Guild.Id}\n" +
                $"   **Miembros**: {e.Guild.MemberCount - 1}\n" +
                $"   **Owner**: {e.Guild.Owner.Username}#{e.Guild.Owner.Discriminator}",
                Footer = new EmbedFooter()
                {
                    Text = $"{DateTimeOffset.Now}"
                },
                Color = DiscordColor.Green
            });
            return Task.CompletedTask;
        }

        private Task Client_GuildDeleted(DiscordClient sender, GuildDeleteEventArgs e)
        {
            e.Handled = true;
            LogChannelServers.SendMessageAsync(embed: new DiscordEmbedBuilder()
            {
                Author = new EmbedAuthor()
                {
                    IconUrl = e.Guild.IconUrl,
                    Name = $"{e.Guild.Name}"
                },
                Title = "Bye-bye servidor",
                Description =
                $"   **Id**: {e.Guild.Id}\n" +
                $"   **Miembros**: {e.Guild.MemberCount - 1}\n" +
                $"   **Owner**: {e.Guild.Owner.Username}#{e.Guild.Owner.Discriminator}",
                Footer = new EmbedFooter()
                {
                    Text = $"{DateTimeOffset.Now}"
                },
                Color = DiscordColor.Red
            });
            return Task.CompletedTask;
        }

        private Task Client_ClientError(DiscordClient c, ClientErrorEventArgs e)
        {
            e.Handled = true;
            if (e.Exception.Message != "An event handler caused the invocation of an asynchronous event to time out.")
            {
                LogChannelErrores.SendMessageAsync(embed: new DiscordEmbedBuilder()
                {
                    Title = "Ha ocurrido una excepcion",
                    Footer = new EmbedFooter()
                    {
                        Text = $"{DateTimeOffset.Now}"
                    },
                    Color = DiscordColor.Red
                }.AddField("Tipo", $"{e.Exception.GetType()}", false)
                .AddField("Descripcion", $"{e.Exception.Message}", false)
                .AddField("Evento", $"{e.EventName}", false)
                );
            }
            return Task.CompletedTask;
        }

        private Task Commands_CommandExecuted(CommandsNextExtension cm, CommandExecutionEventArgs e)
        {
            e.Handled = true;
            LogChannelGeneral.SendMessageAsync(embed: new DiscordEmbedBuilder()
            {
                Title = "Comando ejecutado",
                Footer = new EmbedFooter()
                {
                    Text = $"{e.Context.User.Username}#{e.Context.User.Discriminator} - {e.Context.Message.Timestamp}",
                    IconUrl = e.Context.User.AvatarUrl
                },
                Author = new EmbedAuthor()
                {
                    IconUrl = e.Context.Guild.IconUrl,
                    Name = $"{e.Context.Guild.Name}"
                },
                Color = DiscordColor.Green
            }.AddField("Canal", $"#{e.Context.Channel.Name}", false)
            .AddField("Mensaje", $"{e.Context.Message.Content}", false)
            );
            if (e.Context.Message != null && funciones.ChequearPermisoYumiko(e.Context, Permissions.ManageMessages))
                e.Context.Message.DeleteAsync("Auto borrado de Yumiko");
            return Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandsNextExtension cm, CommandErrorEventArgs e)
        {
            e.Handled = true;
            string web = ConfigurationManager.AppSettings["Web"] + "#commands";
            if (e.Exception.Message == "Specified command was not found.")
            {
                await LogChannelGeneral.SendMessageAsync(embed: new DiscordEmbedBuilder()
                {
                    Title = "Comando no encontrado",
                    Footer = new EmbedFooter()
                    {
                        Text = $"{e.Context.Message.Timestamp}"
                    },
                    Author = new EmbedAuthor()
                    {
                        IconUrl = e.Context.User.AvatarUrl,
                        Name = $"{e.Context.User.Username}#{e.Context.User.Discriminator}"
                    },
                    Color = DiscordColor.Yellow
                }.AddField("Servidor", $"{e.Context.Guild.Name}", false)
                .AddField("Canal", $"#{e.Context.Channel.Name}", false)
                .AddField("Mensaje", $"{e.Context.Message.Content}", false)
                );

                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Comando no encontrado",
                    Description = $"Puedes ver los comandos disponibles en mi [página web]({web}).",
                    Color = DiscordColor.Red,
                    Footer = new EmbedFooter()
                    {
                        Text = $"Invocado por {e.Context.Member.DisplayName} ({e.Context.Member.Username}#{e.Context.Member.Discriminator})",
                        IconUrl = e.Context.Member.AvatarUrl
                    }
                };
                var mensajeErr = e.Context.RespondAsync(embed: embed);
                if(e.Context.Message != null && mensajeErr != null && funciones.ChequearPermisoYumiko(e.Context, Permissions.ManageMessages))
                {
                    await Task.Delay(7000);
                    await e.Context.Message.DeleteAsync("Auto borrado de yumiko");
                    await mensajeErr.Result.DeleteAsync("Auto borrado de yumiko");
                }
            }
            else if (e.Exception.Message == "Could not find a suitable overload for the command.")
            {
                await LogChannelGeneral.SendMessageAsync(embed: new DiscordEmbedBuilder()
                {
                    Title = "Comando mal escrito",
                    Footer = new EmbedFooter()
                    {
                        Text = $"{e.Context.Message.Timestamp}"
                    },
                    Author = new EmbedAuthor()
                    {
                        IconUrl = e.Context.User.AvatarUrl,
                        Name = $"{e.Context.User.Username}#{e.Context.User.Discriminator}"
                    },
                    Color = DiscordColor.Yellow
                }.AddField("Servidor", $"{e.Context.Guild.Name}", false)
                .AddField("Canal", $"#{e.Context.Channel.Name}", false)
                .AddField("Mensaje", $"{e.Context.Message.Content}", false)
                );

                var emoji = DiscordEmoji.FromName(e.Context.Client, ":warning:");
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Comando mal escrito",
                    Description = $"Puedes ver ejemplos de como usar los comandos en mi [página web]({web}).",
                    Color = DiscordColor.Yellow,
                    Footer = new EmbedFooter()
                    {
                        Text = $"Invocado por {e.Context.Member.DisplayName} ({e.Context.Member.Username}#{e.Context.Member.Discriminator}) | {e.Context.Prefix}{e.Command.Name}",
                        IconUrl = e.Context.Member.AvatarUrl
                    }
                };
                var mensajeErr = e.Context.RespondAsync(embed: embed);
                if (e.Context.Message != null && mensajeErr != null && funciones.ChequearPermisoYumiko(e.Context, Permissions.ManageMessages))
                {
                    await Task.Delay(7000);
                    await e.Context.Message.DeleteAsync("Auto borrado de yumiko");
                    await mensajeErr.Result.DeleteAsync("Auto borrado de yumiko");
                }
            }
            else if (e.Exception.Message == "Unauthorized: 403")
            {
                await LogChannelGeneral.SendMessageAsync(embed: new DiscordEmbedBuilder()
                {
                    Title = "Permisos faltantes",
                    Footer = new EmbedFooter()
                    {
                        Text = $"{e.Context.Message.Timestamp}"
                    },
                    Author = new EmbedAuthor()
                    {
                        IconUrl = e.Context.User.AvatarUrl,
                        Name = $"{e.Context.User.Username}#{e.Context.User.Discriminator}"
                    },
                    Color = DiscordColor.Red
                }.AddField("Servidor", $"{e.Context.Guild.Name}", false)
                .AddField("Canal", $"#{e.Context.Channel.Name}", false)
                .AddField("Mensaje", $"{e.Context.Message.Content}", false)
                );

                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Permisos faltantes",
                    Description = $"{emoji} Me faltan permisos para poder ejecutar este comando.",
                    Color = DiscordColor.Red
                };
                var mensajeErr = e.Context.RespondAsync(embed: embed);
                if (e.Context.Message != null && mensajeErr != null && funciones.ChequearPermisoYumiko(e.Context, Permissions.ManageMessages))
                {
                    await Task.Delay(3000);
                    await e.Context.Message.DeleteAsync("Auto borrado de yumiko");
                    await mensajeErr.Result.DeleteAsync("Auto borrado de yumiko");
                } 
            }
            else
            {
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
                            case "DSharpPlus.CommandsNext.Attributes.RequirePermissionsAttribute":
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
                        var msg = e.Context.RespondAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = titulo,
                            Description = descripcion,
                            Color = new DiscordColor(0xFF0000),
                            Footer = footer
                        });
                        mensajes.Add(msg.Result);
                        await LogChannelGeneral.SendMessageAsync(embed: new DiscordEmbedBuilder {
                            Title = titulo,
                            Description = descripcion,
                            Footer = new EmbedFooter()
                            {
                                Text = $"{e.Context.Message.Timestamp}"
                            },
                            Author = new EmbedAuthor()
                            {
                                IconUrl = e.Context.User.AvatarUrl,
                                Name = $"{e.Context.User.Username}#{e.Context.User.Discriminator}"
                            },
                            Color = DiscordColor.Yellow
                        }.AddField("Servidor", $"{e.Context.Guild.Name}", false)
                        .AddField("Canal", $"#{e.Context.Channel.Name}", false)
                        .AddField("Mensaje", $"{e.Context.Message.Content}", false));
                    }
                    if (funciones.ChequearPermisoYumiko(e.Context, Permissions.ManageMessages))
                    {
                        await Task.Delay(3000);
                        if (e.Context.Message != null)
                            await e.Context.Message.DeleteAsync("Auto borrado de Yumiko");
                        foreach (DiscordMessage mensaje in mensajes)
                        {
                            await mensaje.DeleteAsync("Auto borrado de Yumiko");
                        }
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
                    var msg = e.Context.RespondAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "Error desconocido",
                        Description = "Ha ocurrido un error que no puedo manejar",
                        Color = new DiscordColor(0xFF0000),
                        Footer = footer
                    });
                    await LogChannelErrores.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "Error desconocido",
                        Description = $"{e.Exception.Message}",
                        Color = DiscordColor.Red,
                        Footer = footer
                    }.AddField("Servidor", $"{e.Context.Guild.Name}", false)
                    .AddField("Canal", $"#{e.Context.Channel.Name}", false)
                    .AddField("Mensaje", $"{e.Context.Message.Content}", false));
                    if (funciones.ChequearPermisoYumiko(e.Context, Permissions.ManageMessages))
                    {
                        await Task.Delay(3000);
                        if (e.Context.Message != null)
                            await e.Context.Message.DeleteAsync("Auto borrado de Yumiko");
                        await msg.Result.DeleteAsync("Auto borrado de Yumiko");
                    }
                }
            }
        }
    }
}
