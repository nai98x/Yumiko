using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static DSharpPlus.Entities.DiscordEmbedBuilder;
using DSharpPlus.VoiceNext;
using System.Diagnostics;
using System.Linq.Expressions;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using System.Configuration;

namespace Discord_Bot.Modulos
{
    public class Musica : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();
        private int Id { get; set; }
        private VoiceNextConnection vnc { get; set; }

        private List<ColaReproduccion> colaReproduccion = new List<ColaReproduccion>();

        [Command("join")]
        [Aliases("entrar")]
        [Description("Entra al canal")]
        public async Task Join(CommandContext ctx, DiscordChannel chn = null)
        {
            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                await ctx.RespondAsync("Error en la configuración del bot (VoiceNext)");
                return;
            }

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
            {
                await ctx.RespondAsync("Ya estoy conectada, baka");
                return;
            }

            var vstat = ctx.Member?.VoiceState;
            if (vstat?.Channel == null && chn == null)
            {
                await ctx.RespondAsync("No estas en ningun canal, baka");
                return;
            }

            if (chn == null)
                chn = vstat.Channel;

            await vnext.ConnectAsync(chn);
            await ctx.RespondAsync($"Me he conectado a `{chn.Name}`");
        }

        [Command("leave")]
        [Aliases("salir")]
        [Description("Sale del canal")]
        public async Task Leave(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                await ctx.RespondAsync("Error en la configuración del bot (VoiceNext)");
                return;
            }
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.RespondAsync("No estaba conectada, baka");
                return;
            }
            vnc.Disconnect();
            await ctx.RespondAsync("Me he desconectado, no me extrañes " + ctx.Member.DisplayName + " onii-chan");
        }
        
        [Command("play")]
        public async Task Play(CommandContext ctx, [Description("Archivo")]int posicion)
        {
            var vnext = ctx.Client.GetVoiceNext();

            vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await Join(ctx, null);
                vnc = vnext.GetConnection(ctx.Guild);
            }

            string archivo = funciones.GetCancionByPosicion(posicion);

            string filePath = ConfigurationManager.AppSettings["PathMusica"] + archivo + ".mp3";

            if (!File.Exists(filePath))
            {
                await ctx.RespondAsync("No se ha encontrado el archivo");
                return;
            }
                
            await ctx.RespondAsync("Reproduciendo " + archivo + " 🎵");
            await vnc.SendSpeakingAsync(true); 

            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $@"-i ""{filePath}"" -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            
            var ffmpeg = Process.Start(psi);

            Id = ffmpeg.Id;

            var ffout = ffmpeg.StandardOutput.BaseStream;

            var txStream = vnc.GetTransmitStream();
            await ffout.CopyToAsync(txStream);
            await txStream.FlushAsync();

            await vnc.WaitForPlaybackFinishAsync(); 
            await vnc.SendSpeakingAsync(false);
        }

        [Command("archivos")]
        [Description("Da un listado de los temasos disponibles")]
        public async Task ListadoMusica(CommandContext ctx)
        {
            string[] filePaths = Directory.GetFiles(@"C:\Users\Mariano\Music\Yumiko\");

            string path = "";
            int n = 1;
            foreach (string file in filePaths)
            {
                string preString = file.Remove(0, 30); // Cantidad de caracteres del path original
                path += n.ToString() + "- " + preString.Remove(preString.Length-4) + "\n";
                n++;
            }

            await ctx.RespondAsync(path);
        }

        [Command("pause")]
        [Description("Pausa la reproduccion")]
        public async Task Pause(CommandContext ctx)
        {
            vnc.Pause();
            await ctx.RespondAsync("Se pauso la wea");
        }

        [Command("resume")]
        [Description("Reanuda la reproduccion")]
        public async Task Resume(CommandContext ctx)
        {
            await vnc.ResumeAsync();
            await ctx.RespondAsync("Se reanudo la wea");
        }

        [Command("stop")]
        [Description("Detiene la reproduccion")]
        public async Task Stop(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                await ctx.RespondAsync("Error en la configuración del bot (VoiceNext)");
                return;
            }
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.RespondAsync("No estaba conectada, baka");
                return;
            }
            vnc.Disconnect();
            await ctx.RespondAsync("Dejo de hablar si quieres b-baka");
        }

        private bool AgregarTrackAListaDeReproduccion(string track)
        {
            if (vnc.IsPlaying)
            {

            }
            return true;
        }

    }
}

