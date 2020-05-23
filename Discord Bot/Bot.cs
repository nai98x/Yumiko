using Discord_Bot.Modulos;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
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
        //public InteractivityExtension Interactivity { get; private set; }
        public CommandsNextModule Commands { get; private set; }

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
                StringPrefix = "*",
                EnableMentionPrefix = true,
                EnableDms = false,
                CaseSensitive = false,
                
                //EnableDefaultHelp = false cuando esten todos los comandos descomentar
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<Administracion>();
            Commands.RegisterCommands<Memes>();
            Commands.RegisterCommands<Misc>();
            Commands.RegisterCommands<Musica>();

            await Client.ConnectAsync();

            //await Client.UpdateStatusAsync(new DiscordActivity { ActivityType = ActivityType.Watching, Name = "Por ayuda: *help" }, UserStatus.Online);

            await Task.Delay(1000); // esperar a que autentifique xd
            //await Client.UpdateStatusAsync(new DiscordActivity { ActivityType = ActivityType.Playing, Name = "*help" }, UserStatus.Online);
            await Client.UpdateStatusAsync(new DiscordGame {Name = "*help" }, UserStatus.Online);
            await Task.Delay(-1);
        }

        private Task OnClientReady(ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}
