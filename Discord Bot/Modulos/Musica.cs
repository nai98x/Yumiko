using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.Net;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

namespace Discord_Bot.Modulos
{
    public class Musica : BaseCommandModule
    {
        private LavalinkNodeConnection Lavalink { get; set; }
        private LavalinkGuildConnection LavalinkVoice { get; set; }
        private DiscordChannel ContextChannel { get; set; }

        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command, Description("Conecta con Lavalink")]
        public async Task ConnectAsync(CommandContext ctx)
        {
            if (this.Lavalink != null)
                return;

            var lava = ctx.Client.GetLavalink();
            if (lava == null)
            {
                await ctx.RespondAsync("Lavalink no está configurado correctamente.").ConfigureAwait(false);
                return;
            }

            this.Lavalink = await lava.ConnectAsync(new LavalinkConfiguration
            {
                RestEndpoint = new ConnectionEndpoint { Hostname = "localhost", Port = 2333 },
                SocketEndpoint = new ConnectionEndpoint { Hostname = "localhost", Port = 2333 },
                Password = "shallnotpass"
            }).ConfigureAwait(false);


            this.Lavalink.Disconnected += this.Lavalink_Disconnected;
            await ctx.RespondAsync("Conectada a lavalink.").ConfigureAwait(false);
        }

        private Task Lavalink_Disconnected(NodeDisconnectedEventArgs e)
        {
            this.Lavalink = null;
            this.LavalinkVoice = null;
            return Task.CompletedTask;
        }

        [Command, Description("Se desconecta de Lavalink")]
        public async Task DisconnectAsync(CommandContext ctx)
        {
            if (this.Lavalink == null)
                return;

            var lava = ctx.Client.GetLavalink();
            if (lava == null)
            {
                await ctx.RespondAsync("Lavalink no está configurado correctamente.").ConfigureAwait(false);
                return;
            }

            await this.Lavalink.StopAsync().ConfigureAwait(false);
            this.Lavalink = null;
            await ctx.RespondAsync("Desconectada de Lavalink.").ConfigureAwait(false);
        }

        [Command, Description("Entra a un canal de voz.")]
        public async Task JoinAsync(CommandContext ctx, DiscordChannel chn = null)
        {
            if (this.Lavalink == null)
            {
                await ctx.RespondAsync("Falta conectar a Lavalink.").ConfigureAwait(false);
                return;
            }

            var vc = chn ?? ctx.Member.VoiceState.Channel;
            if (vc == null)
            {
                await ctx.RespondAsync("No estas en un canal de voz.").ConfigureAwait(false);
                return;
            }

            this.LavalinkVoice = await this.Lavalink.ConnectAsync(vc);
            this.LavalinkVoice.PlaybackFinished += this.LavalinkVoice_PlaybackFinished;
            await ctx.RespondAsync("Me he conectado.").ConfigureAwait(false);
        }

        private async Task LavalinkVoice_PlaybackFinished(TrackFinishEventArgs e)
        {
            if (this.ContextChannel == null)
                return;

            await this.ContextChannel.SendMessageAsync($"La reproducción de {Formatter.Bold(Formatter.Sanitize(e.Track.Title))} ha terminado.").ConfigureAwait(false);
            this.ContextChannel = null;
        }

        [Command, Description("Deja un canal de voz.")]
        public async Task LeaveAsync(CommandContext ctx)
        {
            if (this.LavalinkVoice == null)
                return;

            await this.LavalinkVoice.DisconnectAsync().ConfigureAwait(false);
            this.LavalinkVoice = null;
            await ctx.RespondAsync("No me extrañes " + ctx.Member.DisplayName + " onii-chan.").ConfigureAwait(false);
        }

