using Discord_Bot.Modulos;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using DSharpPlus.VoiceNext;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace Discord_Bot
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        public DiscordActivity Activity { get; private set; }
        public LavalinkExtension Lavalink { get; private set; }

        public async Task RunAsync()
        {
            var Config = new DiscordConfiguration
            {
                Token = ConfigurationManager.AppSettings["DiscordAPIKey"],
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LogLevel = LogLevel.Info,
                UseInternalLogHandler = true,
            };
            Client = new DiscordClient(Config);

            Client.Ready += OnClientReady;
            Client.GuildAvailable += this.Client_GuildAvailable;
            Client.ClientErrored += this.Client_ClientError;

            Client.UseInteractivity(new InteractivityConfiguration());
            Client.UseVoiceNext();

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { ConfigurationManager.AppSettings["Prefix"] },
                EnableMentionPrefix = true,
                EnableDms = false,
                DmHelp = false,
                EnableDefaultHelp = true
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            this.Commands.CommandExecuted += this.Commands_CommandExecuted;
            this.Commands.CommandErrored += this.Commands_CommandErrored;

            var lavaconfig = new LavalinkConfiguration
            {
                RestEndpoint = new ConnectionEndpoint { Hostname = "localhost", Port = 2333 },
                SocketEndpoint = new ConnectionEndpoint { Hostname = "localhost", Port = 2333 },
                Password = "biguwu"
            };

            Lavalink = Client.UseLavalink();

            Commands.RegisterCommands<Administracion>();
            Commands.RegisterCommands<Memes>();
            Commands.RegisterCommands<Misc>();
            Commands.RegisterCommands<Musica>();
            Commands.RegisterCommands<Help>();
            //Commands.RegisterCommands<Test>();
            //Commands.RegisterCommands<TestBotVoiceCommands>();
            Commands.RegisterCommands<TestBotLavaCommands>();

            await Client.ConnectAsync();

            await Task.Delay(1000); // esperar a que autentifique xd
            await Client.UpdateStatusAsync(new DiscordActivity { ActivityType = ActivityType.Playing, Name = ConfigurationManager.AppSettings["Prefix"]  + "help"}, UserStatus.Online);
            //await Lavalink.ConnectAsync(lavaconfig);
            await Task.Delay(-1);
        }

        private Task OnClientReady(ReadyEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "Yumiko", "El cliente esta listo para procesar eventos.", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task Client_GuildAvailable(GuildCreateEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "Yumiko", $"Servidor disponible: {e.Guild.Name}", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task Client_ClientError(ClientErrorEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Error, "Yumiko", $"Ha ocurrido una excepcion: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, e.Context.Guild.Name, $"{e.Context.User.Username} ejecuto el comando '{e.Command.QualifiedName}'", DateTime.Now);
            return Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, e.Context.Guild.Name, $"{e.Context.User.Username} trato de ejecutar '{e.Command?.QualifiedName ?? "<comando desconocido>"}' pero falló: {e.Exception.GetType()}: {e.Exception.Message ?? "<sin mensaje>"}", DateTime.Now);
            if (e.Exception is ChecksFailedException ex)
            {
                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Acceso denegado",
                    Description = $"{emoji} No tienes los suficientes permisos para ejecutar este comando.",
                    Color = new DiscordColor(0xFF0000)
                };
                await e.Context.RespondAsync("", embed: embed);
            }
        }
    }
}
