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
    [Group("lavalink"), Description("Provides audio playback via lavalink."), Aliases("lava", "l")]
    public class TestBotLavaCommands : BaseCommandModule
    {
        private LavalinkNodeConnection Lavalink { get; set; }
        private LavalinkGuildConnection LavalinkVoice { get; set; }
        private DiscordChannel ContextChannel { get; set; }

        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command ("SConnect")]
        public async Task SuperConnect(CommandContext ctx)
        {
            await ConnectAsync(ctx);
            await JoinAsync(ctx);
            await LeaveAsync(ctx);
            await JoinAsync(ctx);
        }

        [Command, Description("Connects to Lavalink")]
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

        [Command, Description("Disconnects from Lavalink")]
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

        [Command, Description("Joins a voice channel.")]
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

        [Command, Description("Leaves a voice channel.")]
        public async Task LeaveAsync(CommandContext ctx)
        {
            if (this.LavalinkVoice == null)
                return;

            await this.LavalinkVoice.DisconnectAsync().ConfigureAwait(false);
            this.LavalinkVoice = null;
            await ctx.RespondAsync("No me extrañes " + ctx.Member.DisplayName + " onii-chan.").ConfigureAwait(false);
        }

        [Command, Description("Queues tracks for playback.")]
        public async Task PlayAsync(CommandContext ctx, [RemainingText] Uri uri)
        {
            if (this.LavalinkVoice == null)
                return;

            this.ContextChannel = ctx.Channel;

            var trackLoad = await this.Lavalink.Rest.GetTracksAsync(uri);
            var track = trackLoad.Tracks.First();
            await this.LavalinkVoice.PlayAsync(track);

            //await ctx.RespondAsync($"Reproduciendo: {Formatter.Bold(Formatter.Sanitize(track.Title))}.").ConfigureAwait(false);
            await ctx.Message.DeleteAsync().ConfigureAwait(false);
            EmbedFooter footer = new EmbedFooter()
            {
                Text = "Preguntado por " + funciones.GetFooter(ctx)
            };
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder
            {
                Footer = footer,
                Color = DiscordColor.Green,
                Title = track.Title,
                Url = uri.ToString(),
                Timestamp = DateTime.Now
            }).ConfigureAwait(false);
        }

        [Command, Description("Queues tracks for playback.")]
        public async Task PlayFileAsync(CommandContext ctx, [RemainingText] string path)
        {
            if (this.LavalinkVoice == null)
                return;

            var trackLoad = await this.Lavalink.Rest.GetTracksAsync(new FileInfo(path));
            var track = trackLoad.Tracks.First();
            await this.LavalinkVoice.PlayAsync(track);

            await ctx.RespondAsync($"Reproduciendo: {Formatter.Bold(Formatter.Sanitize(track.Title))}.").ConfigureAwait(false);
        }

        [Command, Description("Queues track for playback.")]
        public async Task PlaySoundCloudAsync(CommandContext ctx, string search)
        {
            if (this.Lavalink == null)
                return;

            var result = await this.Lavalink.Rest.GetTracksAsync(search, LavalinkSearchType.SoundCloud);
            var track = result.Tracks.First();
            await this.LavalinkVoice.PlayAsync(track);

            await ctx.RespondAsync($"Reproduciendo: {Formatter.Bold(Formatter.Sanitize(track.Title))}.");
        }

        [Command, Description("Queues tracks for playback.")]
        public async Task PlayPartialAsync(CommandContext ctx, TimeSpan start, TimeSpan stop, [RemainingText] Uri uri)
        {
            if (this.LavalinkVoice == null)
                return;

            var trackLoad = await this.Lavalink.Rest.GetTracksAsync(uri);
            var track = trackLoad.Tracks.First();
            await this.LavalinkVoice.PlayPartialAsync(track, start, stop);

            await ctx.RespondAsync($"Reproduciendo: {Formatter.Bold(Formatter.Sanitize(track.Title))}.").ConfigureAwait(false);
        }

        [Command, Description("Pauses playback.")]
        public async Task PauseAsync(CommandContext ctx)
        {
            if (this.LavalinkVoice == null)
                return;

            await this.LavalinkVoice.PauseAsync();
            await ctx.RespondAsync("Se ha pausado la reproducción.").ConfigureAwait(false);
        }

        [Command, Description("Resumes playback.")]
        public async Task ResumeAsync(CommandContext ctx)
        {
            if (this.LavalinkVoice == null)
                return;

            await this.LavalinkVoice.ResumeAsync();
            await ctx.RespondAsync("Se ha reanudado la reproducción.").ConfigureAwait(false);
        }

        [Command, Description("Stops playback.")]
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

        [Command, Description("Changes playback volume.")]
        public async Task VolumeAsync(CommandContext ctx, int volume)
        {
            if (this.LavalinkVoice == null)
                return;

            await this.LavalinkVoice.SetVolumeAsync(volume);
            await ctx.RespondAsync($"El volumen se ha cambiado a {volume}%.").ConfigureAwait(false);
        }

        [Command, Description("Shows what's being currently played."), Aliases("np")]
        public async Task NowPlayingAsync(CommandContext ctx)
        {
            if (this.LavalinkVoice == null)
                return;

            var state = this.LavalinkVoice.CurrentState;
            var track = state.CurrentTrack;
            await ctx.RespondAsync($"Reproduciendo: {Formatter.Bold(Formatter.Sanitize(track.Title))} by {Formatter.Bold(Formatter.Sanitize(track.Author))} [{state.PlaybackPosition}/{track.Length}].").ConfigureAwait(false);
        }

        [Command, Description("Sets or resets equalizer settings."), Aliases("eq")]
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

        [Command, Description("Displays Lavalink statistics.")]
        public async Task StatsAsync(CommandContext ctx)
        {
            if (this.LavalinkVoice == null)
                return;

            var stats = this.Lavalink.Statistics;
            var sb = new StringBuilder();
            sb.Append("Lavalink resources usage statistics: ```")
                .Append("Uptime:                    ").Append(stats.Uptime).AppendLine()
                .Append("Players:                   ").AppendFormat("{0} active / {1} total", stats.ActivePlayers, stats.TotalPlayers).AppendLine()
                .Append("CPU Cores:                 ").Append(stats.CpuCoreCount).AppendLine()
                .Append("CPU Usage:                 ").AppendFormat("{0:#,##0.0%} lavalink / {1:#,##0.0%} system", stats.CpuLavalinkLoad, stats.CpuSystemLoad).AppendLine()
                .Append("RAM Usage:                 ").AppendFormat("{0} allocated / {1} used / {2} free / {3} reservable", SizeToString(stats.RamAllocated), SizeToString(stats.RamUsed), SizeToString(stats.RamFree), SizeToString(stats.RamReservable)).AppendLine()
                .Append("Audio frames (per minute): ").AppendFormat("{0:#,##0} sent / {1:#,##0} nulled / {2:#,##0} deficit", stats.AverageSentFramesPerMinute, stats.AverageNulledFramesPerMinute, stats.AverageDeficitFramesPerMinute).AppendLine()
                .Append("```");
            await ctx.RespondAsync(sb.ToString()).ConfigureAwait(false);
        }

        private static string[] Units = new[] { "", "ki", "Mi", "Gi" };
        private static string SizeToString(long l)
        {
            double d = l;
            int u = 0;
            while (d >= 900 && u < Units.Length - 2)
            {
                u++;
                d /= 1024;
            }

            return $"{d:#,##0.00} {Units[u]}B";
        }
    }
}