        [Command, Description("Reproduce una canción.")]
        public async Task PlayAsync(CommandContext ctx, [RemainingText] String query)
        {
            if (this.LavalinkVoice == null)
                return;

            this.ContextChannel = ctx.Channel;

            Uri uri;
            string titulo;
            if (Uri.IsWellFormedUriString(query, UriKind.Absolute))
            {
                uri = new Uri(query);
                var trackLoad = await this.Lavalink.Rest.GetTracksAsync(uri);
                var track = trackLoad.Tracks.First();
                titulo = track.Title;
                await this.LavalinkVoice.PlayAsync(track);
            }
            else
            {
                var trackLoad = await this.Lavalink.Rest.GetTracksAsync(query);
                var track = trackLoad.Tracks.First();
                uri = track.Uri;
                titulo = track.Title;
                await this.LavalinkVoice.PlayAsync(track);
            }

            await ctx.Message.DeleteAsync().ConfigureAwait(false);
            EmbedFooter footer = new EmbedFooter()
            {
                Text = "Invocado por " + funciones.GetFooter(ctx),
                IconUrl = ctx.Member.AvatarUrl
            };
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder
            {
                Footer = footer,
                Color = new DiscordColor(78, 63, 96),
                Title = "Play",
                Description = "Se está reproduciendo [" + titulo + "](" + uri + ")",
                Timestamp = DateTime.Now
            }).ConfigureAwait(false);
        }

        [Command, Description("Reproduce una canción desde archivos locales.")]
        public async Task PlayFileAsync(CommandContext ctx, [RemainingText] string path)
        {
            if (this.LavalinkVoice == null)
                return;

            var trackLoad = await this.Lavalink.Rest.GetTracksAsync(new FileInfo(path));
            var track = trackLoad.Tracks.First();
            await this.LavalinkVoice.PlayAsync(track);

            await ctx.RespondAsync($"Reproduciendo: {Formatter.Bold(Formatter.Sanitize(track.Title))}.").ConfigureAwait(false);
        }

        [Command, Description("Reproduce una canción desde soundcloud.")]
        public async Task PlaySoundCloudAsync(CommandContext ctx, string search)
        {
            if (this.Lavalink == null)
                return;

            var result = await this.Lavalink.Rest.GetTracksAsync(search, LavalinkSearchType.SoundCloud);
            var track = result.Tracks.First();
            await this.LavalinkVoice.PlayAsync(track);

            await ctx.RespondAsync($"Reproduciendo: {Formatter.Bold(Formatter.Sanitize(track.Title))}.");
        }

        [Command, Description("Reproduce una cancion.")]
        public async Task PlayPartialAsync(CommandContext ctx, TimeSpan start, TimeSpan stop, [RemainingText] Uri uri)
        {
            if (this.LavalinkVoice == null)
                return;

            var trackLoad = await this.Lavalink.Rest.GetTracksAsync(uri);
            var track = trackLoad.Tracks.First();
            await this.LavalinkVoice.PlayPartialAsync(track, start, stop);

            await ctx.RespondAsync($"Reproduciendo: {Formatter.Bold(Formatter.Sanitize(track.Title))}.").ConfigureAwait(false);
        }

        [Command, Description("Pausa la musica.")]
        public async Task PauseAsync(CommandContext ctx)
        {
            if (this.LavalinkVoice == null)
                return;

            await this.LavalinkVoice.PauseAsync();
            await ctx.RespondAsync("Se ha pausado la reproducción.").ConfigureAwait(false);
        }

        [Command, Description("Resume la musica.")]
        public async Task ResumeAsync(CommandContext ctx)
        {
            if (this.LavalinkVoice == null)
                return;

            await this.LavalinkVoice.ResumeAsync();
            await ctx.RespondAsync("Se ha reanudado la reproducción.").ConfigureAwait(false);
        }

        [Command, Description("Para la musica.")]
        public async Task StopAsync(CommandContext ctx)
        {
            if (this.LavalinkVoice == null)
                return;

            await this.LavalinkVoice.StopAsync();
            await ctx.RespondAsync("Se ha parado la reproducción.").ConfigureAwait(false);
        }

        [Command, Description("Seeks in the current track.")]
        public async Task SeekAsync(CommandContext ctx, TimeSpan position)
        {
            if (this.LavalinkVoice == null)
                return;

            await this.LavalinkVoice.SeekAsync(position);
            await ctx.RespondAsync($"Moviendo al minuto {position}.").ConfigureAwait(false);
        }

        [Command, Description("Cambia el volumen.")]
        public async Task VolumeAsync(CommandContext ctx, int volume)
        {
            if (this.LavalinkVoice == null)
                return;

            if (volume < 0 || volume > 100)
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync("El volumen debe estar entre los valores 0-100").ConfigureAwait(false);
                return;
            }

            await this.LavalinkVoice.SetVolumeAsync(volume);
            await ctx.RespondAsync($"El volumen se ha cambiado a {volume}%.").ConfigureAwait(false);
        }

        [Command, Description("Muestra que se está reproduciendo."), Aliases("np")]
        public async Task NowPlayingAsync(CommandContext ctx)
        {
            if (this.LavalinkVoice == null)
                return;

            var state = this.LavalinkVoice.CurrentState;
            var track = state.CurrentTrack;
            await ctx.RespondAsync($"Reproduciendo: {Formatter.Bold(Formatter.Sanitize(track.Title))} by {Formatter.Bold(Formatter.Sanitize(track.Author))} [{state.PlaybackPosition}/{track.Length}].").ConfigureAwait(false);
        }

        [Command, Description("Cambia la configuración del ecualizador."), Aliases("eq")]
        public async Task EqualizerAsync(CommandContext ctx)
        {
            if (this.LavalinkVoice == null)
                return;

            await this.LavalinkVoice.ResetEqualizerAsync();
            await ctx.RespondAsync("Equalizador reseteado.").ConfigureAwait(false);
        }

        [Command]
        public async Task EqualizerAsync(CommandContext ctx, int band, float gain)
        {
            if (this.LavalinkVoice == null)
                return;

            await this.LavalinkVoice.AdjustEqualizerAsync(new LavalinkBandAdjustment(band, gain));
            await ctx.RespondAsync($"Band {band} adjusted by {gain}").ConfigureAwait(false);
        }

        [Command("earrape"), Description("Eleva el volumen para hacer tremendisimo earrape")]
        [Cooldown(1, 300, CooldownBucketType.Guild)]
        public async Task Earrape(CommandContext ctx)
        {
            EmbedFooter footer = new EmbedFooter()
            {
                Text = "Invocado por " + funciones.GetFooter(ctx),
                IconUrl = ctx.Member.AvatarUrl
            };
            await ctx.Message.DeleteAsync().ConfigureAwait(false);
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder
            {
                Footer = footer,
                Color = new DiscordColor(78, 63, 96),
                Title = "EARRAPE",
                ImageUrl = "https://sound.peal.io/ps/covers/000/007/954/large/BF37E882-92E5-4E45-9DBE-A46E195D1601.gif",
                Timestamp = DateTime.Now
            }).ConfigureAwait(false);
            await this.LavalinkVoice.SetVolumeAsync(1000);
            await Task.Delay(10000); // 10 segundos
            await this.LavalinkVoice.SetVolumeAsync(100);
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync("El EARRAPE ha terminado!").ConfigureAwait(false);
        }
    }
}

