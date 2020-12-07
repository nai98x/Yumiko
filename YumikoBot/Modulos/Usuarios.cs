using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;
using System.Collections.Generic;
using DSharpPlus.Entities;
using System;
using GraphQL.Client.Http;
using GraphQL;
using GraphQL.Client.Serializer.Newtonsoft;
using System.Linq;
using System.Configuration;
using DSharpPlus.Interactivity.Extensions;
using YumikoBot.Data_Access_Layer;
using System.Globalization;

namespace Discord_Bot.Modulos
{
    public class Usuarios : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();
        private readonly UsuariosDiscord usuariosService = new UsuariosDiscord();

        [Command("cumpleaños"), Aliases("birthday"), Description("Muestra los próximos cumpleaños del mes."), RequireGuild]
        public async Task Birthdays(CommandContext ctx, string flag = null)
        {
            List<UsuarioDiscord> lista;
            string titulo;
            if (!String.IsNullOrEmpty(flag) && flag == "-all")
            {
                lista = usuariosService.GetBirthdays(ctx, false);
                titulo = "Próximos cumpleaños";
            }

            else
            {
                lista = usuariosService.GetBirthdays(ctx, true);
                titulo = "Próximos cumpleaños en este mes";
            }
                
            string desc = "";
            foreach (var user in lista)
            {
                var miembro = await ctx.Guild.GetMemberAsync((ulong)user.Id);
                int anios = DateTime.Now.Year - user.Birthday.Year;
                DateTime nuevaFecha;
                if (DateTime.Now > new DateTime(day: user.Birthday.Day, month: user.Birthday.Month, year: DateTime.Now.Year))
                {
                    anios += 1;
                    nuevaFecha = new DateTime(day: user.Birthday.Day, month: user.Birthday.Month, year: DateTime.Now.Year + 1);
                }
                else
                {
                    nuevaFecha = new DateTime(day: user.Birthday.Day, month: user.Birthday.Month, year: DateTime.Now.Year);
                }   
                string dia = nuevaFecha.ToString("dddd", CultureInfo.CreateSpecificCulture("es"));
                string mes = nuevaFecha.ToString("MMMM", CultureInfo.CreateSpecificCulture("es"));
                if(user.MostrarYear ?? false)
                    desc += $"- **{miembro.DisplayName}** ({miembro.Username}#{miembro.Discriminator}) - Cumple **{anios} años** el {dia} {user.Birthday.Day} de {mes}\n";
                else
                    desc += $"- **{miembro.DisplayName}** ({miembro.Username}#{miembro.Discriminator}) - Cumple el {dia} {user.Birthday.Day} de {mes}\n";
            }
            if (String.IsNullOrEmpty(desc))
            {
                desc = "(No hay ningún usuario registrado que cumpla años este mes)";
            }
            
            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor(),
                Title = titulo,
                Description = desc
            }).ConfigureAwait(false);
        }

        [Command("setcumpleaños"), Aliases("setbirthday"), Description("Agrega o modifica el cumpleaños del usuario."), RequireGuild]
        public async Task SetBirthday(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            var msgFecha = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Escribe tu fecha de nacimiento",
                Description = "En este formato: **dd/mm/yyyy**\n  Ejemplo: 31/01/2000"
            });
            var msgFechaInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGeneral"])));
            if (!msgFechaInter.TimedOut)
            {
                bool result = DateTime.TryParse(msgFechaInter.Result.Content, out DateTime fecha);
                await msgFechaInter.Result.DeleteAsync("Auto borrado de yumiko");
                await msgFecha.DeleteAsync("Auto borrado de yumiko");
                if (result)
                {
                    var msgOcultar = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "¿Quieres que se muestre tu edad?",
                        Description = "1- Si\n2- No"
                    });
                    var msgOcultarInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGeneral"])));
                    if (!msgOcultarInter.TimedOut)
                    {
                        bool result2 = int.TryParse(msgOcultarInter.Result.Content, out int mostrarEdadInt);
                        if (result2)
                        {
                            await msgOcultarInter.Result.DeleteAsync("Auto borrado de yumiko");
                            await msgOcultar.DeleteAsync("Auto borrado de yumiko");
                            bool mostrarEdad;
                            switch (mostrarEdadInt)
                            {
                                case 1:
                                    mostrarEdad = true;
                                    break;
                                case 2:
                                    mostrarEdad = false;
                                    break;
                                default:
                                    await ctx.RespondAsync("Ingresa bien la respuesta, baka");
                                    return;
                            }
                            usuariosService.SetBirthday(ctx, fecha, mostrarEdad);
                        }
                        else
                        {
                            await ctx.RespondAsync("Ingresa bien la respuesta, baka");
                        }
                    }
                    else
                    {
                        await ctx.RespondAsync("Tiempo agotado esperando la respuesta");
                    }
                }
                else
                {
                    await ctx.RespondAsync("Ingresa bien la fecha, baka");
                }
            }
            else
            {
                await ctx.RespondAsync("Tiempo agotado esperando la fecha de nacimiento");
            }
        }

        [Command("borrarcumpleaños"), Aliases("deletebirthday", "deletecumpleaños"), Description("Borra el cumpleaños del usuario."), RequireGuild]
        public async Task DeleteBirthday(CommandContext ctx)
        {
            usuariosService.DeleteBirthday(ctx);
            await ctx.RespondAsync("Cumpleaños borrado correctamente");
        }
    }
}
