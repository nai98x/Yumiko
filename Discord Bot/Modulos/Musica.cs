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

namespace Discord_Bot.Modulos
{
    public class Musica : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

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
            await ctx.RespondAsync("Me he desconectado, no me extrañes " + ctx.Member.Mention + " onii-chan");
        }

        /* Clausurado porque no anda xd
        [Command("play")]
        public async Task Play(CommandContext ctx, [RemainingText] string file)
        {
            var vnext = ctx.Client.GetVoiceNext();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.RespondAsync("No estoy conectada al canal, baka");
                return;
            }

            char c = (char)92;
            string archivo = "C:" + c + "Users" + c + "Mariano" + c + "Music" + c + "Openings" + c + file + ".mp3";

            if (!File.Exists(archivo))
            {
                await ctx.RespondAsync("No se ha encontrado el archivo");
                return;
            }
                
            await ctx.RespondAsync("👌");
            await vnc.SendSpeakingAsync(true); 

            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $@"-i ""{archivo}"" -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            
            try
            {
                var ffmpeg = Process.Start(psi); // kaboom
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            var ffout = ffmpeg.StandardOutput.BaseStream;

            var txStream = vnc.GetTransmitStream();
            await ffout.CopyToAsync(txStream);
            await txStream.FlushAsync();

            await vnc.WaitForPlaybackFinishAsync(); 
        }*/

    }
}
