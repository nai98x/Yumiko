using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using YumikoBot.DAL;

namespace Discord_Bot
{
    public class FuncionesJuegos
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();
        private readonly LeaderboardoGeneral leaderboard = new LeaderboardoGeneral();
        private readonly GraphQLHttpClient graphQLClient = new GraphQLHttpClient("https://graphql.anilist.co", new NewtonsoftJsonSerializer());

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
                await leaderboard.AddRegistro(ctx, long.Parse(uj.Usuario.Id.ToString()), dificultad, uj.Puntaje, rondas, juego);
            }
            resultados += $"\n**Total ({tot}/{rondas})**";
            string titulo;
            if (juego.Contains("ahorcado"))
                titulo = "Ahorcado";
            else
                titulo = $"Adivina el {juego}";
            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder()
            {
                Title = $"Resultados - {titulo}",
                Description = resultados,
                Color = funciones.GetColor(),
                Footer = funciones.GetFooter(ctx)
            }).ConfigureAwait(false);
            await funciones.ChequearVotoTopGG(ctx);
        }

        public async Task<SettingsJuego> InicializarJuego(CommandContext ctx, InteractivityExtension interactivity, bool elegirDificultad, bool elegirTag)
        {
            var msgCntRondas = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
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
                        if (elegirDificultad)
                        {
                            DiscordEmoji emojiDado = DiscordEmoji.FromName(ctx.Client, ":game_die:");
                            var msgDificultad = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
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
                                        if (dificultad == 0)
                                            dificultad = funciones.GetNumeroRandom(1, 4);
                                        int iterIni;
                                        int iterFin;
                                        switch (dificultad)
                                        {
                                            case 1:
                                                iterIni = 1;
                                                iterFin = 10;
                                                dificultadStr = "Fácil";
                                                break;
                                            case 2:
                                                iterIni = 10;
                                                iterFin = 30;
                                                dificultadStr = "Media";
                                                break;
                                            case 3:
                                                iterIni = 30;
                                                iterFin = 60;
                                                dificultadStr = "Dificil";
                                                break;
                                            case 4:
                                                iterIni = 60;
                                                iterFin = 100;
                                                dificultadStr = "Extremo";
                                                break;
                                            default:
                                                iterIni = 6;
                                                iterFin = 20;
                                                dificultadStr = "Medio";
                                                break;
                                        }
                                        if (msgCntRondas != null)
                                            await funciones.BorrarMensaje(ctx, msgCntRondas.Id);
                                        if (msgRondasInter.Result != null)
                                            await funciones.BorrarMensaje(ctx, msgRondasInter.Result.Id);
                                        if (msgDificultad != null)
                                            await funciones.BorrarMensaje(ctx, msgDificultad.Id);
                                        if (msgDificultadInter.Result != null)
                                            await funciones.BorrarMensaje(ctx, msgDificultadInter.Result.Id);
                                        if (!elegirTag)
                                        {
                                            return new SettingsJuego()
                                            {
                                                Ok = true,
                                                Rondas = rondas,
                                                IterIni = iterIni,
                                                IterFin = iterFin,
                                                Dificultad = dificultadStr
                                            };
                                        }
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
                        if (elegirTag)
                        {
                            string query =
                            "query{" +
                            "   MediaTagCollection{" +
                            "       name," +
                            "       description," +
                            "       isAdult" +
                            "   }" +
                            "}";
                            var request = new GraphQLRequest
                            {
                                Query = query
                            };
                            try
                            {
                                var data = await graphQLClient.SendQueryAsync<dynamic>(request);
                                List<Tag> tags = new List<Tag>();
                                foreach (var x in data.Data.MediaTagCollection)
                                {
                                    if ((x.isAdult == "false") || (x.isAdult == true && ctx.Channel.IsNSFW))
                                    {
                                        string nombre = x.name;
                                        string descripcion = x.description;
                                        tags.Add(new Tag()
                                        {
                                            Nombre = nombre,
                                            Descripcion = descripcion
                                        });
                                    }
                                }
                                var preguntaTag = await ctx.Channel.SendMessageAsync("Escribe un tag");
                                var msgTagInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGames"])));
                                if (!msgTagInter.TimedOut)
                                {
                                    int numTag = 0;
                                    string tagResp = string.Empty;
                                    List<Tag> tagsFiltrados = tags.Where(x => x.Nombre.ToLower().Trim().Contains(msgTagInter.Result.Content.ToLower().Trim())).ToList();
                                    if (tagsFiltrados.Count > 0)
                                    {
                                        foreach (Tag t in tagsFiltrados)
                                        {
                                            numTag++;
                                            tagResp += $"{numTag} - {t.Nombre}\n";
                                        }
                                        var msgOpciones = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                                        {
                                            Footer = funciones.GetFooter(ctx),
                                            Color = funciones.GetColor(),
                                            Title = "Elije el tag escribiendo su número",
                                            Description = tagResp
                                        });
                                        var msgElegirTagInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGames"])));
                                        if (!msgElegirTagInter.TimedOut)
                                        {
                                            bool resultTag = int.TryParse(msgElegirTagInter.Result.Content, out int numTagElegir);
                                            if (resultTag)
                                            {
                                                if (numTagElegir > 0 && (numTagElegir <= tagsFiltrados.Count))
                                                {
                                                    await funciones.BorrarMensaje(ctx, preguntaTag.Id);
                                                    await funciones.BorrarMensaje(ctx, msgTagInter.Result.Id);
                                                    await funciones.BorrarMensaje(ctx, msgOpciones.Id);
                                                    await funciones.BorrarMensaje(ctx, msgElegirTagInter.Result.Id);
                                                    return new SettingsJuego()
                                                    {
                                                        Rondas = rondas,
                                                        Tag = tagsFiltrados[numTagElegir - 1].Nombre,
                                                        TagDesc = tagsFiltrados[numTagElegir - 1].Descripcion,
                                                        Ok = true
                                                    };
                                                }
                                                else
                                                {
                                                    return new SettingsJuego()
                                                    {
                                                        Ok = false,
                                                        MsgError = "El numero indicado del tag debe ser válido"
                                                    };
                                                }
                                            }
                                            else
                                            {
                                                return new SettingsJuego()
                                                {
                                                    Ok = false,
                                                    MsgError = "Debes indicar un numero para elegir el tag"
                                                };
                                            }
                                        }
                                        else
                                        {
                                            return new SettingsJuego()
                                            {
                                                Ok = false,
                                                MsgError = "Tiempo agotado esperando la elección del tag"
                                            };
                                        }
                                    }
                                    else
                                    {
                                        return new SettingsJuego()
                                        {
                                            Ok = false,
                                            MsgError = "No se encontro ningun tag"
                                        };
                                    }
                                }
                                else
                                {
                                    return new SettingsJuego()
                                    {
                                        Ok = false,
                                        MsgError = "Tiempo agotado esperando el tag"
                                    };
                                }
                            }
                            catch (Exception)
                            {
                                // Grabar en logerrores instead
                                //DiscordMessage msg = ex.Message switch
                                //{
                                //    _ => await ctx.Channel.SendMessageAsync($"Error inesperado: {ex.Message}").ConfigureAwait(false),
                                //};
                                return new SettingsJuego()
                                {
                                    Ok = false,
                                    MsgError = "Error inesperado eligiendo el tag"
                                };
                            }
                        }
                        else
                        {
                            return new SettingsJuego()
                            {
                                Ok = false,
                                MsgError = "Error de programación, se debe elegir el tag o las rondas"
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

        public async Task Jugar(CommandContext ctx, string juego, dynamic lista, SettingsJuego settings, InteractivityExtension interactivity)
        {
            List<UsuarioJuego> participantes = new List<UsuarioJuego>();
            int lastRonda;
            for (int ronda = 1; ronda <= settings.Rondas; ronda++)
            {
                lastRonda = ronda;
                int random = funciones.GetNumeroRandom(0, lista.Count - 1);
                dynamic elegido = lista[random];
                DiscordEmoji corazon = DiscordEmoji.FromName(ctx.Client, ":heart:");
                string juegoMostrar;
                if (juego == "tag")
                    juegoMostrar = settings.Dificultad;
                else
                    juegoMostrar = juego;
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Gold,
                    Title = $"Adivina el {juegoMostrar}",
                    Description = $"Ronda {ronda} de {settings.Rondas}",
                    ImageUrl = elegido.Image,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"{elegido.Favoritos} {corazon} (nº {elegido.Popularidad} en popularidad)"
                    }
                }).ConfigureAwait(false);
                string desc = string.Empty;
                dynamic predicate = null;
                switch (juego)
                {
                    case "personaje":
                        desc = $"El nombre es: [{elegido.NameFull}]({elegido.SiteUrl})";
                        Character elegidoP = elegido;
                        predicate = new Func<DiscordMessage, bool>(xm => (xm.Channel == ctx.Channel) && (xm.Author.Id != ctx.Client.CurrentUser.Id) && (!xm.Author.IsBot) &&
                            ((xm.Content.ToLower() == "cancelar" && xm.Author == ctx.User) ||
                            (xm.Content.ToLower().Trim() == elegidoP.NameFull.ToLower().Trim()) ||
                            (xm.Content.ToLower().Trim() == elegidoP.NameFirst.ToLower().Trim()) ||
                            (elegidoP.NameLast != null && xm.Content.ToLower().Trim() == elegidoP.NameLast.ToLower().Trim())
                            ));
                        break;
                    case "anime":
                        desc = $"Los animes de [{elegido.NameFull}]({elegido.SiteUrl}) son:\n\n";
                        foreach (Anime anim in elegido.Animes)
                        {
                            desc += $"- [{anim.TitleRomaji}]({anim.SiteUrl})\n";
                        }
                        Character elegidoC = elegido;
                        predicate = new Func<DiscordMessage, bool>(xm => (xm.Channel == ctx.Channel) && (xm.Author.Id != ctx.Client.CurrentUser.Id) && (!xm.Author.IsBot) &&
                            ((xm.Content.ToLower() == "cancelar" && xm.Author == ctx.User) ||
                            (elegidoC.Animes.Find(x => x.TitleEnglish != null && x.TitleEnglish.ToLower().Trim() == xm.Content.ToLower().Trim()) != null) ||
                            (elegidoC.Animes.Find(x => x.TitleRomaji != null && x.TitleRomaji.ToLower().Trim() == xm.Content.ToLower().Trim()) != null) ||
                            (elegidoC.Animes.Find(x => x.Sinonimos.Find(y => y.ToLower().Trim() == xm.Content.ToLower().Trim()) != null) != null)
                            ));
                        break;
                    case "manga":
                        desc = $"El nombre era: [{elegido.TitleRomaji}]({elegido.SiteUrl})";
                        Anime elegidoM = elegido;
                        predicate = new Func<DiscordMessage, bool>(xm => (xm.Channel == ctx.Channel) && (xm.Author.Id != ctx.Client.CurrentUser.Id) && (!xm.Author.IsBot) &&
                            ((xm.Content.ToLower() == "cancelar" && xm.Author == ctx.User) ||
                            (elegidoM.TitleRomaji != null && (xm.Content.ToLower().Trim() == elegido.TitleRomaji.ToLower().Trim())) || (elegido.TitleEnglish != null && (xm.Content.ToLower().Trim() == elegido.TitleEnglish.ToLower().Trim())) ||
                            (elegidoM.Sinonimos.Find(y => y.ToLower().Trim() == xm.Content.ToLower().Trim()) != null)
                            ));
                        break;
                    case "tag":
                        desc = $"El nombre es: [{elegido.TitleRomaji}]({elegido.SiteUrl})";
                        Anime elegidoT = elegido;
                        predicate = new Func<DiscordMessage, bool>(xm => (xm.Channel == ctx.Channel) && (xm.Author.Id != ctx.Client.CurrentUser.Id) && (!xm.Author.IsBot) &&
                            ((xm.Content.ToLower() == "cancelar" && xm.Author == ctx.User) ||
                            (elegido.TitleRomaji != null && (xm.Content.ToLower().Trim() == elegido.TitleRomaji.ToLower().Trim())) || (elegido.TitleEnglish != null && (xm.Content.ToLower().Trim() == elegido.TitleEnglish.ToLower().Trim())) ||
                            (elegidoT.Sinonimos.Find(y => y.ToLower().Trim() == xm.Content.ToLower().Trim()) != null)
                            ));
                        break;
                    case "estudio":
                        string estudiosStr = $"Los estudios de [{elegido.TitleRomaji}]({elegido.SiteUrl}) son:\n";
                        foreach (var studio in elegido.Estudios)
                        {
                            estudiosStr += $"- [{studio.Nombre}]({studio.SiteUrl})\n";
                        }
                        desc = funciones.NormalizarDescription(estudiosStr);
                        Anime elegidoS = elegido;
                        predicate = new Func<DiscordMessage, bool>(xm => (xm.Channel == ctx.Channel) && (xm.Author.Id != ctx.Client.CurrentUser.Id) && (!xm.Author.IsBot) &&
                            ((xm.Content.ToLower() == "cancelar" && xm.Author == ctx.User) ||
                            (elegidoS.Estudios.Find(y => y.Nombre.ToLower().Trim() == xm.Content.ToLower().Trim()) != null)
                            ));
                        break;
                    case "protagonista":
                        string protagonistasStr = $"Los protagonistas de [{elegido.TitleRomaji}]({elegido.SiteUrl}) son:\n";
                        foreach (var personaje in elegido.Personajes)
                        {
                            protagonistasStr += $"- [{personaje.NameFull}]({personaje.SiteUrl})\n";
                        }
                        desc = funciones.NormalizarDescription(protagonistasStr);
                        Anime elegidoPr = elegido;
                        predicate = new Func<DiscordMessage, bool>(xm => (xm.Channel == ctx.Channel) && (xm.Author.Id != ctx.Client.CurrentUser.Id) && (!xm.Author.IsBot) &&
                            ((xm.Content.ToLower() == "cancelar" && xm.Author == ctx.User) ||
                            (elegidoPr.Personajes.Find(x => x.NameFull != null && x.NameFull.ToLower().Trim() == xm.Content.ToLower().Trim()) != null) ||
                            (elegidoPr.Personajes.Find(x => x.NameFirst != null && x.NameFirst.ToLower().Trim() == xm.Content.ToLower().Trim()) != null) ||
                            (elegidoPr.Personajes.Find(x => x.NameLast != null && x.NameLast.ToLower().Trim() == xm.Content.ToLower().Trim()) != null)
                            ));
                        break;
                    default:
                        // Grabar log
                        return;
                }
                var msg = await interactivity.WaitForMessageAsync(predicate, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["GuessTimeGames"])));
                if (!msg.TimedOut)
                {
                    if (msg.Result.Author == ctx.User && msg.Result.Content.ToLower() == "cancelar")
                    {
                        await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = "¡Juego cancelado!",
                            Description = desc,
                            Color = DiscordColor.Red
                        }).ConfigureAwait(false);
                        await GetResultados(ctx, participantes, lastRonda, settings.Dificultad, juego);
                        await ctx.Channel.SendMessageAsync($"El juego ha sido **cancelado** por **{ctx.User.Username}#{ctx.User.Discriminator}**").ConfigureAwait(false);
                        return;
                    }
                    DiscordMember acertador = await ctx.Guild.GetMemberAsync(msg.Result.Author.Id);
                    UsuarioJuego usr = participantes.Find(x => x.Usuario == msg.Result.Author);
                    if (usr != null)
                    {
                        usr.Puntaje++;
                    }
                    else
                    {
                        participantes.Add(new UsuarioJuego()
                        {
                            Usuario = msg.Result.Author,
                            Puntaje = 1
                        });
                    }
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = $"¡**{acertador.DisplayName}** ha acertado!",
                        Description = desc,
                        Color = DiscordColor.Green
                    }).ConfigureAwait(false);
                }
                else
                {
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "¡Nadie ha acertado!",
                        Description = desc,
                        Color = DiscordColor.Red
                    }).ConfigureAwait(false);
                }
                lista.Remove(lista[random]);
            }
            await GetResultados(ctx, participantes, settings.Rondas, settings.Dificultad, juego);
        }

        public async Task<string> GetEstadisticasDificultad(CommandContext ctx, string tipoStats, string dificultad, string flag)
        {
            bool global = !string.IsNullOrEmpty(flag) && flag == "-g";
            List<StatsJuego> res = await leaderboard.GetLeaderboard(ctx, dificultad, tipoStats, global);
            string stats = string.Empty;
            int pos = 0;
            int lastScore = 0;
            DiscordEmoji emoji;
            foreach (var jugador in res)
            {
                if ((jugador.RondasTotales / jugador.PartidasTotales) >= 2)
                {
                    long x = jugador.UserId;
                    ulong id = (ulong)x;
                    try
                    {
                        DiscordMember miembro = await ctx.Guild.GetMemberAsync(id);
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
                    catch (Exception) { }
                }
            }
            return stats;
        }

        public async Task<DiscordEmbedBuilder> GetEstadisticas(CommandContext ctx, string juego, string flag)
        {
            string facil = await GetEstadisticasDificultad(ctx, juego, "Fácil", flag);
            string media = await GetEstadisticasDificultad(ctx, juego, "Media", flag);
            string dificil = await GetEstadisticasDificultad(ctx, juego, "Dificil", flag);
            string extremo = await GetEstadisticasDificultad(ctx, juego, "Extremo", flag);

            string titulo;
            if (juego.Contains("ahorcado"))
                titulo = "Ahorcado";
            else
                titulo = $"Adivina el {juego}";
            var builder = CrearEmbedStats(ctx, $"Estadisticas - {titulo}", facil, media, dificil, extremo, flag);
            return builder;
        }

        public async Task<DiscordEmbedBuilder> GetEstadisticasTag(CommandContext ctx, string flag)
        {
            string msgError = string.Empty;
            var interactivity = ctx.Client.GetInteractivity();
            List<string> tagsList = await leaderboard.GetTags(ctx);
            if (tagsList.Count > 0)
            {
                string tags = string.Empty;
                int cont = 1;
                foreach (string s in tagsList)
                {
                    tags += $"{cont} - {s}\n";
                    cont++;
                }
                var msgOpciones = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
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
                            await funciones.BorrarMensaje(ctx, msgOpciones.Id);
                            await funciones.BorrarMensaje(ctx, msgElegirTagInter.Result.Id);
                            List<Anime> animeList = new List<Anime>();
                            string elegido = tagsList[numTagElegir - 1];
                            string stats = await GetEstadisticasDificultad(ctx, "tag", elegido, flag);
                            return new DiscordEmbedBuilder
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
                await funciones.BorrarMensaje(ctx, msgOpciones.Id);
                await funciones.BorrarMensaje(ctx, msgElegirTagInter.Result.Id);
                return new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Footer = funciones.GetFooter(ctx),
                    Color = DiscordColor.Red,
                    Description = msgError
                };
            }
            else
            {
                return new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Footer = funciones.GetFooter(ctx),
                    Color = DiscordColor.Red,
                    Description = "No se encontró ninguna partida de adivina el tag, juega partidas para consultar las estadísticas."
                };
            }
        }

        public DiscordEmbedBuilder CrearEmbedStats(CommandContext ctx, string titulo, string facil, string media, string dificil, string extremo, string flag)
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
            /* Comentado hasta unificar ranking de un usr en varios servidores
            if (string.IsNullOrEmpty(flag) || flag != "-g")
                builder.AddField("Tip", "Si agregas ` -g` al final del comando, veras las puntuaciones globales.");
            */
            return builder;
        }

        public async Task ElimianrEstadisticas(CommandContext ctx)
        {
            await leaderboard.EliminarEstadisticas(ctx, "personaje");
            await leaderboard.EliminarEstadisticas(ctx, "anime");
            await leaderboard.EliminarEstadisticas(ctx, "manga");
            await leaderboard.EliminarEstadisticasTag(ctx);
            await leaderboard.EliminarEstadisticas(ctx, "estudio");
            await leaderboard.EliminarEstadisticas(ctx, "protagonista");
        }

        public async Task<List<Anime>> GetMedia(CommandContext ctx, string tipo, SettingsJuego settings, bool personajes, bool estudios, bool tag)
        {
            List<Anime> animeList = new List<Anime>();
            DiscordMessage mensaje = await ctx.Channel.SendMessageAsync($"Obteniendo animes...").ConfigureAwait(false);
            string mediaFiltros;
            if (tag)
                mediaFiltros = $"type: ANIME, sort: POPULARITY_DESC, tag: \"{settings.Tag}\", minimumTagRank:{settings.PorcentajeTag}, isAdult:false";
            else
                mediaFiltros = $"type: {tipo}, sort: FAVOURITES_DESC, isAdult:false";
            string query = "query($pagina : Int){" +
                    "   Page(page: $pagina){" +
                   $"       media({mediaFiltros}){{" +
                    "           siteUrl," +
                    "           favourites," +
                    "           title{" +
                    "               romaji," +
                    "               english" +
                    "           }," +
                    "           synonyms," +
                    "           coverImage{" +
                    "               large" +
                    "           }," +
                    "           characters(role: MAIN){" +
                    "               nodes{" +
                    "                   name{" +
                    "                       first," +
                    "                       last," +
                    "                       full" +
                    "                   }," +
                    "                   siteUrl," +
                    "                   favourites," +
                    "               }" +
                    "           }," +
                    "           studios{" +
                    "               nodes{" +
                    "                   name," +
                    "                   siteUrl," +
                    "                   favourites," +
                    "                   isAnimationStudio" +
                    "               }" +
                    "           }" +
                    "       }," +
                    "       pageInfo{" +
                    "           hasNextPage" +
                    "       }" +
                    "   }" +
                    "}";
            int popularidad;
            if (settings.IterIni == 1)
                popularidad = 1;
            else
                popularidad = settings.IterIni * 50;
            int i = settings.IterIni;
            string hasNextValue;
            do
            {
                var request = new GraphQLRequest
                {
                    Query = query,
                    Variables = new
                    {
                        pagina = i
                    }
                };
                try
                {
                    var data = await graphQLClient.SendQueryAsync<dynamic>(request);
                    hasNextValue = data.Data.Page.pageInfo.hasNextPage;
                    foreach (var x in data.Data.Page.media)
                    {
                        string titleEnglish = x.title.english;
                        string titleRomaji = x.title.romaji;
                        Anime anim = new Anime()
                        {
                            Image = x.coverImage.large,
                            TitleEnglish = funciones.QuitarCaracteresEspeciales(titleEnglish),
                            TitleRomaji = funciones.QuitarCaracteresEspeciales(titleRomaji),
                            SiteUrl = x.siteUrl,
                            Favoritos = x.favourites,
                            Popularidad = popularidad,
                            Estudios = new List<Estudio>()
                        };
                        if (personajes)
                        {
                            foreach (var character in x.characters.nodes)
                            {
                                anim.Personajes.Add(new Character()
                                {
                                    NameFull = character.name.full,
                                    NameFirst = character.name.first,
                                    NameLast = character.name.last,
                                    SiteUrl = character.siteUrl,
                                    Favoritos = character.favourites
                                });
                            }
                        }
                        if (estudios)
                        {
                            foreach (var estudio in x.studios.nodes)
                            {
                                if (estudio.isAnimationStudio == "true")
                                {
                                    anim.Estudios.Add(new Estudio()
                                    {
                                        Nombre = estudio.name,
                                        SiteUrl = estudio.siteUrl,
                                        Favoritos = estudio.favourites
                                    });
                                }
                            }
                        }
                        popularidad++;
                        if (anim.Estudios.Count() > 0)
                        {
                            animeList.Add(anim);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DiscordMessage msg = ex.Message switch
                    {
                        _ => await ctx.Channel.SendMessageAsync($"Error inesperado: {ex.Message}").ConfigureAwait(false),
                    };
                    await Task.Delay(3000);
                    await funciones.BorrarMensaje(ctx, msg.Id);
                    return animeList;
                }
                i++;
            } while (hasNextValue.ToLower() == "true" && (tag || (!tag && i <= settings.IterFin)));
            await funciones.BorrarMensaje(ctx, mensaje.Id);
            return animeList;
        }

        public async Task<List<Character>> GetCharacters(CommandContext ctx, SettingsJuego settings, bool animes)
        {
            var characterList = new List<Character>();
            DiscordMessage mensaje = await ctx.Channel.SendMessageAsync($"Obteniendo animes...").ConfigureAwait(false);
            string query = "query($pagina : Int){" +
                        "   Page(page: $pagina){" +
                        "       characters(sort: FAVOURITES_DESC){" +
                        "           siteUrl," +
                        "           favourites," +
                        "           name{" +
                        "               first," +
                        "               last," +
                        "               full" +
                        "           }," +
                        "           image{" +
                        "               large" +
                        "           }," +
                        "           media(type:ANIME){" +
                        "               nodes{" +
                        "                   title{" +
                        "                       romaji," +
                        "                       english" +
                        "                   }," +
                        "                   siteUrl," +
                        "                   synonyms" +
                        "               }" +
                        "           }" +
                        "       }" +
                        "   }" +
                        "}";
            int popularidad;
            if (settings.IterIni == 1)
                popularidad = 1;
            else
                popularidad = settings.IterIni * 50;
            int i = settings.IterIni;
            string hasNextValue;
            do
            {
                var request = new GraphQLRequest
                {
                    Query = query,
                    Variables = new
                    {
                        pagina = i
                    }
                };
                try
                {
                    var data = await graphQLClient.SendQueryAsync<dynamic>(request);
                    hasNextValue = data.Data.Page.pageInfo.hasNextPage;
                    foreach (var x in data.Data.Page.characters)
                    {
                        Character c = new Character()
                        {
                            Image = x.image.large,
                            NameFull = x.name.full,
                            SiteUrl = x.siteUrl,
                            Favoritos = x.favourites,
                            Animes = new List<Anime>(),
                            Popularidad = popularidad
                        };
                        popularidad++;
                        if (animes)
                        {
                            foreach (var y in x.media.nodes)
                            {
                                string titleEnglish = y.title.english;
                                string titleRomaji = y.title.romaji;
                                Anime anim = new Anime()
                                {
                                    TitleEnglish = funciones.QuitarCaracteresEspeciales(titleEnglish),
                                    TitleRomaji = funciones.QuitarCaracteresEspeciales(titleRomaji),
                                    SiteUrl = y.siteUrl,
                                    Sinonimos = new List<string>()
                                };
                                foreach (var syn in y.synonyms)
                                {
                                    string value = syn.Value;
                                    string bien = funciones.QuitarCaracteresEspeciales(value);
                                    anim.Sinonimos.Add(bien);
                                }
                                c.Animes.Add(anim);
                            }
                        }
                        if (!animes || (animes && c.Animes.Count() > 0))
                        {
                            characterList.Add(c);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DiscordMessage msg = ex.Message switch
                    {
                        _ => await ctx.Channel.SendMessageAsync($"Error inesperado: {ex.Message}").ConfigureAwait(false),
                    };
                    await Task.Delay(3000);
                    await funciones.BorrarMensaje(ctx, msg.Id);
                    return characterList;
                }
                i++;
            } while (hasNextValue.ToLower() == "true" && i <= settings.IterFin);
            await funciones.BorrarMensaje(ctx, mensaje.Id);
            return characterList;
        }
    }
}
