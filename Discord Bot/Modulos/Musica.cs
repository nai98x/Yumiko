using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
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
        private int Volumen { get; set; } = 100;

        private List<Track> Queue = new List<Track>();
        private CommandContext contexto { get; set; }
        private int posArchivo { get; set; }

        [Command]
        public async Task Test(CommandContext ctx)
        {
            foreach(Track t in Queue)
            {
                await ctx.RespondAsync(t.Id + " " + t.Link);
            }
        }

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

            string host = ConfigurationManager.AppSettings["LavalinkHost"];
            int port = Int32.Parse(ConfigurationManager.AppSettings["LavalinkPort"]);
            string pass = ConfigurationManager.AppSettings["LavalinkPass"];
            this.Lavalink = await lava.ConnectAsync(new LavalinkConfiguration
            {
                RestEndpoint = new ConnectionEndpoint { Hostname = host, Port = port },
                SocketEndpoint = new ConnectionEndpoint { Hostname = host, Port = port },
                Password = "shallnotpass"
            }).ConfigureAwait(false);

            Lavalink.Disconnected += Lavalink_Disconnected;
            Lavalink.PlaybackStarted += Lavalink_PlaybackStarted;
            Lavalink.PlaybackFinished += Lavalink_PlaybackFinished;
            Lavalink.TrackException += Lavalink_TrackException;
        }

        private Task Lavalink_Disconnected(NodeDisconnectedEventArgs e)
        {
            this.Lavalink = null;
            this.LavalinkVoice = null;
            return Task.CompletedTask;
        }

        private Task Lavalink_PlaybackStarted(TrackStartEventArgs e)
        {
            var link = e.Track.Uri;
            if(link != null)
            {
                Queue.Add(new Track
                {
                    Id = e.Track.Identifier,
                    Link = link,
                    Titulo = e.Track.Title,
                    Source = "Internet"
                });
            }
            else
            {
                Queue.Add(new Track
                {
                    Id = e.Track.Identifier,
                    PosLocal = posArchivo,
                    Titulo = e.Track.Title,
                    Source = "Local"
                });
            }
            return Task.CompletedTask;
        }

        private Task Lavalink_PlaybackFinished(TrackFinishEventArgs e)
        {
            Track findQueue = Queue.Find(r => r.Id == e.Track.Identifier);
            Queue.Remove(findQueue);
            if(Queue.Count > 0)
            {
                Track primero = Queue.First();
                switch (primero.Source)
                {
                    case "Internet":
                        Queue.Remove(primero);
                        _ = this.PlayAsync(contexto, primero.Link.ToString());
                        break;
                    case "Local":
                        _ = this.PlayFileAsync(contexto, primero.PosLocal);
                        break;
                }
            }
            return Task.CompletedTask;
        }

        private Task Lavalink_TrackException(TrackExceptionEventArgs e)
        {
            return Task.CompletedTask;
        }

        [Command, Description("Se desconecta de Lavalink"), Hidden]
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
                await ConnectAsync(ctx);
            }
            var vc = chn ?? ctx.Member?.VoiceState?.Channel;
            if (vc == null)
            {
                await ctx.RespondAsync("No estas en ningun canal, baka").ConfigureAwait(false);
                return;
            }
            if (chn == null)
            {
                chn = ctx.Channel;
            }
            contexto = ctx;
            this.LavalinkVoice = await this.Lavalink.ConnectAsync(vc);
            this.LavalinkVoice.PlaybackFinished += this.LavalinkVoice_PlaybackFinished;
            await ctx.RespondAsync($"Me he conectado a `{chn.Name}`").ConfigureAwait(false);
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
            await ctx.RespondAsync("Me he desconectado, no me extrañes " + ctx.Member.DisplayName + " onii-chan.").ConfigureAwait(false);
        }

        [Command, Description("Reproduce una canción.")]
        public async Task PlayAsync(CommandContext ctx, [RemainingText] String query)
        {
            if (this.LavalinkVoice == null)
                await JoinAsync(ctx);

            this.ContextChannel = ctx.Channel;

            Uri uri;
            string titulo;
            LavalinkTrack track;
            if (Uri.IsWellFormedUriString(query, UriKind.Absolute))
            {
                uri = new Uri(query);
                var trackLoad = await this.Lavalink.Rest.GetTracksAsync(uri);
                track = trackLoad.Tracks.First();
                titulo = track.Title;
            }
            else
            {
                var trackLoad = await this.Lavalink.Rest.GetTracksAsync(query);
                track = trackLoad.Tracks.First();
                uri = track.Uri;
                titulo = track.Title;
            }

            if(track != null)
            {
                if(Queue.Count == 0)
                {
                    await this.LavalinkVoice.PlayAsync(track);
                    DiscordMessage mensajeBorrar = await ctx.Channel.GetMessageAsync(ctx.Message.Id);
                    if(mensajeBorrar != null)
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
                else
                {
                    await ctx.Message.DeleteAsync().ConfigureAwait(false);
                    Queue.Add(new Track
                    {
                        Id = track.Identifier,
                        Link = track.Uri,
                        Source = "Internet",
                        Titulo = titulo
                    });
                    await ctx.RespondAsync($"Se ha añadido `{titulo}` a la lista de reproducción").ConfigureAwait(false);
                }
            }
        }

        [Command, Description("Reproduce una canción desde archivos locales.")]
        public async Task PlayFileAsync(CommandContext ctx, [RemainingText] int posicion)
        {
            if (this.LavalinkVoice == null)
                await JoinAsync(ctx);

            string archivo = funciones.GetCancionByPosicion(posicion);

            string filePath = ConfigurationManager.AppSettings["PathMusica"] + archivo + ".mp3";

            if (!File.Exists(filePath))
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync("No se ha encontrado el archivo");
                return;
            }

            posArchivo = posicion;
            var trackLoad = await this.Lavalink.Rest.GetTracksAsync(new FileInfo(filePath));
            var track = trackLoad.Tracks.First();

            if (track != null)
            {
                if (Queue.Count == 0)
                {
                    await this.LavalinkVoice.PlayAsync(track);
                    DiscordMessage mensajeBorrar = await ctx.Channel.GetMessageAsync(ctx.Message.Id);
                    if (mensajeBorrar != null)
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
                        Description = "Se está reproduciendo ```" + archivo + "```",
                        Timestamp = DateTime.Now
                    }).ConfigureAwait(false);
                }
                else
                {
                    await ctx.Message.DeleteAsync().ConfigureAwait(false);
                    Queue.Add(new Track
                    {
                        Id = track.Identifier,
                        PosLocal = posicion,
                        Source = "Local",
                        Titulo = archivo
                    });
                    await ctx.RespondAsync($"Se ha añadido `{archivo}` a la lista de reproducción").ConfigureAwait(false);
                }
            }
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

        [Command, Description("Skipea la musica.")]
        public async Task SkipAsync(CommandContext ctx)
        {
            if (this.LavalinkVoice == null)
                return;

            await this.LavalinkVoice.StopAsync();
            await ctx.RespondAsync($"**{ctx.User.Mention}** ha skipeado la reproducción de {LavalinkVoice.CurrentState.CurrentTrack.Title}.").ConfigureAwait(false);
        }

        [Command, Description("Para la musica.")]
        public async Task StopAsync(CommandContext ctx)
        {
            if (this.LavalinkVoice == null)
                return;

            await this.LavalinkVoice.StopAsync();
            Queue.Clear();
            await ctx.RespondAsync("Se ha parado la reproducción.").ConfigureAwait(false);
            await this.LavalinkVoice.DisconnectAsync().ConfigureAwait(false);
            this.LavalinkVoice = null;
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
                Volumen = volume;
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

        [Command("archivos"), Aliases("listado")]
        [Description("Da un listado de los temasos disponibles")]
        public async Task ListadoMusica(CommandContext ctx)
        {
            string[] filePaths = Directory.GetFiles(ConfigurationManager.AppSettings["PathMusica"]);
            int lenghtPath = ConfigurationManager.AppSettings["PathMusica"].Length;

            string listado = "";
            int n = 1;
            foreach (string file in filePaths)
            {
                string preString = file.Remove(0, lenghtPath);
                listado += n.ToString() + "- " + preString.Remove(preString.Length - 4) + "\n";
                n++;
            }
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder
            {
                Color = new DiscordColor(78, 63, 96),
                Description = listado
            }).ConfigureAwait(false);
        }

        [Command("queue")]
        [Description("Trae la cola de reproducción actual")]
        public async Task QueueAsync(CommandContext ctx)
        {
            string listado = "";

            if(Queue.Count > 0)
            {
                int n = 1;
                foreach (Track track in Queue)
                {
                    listado += n.ToString() + "- " + track.Titulo + "\n";
                    n++;
                }
            }
            else
            {
                listado = "No hay elementos en la cola de reproducción";
            }

            EmbedFooter footer = new EmbedFooter()
            {
                Text = "Invocado por " + funciones.GetFooter(ctx),
                IconUrl = ctx.Member.AvatarUrl
            };
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Cola de reproducción",
                Color = new DiscordColor(78, 63, 96),
                Description = listado,
                Footer = footer,
                Timestamp = DateTime.Now
            }).ConfigureAwait(false);
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
            await Task.Delay(5000); // 5 segundos
            await this.LavalinkVoice.SetVolumeAsync(Volumen);
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync("El EARRAPE ha terminado!").ConfigureAwait(false);
        }
    }
}