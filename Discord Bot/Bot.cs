using Discord_Bot.Modulos;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using DSharpPlus.VoiceNext;
using Newtonsoft.Json;
using System.IO;
using System.Text;
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
                Token = "Mjk1MTgyODI1NTIxNTQ1MjE4.XqNETg.R04GlssFnqFwkLxaZYZVj-GNnJs",
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true,
            };
            Client = new DiscordClient(Config);

            Client.Ready += OnClientReady;

            Client.UseInteractivity(new InteractivityConfiguration());
            Client.UseVoiceNext();

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { "*" },
                EnableMentionPrefix = true,
                EnableDms = false,
                DmHelp = false,
                //EnableDefaultHelp = false cuando esten todos los comandos descomentar
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            var lavaconfig = new LavalinkConfiguration
            {
                RestEndpoint = new ConnectionEndpoint { Hostname = "localhost", Port = 2333 },
                SocketEndpoint = new ConnectionEndpoint { Hostname = "localhost", Port = 80 },
                Password = "youshallnotpass"
            };

            Lavalink = Client.UseLavalink();
            //await Lavalink.ConnectAsync(lavaconfig);

            Commands.RegisterCommands<Administracion>();
            Commands.RegisterCommands<Memes>();
            Commands.RegisterCommands<Misc>();
            Commands.RegisterCommands<Musica>();
            //Commands.RegisterCommands<TestBotVoiceCommands>();
            //Commands.RegisterCommands<TestBotLavaCommands>();

            await Client.ConnectAsync();

            //await Client.UpdateStatusAsync(new DiscordActivity { ActivityType = ActivityType.Watching, Name = "Por ayuda: *help" }, UserStatus.Online);

            await Task.Delay(1000); // esperar a que autentifique xd
            await Client.UpdateStatusAsync(new DiscordActivity { ActivityType = ActivityType.Playing, Name = "*help" }, UserStatus.Online);
            await Task.Delay(-1);
        }

        private Task OnClientReady(ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}
