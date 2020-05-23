using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
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

namespace Discord_Bot.Modulos
{
    public class Musica// : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("join")]
        [Aliases("entrar")]
        [Description("Entra al canal")]
        public async Task Join(CommandContext ctx, DiscordChannel chn = null)
        {
            var vnext = ctx.Client.GetVoiceNextClient();
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
            var vnext = ctx.Client.GetVoiceNextClient();
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


       /* public async Task SendAudioAsync(CommandContext ctx, string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    await ctx.Channel.SendMessageAsync("File does not exist.");
                    return;
                }
                AudioClient client;
                if (ConnectedChannels.TryGetValue(ctx.Guild.Id, out client))
                {
                    //await Log.d($"Starting playback of \"{path}\" in \"{guild.Name}\"", src);
                    var OutFormat = new WaveFormat(48000, 16, 2);

                    using (var MP3Reader = new Mp3FileReader(path)) // Create a new Disposable MP3FileReader, to read audio from the filePath parameter
                    using (var resampler = new MediaFoundationResampler(MP3Reader, OutFormat)) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
                    {
                        resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
                        int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
                        byte[] buffer = new byte[blockSize];
                        int byteCount;
                        while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0) // Read audio into our buffer, and keep a loop open while data is present
                        {
                            if (byteCount < blockSize)
                            {
                                // Incomplete Frame
                                for (int i = byteCount; i < blockSize; i++)
                                    buffer[i] = 0;
                            }
                            using (var output = client.CreatePCMStream(AudioApplication.Mixed))
                                await output.WriteAsync(buffer, 0, blockSize); // Send the buffer to Discord
                        }
                    }
                }
            }
            catch (Exception e)
            {
                await ctx.RespondAsync(e.Message);
            }
        }*/
        /*
        [Command("play")]
        public async Task Play(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await Join(ctx, null);
                vnc = vnext.GetConnection(ctx.Guild);
            }

            string filePath = @"C:\Users\Mariano\Music\Openings\Evangelion.mp3";

            if (!File.Exists(filePath))
            {
                await ctx.RespondAsync("No se ha encontrado el archivo");
                return;
            }
                
            await ctx.RespondAsync("👌");
            await vnc.SendSpeakingAsync(true); 

            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $@"-i ""{filePath}"" -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            
            try
            {
                var ffmpeg = Process.Start(psi);
            }
            catch(Exception e)
            {
                await ctx.RespondAsync(e.Message);
            }

            var ffout = ffmpeg.StandardOutput.BaseStream;

            var txStream = vnc.GetTransmitStream();
            await ffout.CopyToAsync(txStream);
            await txStream.FlushAsync();
            
            await vnc.WaitForPlaybackFinishAsync(); 
            await vnc.SendSpeakingAsync(false);
            


        }
        */
    }
}
