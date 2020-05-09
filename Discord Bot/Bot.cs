﻿using Discord_Bot.Modulos;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
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
        //public VoiceNext voice { get; private set; }

        public async Task RunAsync()
        {
            var json = string.Empty;

            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            var Config = new DiscordConfiguration
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true,
            };
            Client = new DiscordClient(Config);

            Client.Ready += OnClientReady;

            Client.UseInteractivity(new InteractivityConfiguration());

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] {configJson.Prefix},
                EnableMentionPrefix = true,
                EnableDms = false,
                DmHelp = false,
                //EnableDefaultHelp = false cuando esten todos los comandos descomentar
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<Administracion>();
            Commands.RegisterCommands<Memes>();
            Commands.RegisterCommands<Misc>();

            await Client.ConnectAsync();

            //DiscordActivity act = new DiscordActivity("type *help");
            //await Client.UpdateStatusAsync(act,UserStatus.Online);

            await Task.Delay(-1);

            
        }

        private Task OnClientReady(ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}
