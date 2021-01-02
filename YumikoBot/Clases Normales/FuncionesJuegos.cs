using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using YumikoBot.Data_Access_Layer;

namespace Discord_Bot
{
    public class FuncionesJuegos
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();
        private readonly LeaderboardGeneral leaderboard = new LeaderboardGeneral();

        public async Task GetResultados(CommandContext ctx, List<UsuarioJuego> participantes, int rondas, string dificultad, string juego)
        {
            string resultados;
            if (juego == "tag")
                resultados = $"Tag: **{dificultad}**\n\n";
            else
                resultados = $"Dificultad: **{dificultad}**\n\n";
            participantes.Sort((x, y) => y.Puntaje.CompareTo(x.Puntaje));
            int tot = 0;
            int pos = 0;
            int lastScore = 0;
            foreach (UsuarioJuego uj in participantes)
            {
                if (lastScore != uj.Puntaje)
                    pos++;
                int porcentaje = (uj.Puntaje * 100) / rondas;
                switch (pos)
                {
                    case 1:
                        DiscordEmoji emoji1 = DiscordEmoji.FromName(ctx.Client, ":first_place:");
                        resultados += $"{emoji1} - **{uj.Usuario.Username}#{uj.Usuario.Discriminator}**: {uj.Puntaje} aciertos ({porcentaje}%)\n";
                        break;
                    case 2:
                        DiscordEmoji emoji2 = DiscordEmoji.FromName(ctx.Client, ":second_place:");
                        resultados += $"{emoji2} - **{uj.Usuario.Username}#{uj.Usuario.Discriminator}**: {uj.Puntaje} aciertos ({porcentaje}%)\n";
                        break;
                    case 3:
                        DiscordEmoji emoji3 = DiscordEmoji.FromName(ctx.Client, ":third_place:");
                        resultados += $"{emoji3} - **{uj.Usuario.Username}#{uj.Usuario.Discriminator}**: {uj.Puntaje} aciertos ({porcentaje}%)\n";
                        break;
                    default:
                        resultados += $"**#{pos}** - **{uj.Usuario.Username}#{uj.Usuario.Discriminator}**: {uj.Puntaje} aciertos ({porcentaje}%)\n";
                        break;
                }
                lastScore = uj.Puntaje;
                tot += uj.Puntaje;
                leaderboard.AddRegistro(ctx, long.Parse(uj.Usuario.Id.ToString()), dificultad, uj.Puntaje, rondas, juego);
            }
            resultados += $"\n**Total ({tot}/{rondas})**";
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
            {
                Title = $"Resultados - Adivina el {juego}",
                Description = resultados,
                Color = funciones.GetColor()
            }).ConfigureAwait(false);
        }

        public async Task<SettingsJuego> InicializarJuego(CommandContext ctx, InteractivityExtension interactivity)
        {
            var msgCntRondas = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Elige la cantidad de rondas (máximo 100)",
                Description = "Por ejemplo: 10"
            });
            var msgRondasInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGames"])));
            if (!msgRondasInter.TimedOut)
            {
                bool result = int.TryParse(msgRondasInter.Result.Content, out int rondas);
                if (result)
                {
                    if (rondas > 0 && rondas <= 100)
                    {
                        DiscordEmoji emojiDado = DiscordEmoji.FromName(ctx.Client, ":game_die:");
                        var msgDificultad = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = "Elije la dificultad",
                            Description = $"0- Aleatorio {emojiDado}\n1- Fácil\n2- Media\n3- Dificil\n4- Extremo"
                        });
                        var msgDificultadInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGames"])));
                        if (!msgDificultadInter.TimedOut)
                        {
                            result = int.TryParse(msgDificultadInter.Result.Content, out int dificultad);
                            if (result)
                            {
                                string dificultadStr;
                                if (dificultad >= 0 && dificultad <= 4)
                                {
                                    if(dificultad == 0)
                                        dificultad = funciones.GetNumeroRandom(1, 4);
                                    int iterIni;
                                    int iterFin;
                                    string orden;
                                    switch (dificultad)
                                    {
                                        case 1:
                                            iterIni = 1;
                                            iterFin = 10;
                                            dificultadStr = "Fácil";
                                            orden = "FAVOURITES_DESC";
                                            break;
                                        case 2:
                                            iterIni = 10;
                                            iterFin = 30;
                                            dificultadStr = "Media";
                                            orden = "FAVOURITES_DESC";
                                            break;
                                        case 3:
                                            iterIni = 30;
                                            iterFin = 60;
                                            dificultadStr = "Dificil";
                                            orden = "FAVOURITES_DESC";
                                            break;
                                        case 4:
                                            iterIni = 60;
                                            iterFin = 100;
                                            dificultadStr = "Extremo";
                                            orden = "FAVOURITES_DESC";
                                            break;
                                        default:
                                            iterIni = 6;
                                            iterFin = 20;
                                            dificultadStr = "Medio";
                                            orden = "FAVOURITES_DESC";
                                            break;
                                    }
                                    await msgCntRondas.DeleteAsync("Auto borrado de Yumiko");
                                    await msgRondasInter.Result.DeleteAsync("Auto borrado de Yumiko");
                                    await msgDificultad.DeleteAsync("Auto borrado de Yumiko");
                                    await msgDificultadInter.Result.DeleteAsync("Auto borrado de Yumiko");
                                    return new SettingsJuego()
                                    {
                                        Ok = true,
                                        Rondas = rondas,
                                        IterIni = iterIni,
                                        IterFin = iterFin,
                                        Dificultad = dificultadStr,
                                        Orden = orden
                                    };
                                }
                                else
                                {
                                    return new SettingsJuego()
                                    {
                                        Ok = false,
                                        MsgError = "La dificultad debe ser 0, 1, 2, 3 o 4"
                                    };
                                }
                            }
                            else
                            {
                                return new SettingsJuego()
                                {
                                    Ok = false,
                                    MsgError = "La dificultad debe ser un número (0, 1, 2, 3 o 4)"
                                };
                            }
                        }
                        else
                        {
                            return new SettingsJuego()
                            {
                                Ok = false,
                                MsgError = "Tiempo agotado esperando la dificultad"
                            };
                        }
                    }
                    else
                    {
                        return new SettingsJuego()
                        {
                            Ok = false,
                            MsgError = "La cantidad de rondas debe ser mayor a 0 y menor a 100"
                        };
                    }
                }
                else
                {
                    return new SettingsJuego()
                    {
                        Ok = false,
                        MsgError = "La cantidad de rondas debe ser un numero"
                    };
                }
            }
            else
            {
                return new SettingsJuego()
                {
                    Ok = false,
                    MsgError = "Tiempo agotado esperando la cantidad de rondas"
                };
            }
        }

        public async Task<string> GetEstadisticasDificultad(CommandContext ctx, string tipoStats, string dificultad)
        {
            List<StatsJuego> res = leaderboard.GetLeaderboard(ctx, dificultad, tipoStats);
            string stats = "";
            int pos = 0;
            int lastScore = 0;
            DiscordEmoji emoji;
            foreach (var jugador in res)
            {
                if ((jugador.RondasTotales/jugador.PartidasTotales) >= 2)
                {
                    long x = jugador.UserId;
                    ulong id = (ulong)x;
                    DiscordUser miembro = await ctx.Client.GetUserAsync(id);
                    if (miembro != null)
                    {
                        if (lastScore != jugador.PorcentajeAciertos)
                            pos++;
                        switch (pos)
                        {
                            case 1:
                                emoji = DiscordEmoji.FromName(ctx.Client, ":first_place:");
                                stats += $"{emoji} - **{miembro.Username}#{miembro.Discriminator}** - Aciertos: **{jugador.PorcentajeAciertos}%** - Partidas: **{jugador.PartidasTotales}**\n";
                                break;
                            case 2:
                                emoji = DiscordEmoji.FromName(ctx.Client, ":second_place:");
                                stats += $"{emoji} - **{miembro.Username}#{miembro.Discriminator}** - Aciertos: **{jugador.PorcentajeAciertos}%** - Partidas: **{jugador.PartidasTotales}**\n";
                                break;
                            case 3:
                                emoji = DiscordEmoji.FromName(ctx.Client, ":third_place:");
                                stats += $"{emoji} - **{miembro.Username}#{miembro.Discriminator}** - Aciertos: **{jugador.PorcentajeAciertos}%** - Partidas: **{jugador.PartidasTotales}**\n";
                                break;
                            default:
                                stats += $"**#{pos}** - **{miembro.Username}#{miembro.Discriminator}** - Aciertos: **{jugador.PorcentajeAciertos}%** - Partidas: **{jugador.PartidasTotales}**\n";
                                break;
                        }
                        lastScore = jugador.PorcentajeAciertos;
                    }
                }
            }
            return stats;
        }

        public string GetEstadisticasUsuarioDificultad(CommandContext ctx, string tipoStats, DiscordUser usuario, string dificultad, out int partidasTotales, out int rondasAcertadas, out int rondasTotales)
        {
            StatsJuego res = leaderboard.GetStatsUser(ctx, (long)usuario.Id, tipoStats, dificultad);

            if(res != null)
            {
               partidasTotales = res.PartidasTotales;
               rondasAcertadas = res.RondasAcertadas;
               rondasTotales = res.RondasTotales;
               return
                    $"  - Porcentaje de aciertos: **{res.PorcentajeAciertos}%**\n" +
                    $"  - Partidas totales: **{partidasTotales}**\n" +
                    $"  - Rondas acertadas: **{rondasAcertadas}**\n" +
                    $"  - Rondas totales: **{rondasTotales}**\n\n";
            }

            partidasTotales = 0;
            rondasAcertadas = 0;
            rondasTotales = 0;
            return String.Empty;
        }

        public async Task<DiscordEmbedBuilder> GetEstadisticas(CommandContext ctx, string juego)
        {
            string facil = await GetEstadisticasDificultad(ctx, juego, "Fácil");
            string media = await GetEstadisticasDificultad(ctx, juego, "Media");
            string dificil = await GetEstadisticasDificultad(ctx, juego, "Dificil");
            string extremo = await GetEstadisticasDificultad(ctx, juego, "Extremo");

            var builder = CrearEmbedStats(ctx, $"Estadisticas - Adivina el {juego}", facil, media, dificil, extremo);
            return builder;
        }

        public async Task<DiscordEmbedBuilder> GetEstadisticasTag(CommandContext ctx)
        {
            string msgError = "";
            var interactivity = ctx.Client.GetInteractivity();
            List<string> tagsList = leaderboard.GetTags();
            string tags = "";
            int cont = 1;
            foreach(string s in tagsList)
            {
                tags += $"{cont} - {s}\n";
                cont++;
            }
            var msgOpciones = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
            {
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor(),
                Title = "Elije el tag escribiendo su número",
                Description = tags
            });
            var msgElegirTagInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGames"])));
            if (!msgElegirTagInter.TimedOut)
            {
                bool result = int.TryParse(msgElegirTagInter.Result.Content, out int numTagElegir);
                if (result)
                {
                    if (numTagElegir > 0 && (numTagElegir <= tagsList.Count))
                    {
                        await msgOpciones.DeleteAsync("Auto borrado de Yumiko");
                        await msgElegirTagInter.Result.DeleteAsync("Auto borrado de Yumiko");
                        List<Anime> animeList = new List<Anime>();
                        string elegido = tagsList[numTagElegir - 1];
                        string stats = await GetEstadisticasDificultad(ctx, "tag", elegido);
                        return  new DiscordEmbedBuilder
                        {
                            Title = $"Estadisticas - Adivina el {elegido}",
                            Footer = funciones.GetFooter(ctx),
                            Color = funciones.GetColor(),
                            Description = stats
                        };
                    }
                    else
                    {
                        msgError = "El numero que indica el tag debe ser valido";
                    }
                }
                else
                {
                    msgError = "Debes indicar un numero para elegir el tag";
                }
            }
            else
            {
                msgError = "Tiempo agotado esperando el tag";
            }
            await msgElegirTagInter.Result.DeleteAsync("Auto borrado de Yumiko");
            return new DiscordEmbedBuilder
            {
                Title = $"Error!",
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor(),
                Description = msgError
            };
        }

        public DiscordEmbedBuilder GetEstadisticasUsuario(CommandContext ctx, string juego, DiscordUser usuario)
        {
            int partidasTotales = 0;
            int rondasAcertadas = 0;
            int rondasTotales = 0;

            string facil = GetEstadisticasUsuarioDificultad(ctx, juego, usuario, "Fácil", out int partidasTotalesF, out int rondasAcertadasF, out int rondasTotalesF);
            partidasTotales += partidasTotalesF;
            rondasAcertadas += rondasAcertadasF;
            rondasTotales += rondasTotalesF;

            string media = GetEstadisticasUsuarioDificultad(ctx, juego, usuario, "Media", out int partidasTotalesM, out int rondasAcertadasM, out int rondasTotalesM);
            partidasTotales += partidasTotalesM;
            rondasAcertadas += rondasAcertadasM;
            rondasTotales += rondasTotalesM;

            string dificil = GetEstadisticasUsuarioDificultad(ctx, juego, usuario, "Dificil", out int partidasTotalesD, out int rondasAcertadasD, out int rondasTotalesD);
            partidasTotales += partidasTotalesD;
            rondasAcertadas += rondasAcertadasD;
            rondasTotales += rondasTotalesD;

            string extremo = GetEstadisticasUsuarioDificultad(ctx, juego, usuario, "Extremo", out int partidasTotalesE, out int rondasAcertadasE, out int rondasTotalesE);
            partidasTotales += partidasTotalesE;
            rondasAcertadas += rondasAcertadasE;
            rondasTotales += rondasTotalesE;

            int porcentajeAciertos = 0;
            if (rondasTotales > 0)
                porcentajeAciertos = (rondasAcertadas * 100) / rondasTotales;
            string totales =
                $"  - Porcentaje de aciertos: **{porcentajeAciertos}%**\n" +
                $"  - Partidas totales: **{partidasTotales}**\n" +
                $"  - Rondas acertadas: **{rondasAcertadas}**\n" +
                $"  - Rondas totales: **{rondasTotales}**\n\n";

            var builder = CrearEmbedStats(ctx, $"Estadisticas de **{usuario.Username}#{usuario.Discriminator}** - Adivina el {juego}", facil, media, dificil, extremo);
            builder.AddField("Totales", totales);

            return builder;
        }

        public DiscordEmbedBuilder CrearEmbedStats(CommandContext ctx, string titulo, string facil, string media, string dificil, string extremo)
        {
            var builder = new DiscordEmbedBuilder
            {
                Title = titulo,
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor()
            };
            if (!String.IsNullOrEmpty(facil))
                builder.AddField("Dificultad Fácil", facil);
            if (!String.IsNullOrEmpty(media))
                builder.AddField("Dificultad Media", media);
            if (!String.IsNullOrEmpty(dificil))
                builder.AddField("Dificultad Dificil", dificil);
            if (!String.IsNullOrEmpty(extremo))
                builder.AddField("Dificultad Extremo", extremo);

            return builder;
        }
    }
}
