using DSharpPlus;
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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YumikoBot.DAL;

namespace Discord_Bot
{
    public class FuncionesJuegos
    {
        private readonly FuncionesAuxiliares funciones = new();
        private readonly LeaderboardoGeneral leaderboard = new();
        private readonly GraphQLHttpClient graphQLClient = new("https://graphql.anilist.co", new NewtonsoftJsonSerializer());

        public async Task QuizCharactersGlobal(Context ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            SettingsJuego settings = await InicializarJuego(ctx, interactivity, true, false, false);
            if (settings.Ok)
            {
                DiscordEmbed embebido = new DiscordEmbedBuilder
                {
                    Title = "Adivina el personaje",
                    Description = $"{ctx.User.Mention}, puedes escribir `cancelar` en cualquiera de las rondas si deseas terminar la partida.",
                    Color = funciones.GetColor(),
                    Footer = funciones.GetFooter(ctx)
                }.AddField("Rondas", $"{settings.Rondas}").AddField("Dificultad", $"{settings.Dificultad}");
                await ctx.Channel.SendMessageAsync(embed: embebido).ConfigureAwait(false);
                var characterList = await GetCharacters(ctx, settings, false);
                await Jugar(ctx, "personaje", characterList, settings, interactivity);
            }
            else
            {
                var error = await ctx.Channel.SendMessageAsync(settings.MsgError).ConfigureAwait(false);
                await Task.Delay(5000);
                await funciones.BorrarMensaje(ctx, error.Id);
            }
        }

        public async Task QuizAnimeGlobal(Context ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            SettingsJuego settings = await InicializarJuego(ctx, interactivity, true, false, false);
            if (settings.Ok)
            {
                DiscordEmbed embebido = new DiscordEmbedBuilder
                {
                    Title = "Adivina el anime",
                    Description = $"{ctx.User.Mention}, puedes escribir `cancelar` en cualquiera de las rondas si deseas terminar la partida.",
                    Color = funciones.GetColor(),
                    Footer = funciones.GetFooter(ctx)
                }.AddField("Rondas", $"{settings.Rondas}").AddField("Dificultad", $"{settings.Dificultad}");
                await ctx.Channel.SendMessageAsync(embed: embebido).ConfigureAwait(false);
                var characterList = await GetCharacters(ctx, settings, true);
                await Jugar(ctx, "anime", characterList, settings, interactivity);
            }
            else
            {
                var error = await ctx.Channel.SendMessageAsync(settings.MsgError).ConfigureAwait(false);
                await Task.Delay(5000);
                await funciones.BorrarMensaje(ctx, error.Id);
            }
        }

        public async Task QuizMangaGlobal(Context ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            SettingsJuego settings = await InicializarJuego(ctx, interactivity, true, false, false);
            if (settings.Ok)
            {
                DiscordEmbed embebido = new DiscordEmbedBuilder
                {
                    Title = "Adivina el manga",
                    Description = $"{ctx.User.Mention}, puedes escribir `cancelar` en cualquiera de las rondas si deseas terminar la partida.",
                    Color = funciones.GetColor(),
                    Footer = funciones.GetFooter(ctx)
                }.AddField("Rondas", $"{settings.Rondas}").AddField("Dificultad", $"{settings.Dificultad}");
                await ctx.Channel.SendMessageAsync(embed: embebido).ConfigureAwait(false);
                var animeList = await GetMedia(ctx, "MANGA", settings, false, false, false, false);
                await Jugar(ctx, "manga", animeList, settings, interactivity);
            }
            else
            {
                var error = await ctx.Channel.SendMessageAsync(settings.MsgError).ConfigureAwait(false);
                await Task.Delay(5000);
                await funciones.BorrarMensaje(ctx, error.Id);
            }
        }

        public async Task QuizAnimeTagGlobal(Context ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            SettingsJuego settings = await InicializarJuego(ctx, interactivity, false, true, false);
            if (settings.Ok)
            {
                DiscordEmbed embebido = new DiscordEmbedBuilder
                {
                    Title = $"Adivina el tag",
                    Description = $"{ctx.User.Mention}, puedes escribir `cancelar` en cualquiera de las rondas si deseas terminar la partida.",
                    Color = funciones.GetColor(),
                    Footer = funciones.GetFooter(ctx)
                }.AddField("Rondas", $"{settings.Rondas}").AddField("Tag", $"{settings.Tag}");
                await ctx.Channel.SendMessageAsync(embed: embebido).ConfigureAwait(false);
                settings.PorcentajeTag = 70;
                var animeList = await GetMedia(ctx, "ANIME", settings, false, false, true, false);
                int cantidadAnimes = animeList.Count;
                if (cantidadAnimes > 0)
                {
                    if (cantidadAnimes < settings.Rondas)
                    {
                        settings.Rondas = cantidadAnimes;
                        await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                        {
                            Color = DiscordColor.Yellow,
                            Title = $"Rondas reducidas",
                            Description = $"Se han reducido el numero de rondas a {settings.Rondas} ya que esta es la cantidad de animes con al menos un {settings.PorcentajeTag}% de {settings.Tag}",
                        }).ConfigureAwait(false);
                    }
                    settings.Dificultad = settings.Tag;
                    await Jugar(ctx, "tag", animeList, settings, interactivity);
                }
                else
                {
                    settings.Ok = false;
                    settings.MsgError = "No hay ningun anime con este tag con al menos 70%";
                }
            }
            if (!settings.Ok)
            {
                var error = await ctx.Channel.SendMessageAsync(settings.MsgError).ConfigureAwait(false);
                await Task.Delay(3000);
                await funciones.BorrarMensaje(ctx, error.Id);
            }
        }

        public async Task QuizStudioGlobal(Context ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            SettingsJuego settings = await InicializarJuego(ctx, interactivity, true, false, false);
            if (settings.Ok)
            {
                DiscordEmbed embebido = new DiscordEmbedBuilder
                {
                    Title = "Adivina el estudio del anime",
                    Description = $"{ctx.User.Mention}, puedes escribir `cancelar` en cualquiera de las rondas si deseas terminar la partida.",
                    Color = funciones.GetColor(),
                    Footer = funciones.GetFooter(ctx)
                }.AddField("Rondas", $"{settings.Rondas}").AddField("Dificultad", $"{settings.Dificultad}");
                await ctx.Channel.SendMessageAsync(embed: embebido).ConfigureAwait(false);
                var animeList = await GetMedia(ctx, "ANIME", settings, false, true, false, false);
                await Jugar(ctx, "estudio", animeList, settings, interactivity);
            }
            else
            {
                var error = await ctx.Channel.SendMessageAsync(settings.MsgError).ConfigureAwait(false);
                await Task.Delay(5000);
                await funciones.BorrarMensaje(ctx, error.Id);
            }
        }

        public async Task QuizProtagonistGlobal(Context ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            SettingsJuego settings = await InicializarJuego(ctx, interactivity, true, false, false);
            if (settings.Ok)
            {
                DiscordEmbed embebido = new DiscordEmbedBuilder
                {
                    Title = "Adivina el protagonista del anime",
                    Description = $"{ctx.User.Mention}, puedes escribir `cancelar` en cualquiera de las rondas si deseas terminar la partida.",
                    Color = funciones.GetColor(),
                    Footer = funciones.GetFooter(ctx)
                }.AddField("Rondas", $"{settings.Rondas}").AddField("Dificultad", $"{settings.Dificultad}");
                await ctx.Channel.SendMessageAsync(embed: embebido).ConfigureAwait(false);
                var animeList = await GetMedia(ctx, "ANIME", settings, true, false, false, false);
                await Jugar(ctx, "protagonista", animeList, settings, interactivity);
            }
            else
            {
                var error = await ctx.Channel.SendMessageAsync(settings.MsgError).ConfigureAwait(false);
                await Task.Delay(5000);
                await funciones.BorrarMensaje(ctx, error.Id);
            }
        }

        public async Task QuizGenreGlobal(Context ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            SettingsJuego settings = await InicializarJuego(ctx, interactivity, false, false, true);
            if (settings.Ok)
            {
                DiscordEmbed embebido = new DiscordEmbedBuilder
                {
                    Title = $"Adivina el género",
                    Description = $"{ctx.User.Mention}, puedes escribir `cancelar` en cualquiera de las rondas si deseas terminar la partida.",
                    Color = funciones.GetColor(),
                    Footer = funciones.GetFooter(ctx)
                }.AddField("Rondas", $"{settings.Rondas}").AddField("Género", $"{settings.Dificultad}");
                await ctx.Channel.SendMessageAsync(embed: embebido).ConfigureAwait(false);
                var animeList = await GetMedia(ctx, "ANIME", settings, false, false, false, true);
                await Jugar(ctx, "genero", animeList, settings, interactivity);
            }
            else
            {
                var error = await ctx.Channel.SendMessageAsync(settings.MsgError).ConfigureAwait(false);
                await Task.Delay(5000);
                await funciones.BorrarMensaje(ctx, error.Id);
            }
        }
        public async Task GetResultados(Context ctx, List<UsuarioJuego> participantes, int rondas, string dificultad, string juego)
        {
            string resultados;
            if (juego == "tag" || juego == "genero")
                resultados = $"{funciones.UppercaseFirst(juego)}: **{dificultad}**\n\n";
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
                await leaderboard.AddRegistro((long)ctx.Guild.Id, long.Parse(uj.Usuario.Id.ToString()), dificultad, uj.Puntaje, rondas, juego);
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

        public async Task<SettingsJuego> InicializarJuego(Context ctx, InteractivityExtension interactivity, bool elegirDificultad, bool elegirTag, bool elegirGenero)
        {
            DiscordComponentEmoji emote = new(DiscordEmoji.FromName(ctx.Client, ":game_die:"));

            DiscordButtonComponent buttonRandom = new(ButtonStyle.Primary, "0", string.Empty, emoji: emote);
            DiscordButtonComponent button5 = new(ButtonStyle.Primary, "5", "5");
            DiscordButtonComponent button10 = new(ButtonStyle.Primary, "10", "10");
            DiscordButtonComponent button15 = new(ButtonStyle.Primary, "15", "15");
            DiscordButtonComponent button20 = new(ButtonStyle.Primary, "20", "20");
            DiscordButtonComponent button50 = new(ButtonStyle.Primary, "50", "50");
            DiscordButtonComponent button75 = new(ButtonStyle.Primary, "75", "75");
            DiscordButtonComponent button100 = new(ButtonStyle.Primary, "100", "100");

            DiscordMessageBuilder mensajeRondas = new()
            {
                Embed = new DiscordEmbedBuilder
                {
                    Title = "Elige la cantidad de rondas",
                    Description = $"{ctx.User.Mention}, haz click en un boton para continuar"
                }
            };

            mensajeRondas.AddComponents(buttonRandom, button5, button10, button15);
            mensajeRondas.AddComponents(button20, button50, button75, button100);

            DiscordMessage msgCntRondas = await mensajeRondas.SendAsync(ctx.Channel);
            var msgRondasInter = await interactivity.WaitForButtonAsync(msgCntRondas, ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGames"])));
            if (!msgRondasInter.TimedOut)
            {
                var resultRondas = msgRondasInter.Result;
                int rondas = int.Parse(resultRondas.Id);
                if(rondas == 0)
                {
                    rondas = funciones.GetNumeroRandom(3, 100);
                }
                if (msgCntRondas != null)
                    await funciones.BorrarMensaje(ctx, msgCntRondas.Id);
                if (elegirDificultad)
                {
                    DiscordButtonComponent buttonAleatorio = new(ButtonStyle.Primary, "0", string.Empty, emoji: emote);
                    DiscordButtonComponent buttonFacil = new(ButtonStyle.Primary, "1", "Fácil");
                    DiscordButtonComponent buttonMedia = new(ButtonStyle.Primary, "2", "Media");
                    DiscordButtonComponent buttonDificil = new(ButtonStyle.Primary, "3", "Dificil");
                    DiscordButtonComponent buttonExtremo = new(ButtonStyle.Primary, "4", "Extremo");

                    DiscordMessageBuilder mensaje = new()
                    {
                        Embed = new DiscordEmbedBuilder
                        {
                            Title = "Elije la dificultad",
                            Description = $"{ctx.User.Mention}, haz click en un boton para continuar"
                        }
                    };

                    mensaje.AddComponents(buttonAleatorio, buttonFacil, buttonMedia, buttonDificil, buttonExtremo);

                    DiscordMessage msg = await mensaje.SendAsync(ctx.Channel);
                    var interDificultad = await interactivity.WaitForButtonAsync(msg, ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGames"])));
                    if (!interDificultad.TimedOut)
                    {
                        var resultDifiicultad = interDificultad.Result;
                        string dificultad = resultDifiicultad.Id;
                        if (dificultad == "0")
                        {
                            var dificultadNum = funciones.GetNumeroRandom(1, 4);
                            dificultad = dificultadNum.ToString();
                        }
                        int iterIni;
                        int iterFin;
                        string dificultadStr;
                        switch (dificultad)
                        {
                            case "1":
                                iterIni = 1;
                                iterFin = 10;
                                dificultadStr = "Fácil";
                                break;
                            case "2":
                                iterIni = 10;
                                iterFin = 30;
                                dificultadStr = "Media";
                                break;
                            case "3":
                                iterIni = 30;
                                iterFin = 60;
                                dificultadStr = "Dificil";
                                break;
                            case "4":
                                iterIni = 60;
                                iterFin = 100;
                                dificultadStr = "Extremo";
                                break;
                            default:
                                iterIni = 10;
                                iterFin = 30;
                                dificultadStr = "Media";
                                break;
                        }
                        if (msg != null)
                            await funciones.BorrarMensaje(ctx, msg.Id);
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
                        if (msg != null)
                            await funciones.BorrarMensaje(ctx, msg.Id);
                        return new SettingsJuego()
                        {
                            Ok = false,
                            MsgError = "Tiempo agotado esperando la dificultad"
                        };
                    }
                }
                if (elegirTag)
                {
                    var tags = await funciones.GetTags(ctx);
                    if(tags.Count > 0)
                    {
                        if (funciones.ChequearPermisoYumiko(ctx, Permissions.ManageMessages))
                        {
                            int porPagina = 30;
                            int ultPagina = tags.Count / porPagina;
                            int iterInterna = 0;
                            int iter = 0;
                            string tagsStr = string.Empty;
                            List<Page> pages = new();
                            foreach (var tag in tags)
                            {
                                tagsStr += $"{tag.Nombre}\n";
                                iterInterna++;
                                iter++;

                                if (iterInterna == porPagina)
                                {
                                    pages.Add(new()
                                    {
                                        Embed = new DiscordEmbedBuilder
                                        {
                                            Title = "Escribe un tag",
                                            Description = tagsStr,
                                            Color = funciones.GetColor(),
                                            Footer = new DiscordEmbedBuilder.EmbedFooter
                                            {
                                                Text = $"Obtenido desde AniList | Página {pages.Count + 1}/{ultPagina + 1}",
                                                IconUrl = ConfigurationManager.AppSettings["AnilistAvatar"]
                                            }
                                        }
                                    });
                                    tagsStr = string.Empty;
                                    iterInterna = 0;
                                }
                            }
                            if (iterInterna > 0)
                            {
                                pages.Add(new()
                                {
                                    Embed = new DiscordEmbedBuilder
                                    {
                                        Title = "Escribe un tag",
                                        Description = tagsStr,
                                        Color = funciones.GetColor(),
                                        Footer = new DiscordEmbedBuilder.EmbedFooter
                                        {
                                            Text = $"Obtenido desde AniList | Página {pages.Count + 1}/{ultPagina + 1}",
                                            IconUrl = ConfigurationManager.AppSettings["AnilistAvatar"]
                                        }
                                    }
                                });
                            }

                            _ = interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages, token: new CancellationTokenSource(TimeSpan.FromSeconds(300)).Token).ConfigureAwait(false);
                        }
                        else
                        {
                            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                            {
                                Title = "Escriber un tag!",
                                Description = "Para ver los tags dispibles habilita a Yumiko el permiso de `Gestionar Mensajes` y vuelve a ejecutar el comando.",
                                Color = funciones.GetColor()
                            });
                        }

                        var msgTagInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(120));
                        if (!msgTagInter.TimedOut)
                        {
                            int numTag = 0;
                            string tagResp = string.Empty;
                            List<Tag> tagsFiltrados = tags.Where(x => x.Nombre.ToLower().Trim().Contains(msgTagInter.Result.Content.ToLower().Trim())).ToList();
                            if (tagsFiltrados.Count > 0)
                            {
                                if (tagsFiltrados.Count == 1)
                                {
                                    return new SettingsJuego()
                                    {
                                        Rondas = rondas,
                                        Tag = tagsFiltrados[0].Nombre,
                                        TagDesc = tagsFiltrados[0].Descripcion,
                                        Ok = true
                                    };
                                }

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
                    else
                    {
                        await funciones.GrabarLogError(ctx, $"Error inesperado en InicializarJuego eligiendo tag");
                        return new SettingsJuego()
                        {
                            Ok = false,
                            MsgError = "Error inesperado eligiendo el tag"
                        };
                    }
                }
                if (elegirGenero)
                {
                    var respuesta = await ElegirGenero(ctx, interactivity);
                    if (respuesta.Ok)
                    {
                        return new SettingsJuego()
                        {
                            Rondas = rondas,
                            Genero = respuesta.Genero,
                            Dificultad = respuesta.Genero,
                            Ok = true
                        };
                    }
                    else
                    {
                        return respuesta;
                    }
                }
                string mensajeErr = "Error de programación, se debe elegir el tag o las rondas";
                await funciones.GrabarLogError(ctx, $"{mensajeErr}");
                return new SettingsJuego()
                {
                    Ok = false,
                    MsgError = mensajeErr
                };
            }
            else
            {
                if (msgCntRondas != null)
                    await funciones.BorrarMensaje(ctx, msgCntRondas.Id);
                return new SettingsJuego()
                {
                    Ok = false,
                    MsgError = "Tiempo agotado esperando la cantidad de rondas"
                };
            }
        }

        public async Task Jugar(Context ctx, string juego, dynamic lista, SettingsJuego settings, InteractivityExtension interactivity)
        {
            List<UsuarioJuego> participantes = new();
            int lastRonda;
            for (int ronda = 1; ronda <= settings.Rondas; ronda++)
            {
                lastRonda = ronda;
                int random = funciones.GetNumeroRandom(0, lista.Count - 1);
                dynamic elegido = lista[random];
                DiscordEmoji corazon = DiscordEmoji.FromName(ctx.Client, ":heart:");
                string juegoMostrar;
                if (juego == "tag" || juego == "genero")
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
                            (elegidoP.NameFull != null && xm.Content.ToLower().Trim() == elegidoP.NameFull.ToLower().Trim()) ||
                            (elegidoP.NameFirst != null && xm.Content.ToLower().Trim() == elegidoP.NameFirst.ToLower().Trim()) ||
                            (elegidoP.NameLast != null && xm.Content.ToLower().Trim() == elegidoP.NameLast.ToLower().Trim())
                            ));
                        break;
                    case "anime":
                        desc = $"Los animes de [{elegido.NameFull}]({elegido.SiteUrl}) son:\n\n";
                        foreach (Anime anim in elegido.Animes)
                        {
                            desc += $"- [{anim.TitleRomajiFormatted}]({anim.SiteUrl})\n";
                        }
                        Character elegidoC = elegido;
                        predicate = new Func<DiscordMessage, bool>(xm => (xm.Channel == ctx.Channel) && (xm.Author.Id != ctx.Client.CurrentUser.Id) && (!xm.Author.IsBot) &&
                            ((xm.Content.ToLower() == "cancelar" && xm.Author == ctx.User) ||
                            (elegidoC.Animes.Find(x => x.TitleEnglish != null && x.TitleEnglish.ToLower().Trim() == xm.Content.ToLower().Trim()) != null) ||
                            (elegidoC.Animes.Find(x => x.TitleRomaji != null && x.TitleRomaji.ToLower().Trim() == xm.Content.ToLower().Trim()) != null) ||
                            (elegidoC.Animes.Find(x => x.TitleEnglishFormatted != null && x.TitleEnglishFormatted.ToLower().Trim() == xm.Content.ToLower().Trim()) != null) ||
                            (elegidoC.Animes.Find(x => x.TitleRomajiFormatted != null && x.TitleRomajiFormatted.ToLower().Trim() == xm.Content.ToLower().Trim()) != null) ||
                            (elegidoC.Animes.Find(x => x.Sinonimos.Find(y => y.ToLower().Trim() == xm.Content.ToLower().Trim()) != null) != null)
                            ));
                        break;
                    case "manga":
                        desc = $"El nombre era: [{elegido.TitleRomaji}]({elegido.SiteUrl})";
                        Anime elegidoM = elegido;
                        predicate = new Func<DiscordMessage, bool>(xm => (xm.Channel == ctx.Channel) && (xm.Author.Id != ctx.Client.CurrentUser.Id) && (!xm.Author.IsBot) &&
                            ((xm.Content.ToLower() == "cancelar" && xm.Author == ctx.User) ||
                            (elegido.TitleRomaji != null && (xm.Content.ToLower().Trim() == elegido.TitleRomaji.ToLower().Trim())) || (elegido.TitleEnglish != null && (xm.Content.ToLower().Trim() == elegido.TitleEnglish.ToLower().Trim())) ||
                            (elegido.TitleRomajiFormatted != null && (xm.Content.ToLower().Trim() == elegido.TitleRomajiFormatted.ToLower().Trim())) || (elegido.TitleEnglishFormatted != null && (xm.Content.ToLower().Trim() == elegido.TitleEnglishFormatted.ToLower().Trim())) ||
                            (elegidoM.Sinonimos.Find(y => y.ToLower().Trim() == xm.Content.ToLower().Trim()) != null)
                            ));
                        break;
                    case "tag":
                        desc = $"El nombre es: [{elegido.TitleRomaji}]({elegido.SiteUrl})";
                        Anime elegidoT = elegido;
                        predicate = new Func<DiscordMessage, bool>(xm => (xm.Channel == ctx.Channel) && (xm.Author.Id != ctx.Client.CurrentUser.Id) && (!xm.Author.IsBot) &&
                            ((xm.Content.ToLower() == "cancelar" && xm.Author == ctx.User) ||
                            (elegido.TitleRomaji != null && (xm.Content.ToLower().Trim() == elegido.TitleRomaji.ToLower().Trim())) || (elegido.TitleEnglish != null && (xm.Content.ToLower().Trim() == elegido.TitleEnglish.ToLower().Trim())) ||
                            (elegido.TitleRomajiFormatted != null && (xm.Content.ToLower().Trim() == elegido.TitleRomajiFormatted.ToLower().Trim())) || (elegido.TitleEnglishFormatted != null && (xm.Content.ToLower().Trim() == elegido.TitleEnglishFormatted.ToLower().Trim())) ||
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
                    case "genero":
                        desc = $"El nombre es: [{elegido.TitleRomajiFormatted}]({elegido.SiteUrl})";
                        Anime elegidoG = elegido;
                        predicate = new Func<DiscordMessage, bool>(xm => (xm.Channel == ctx.Channel) && (xm.Author.Id != ctx.Client.CurrentUser.Id) && (!xm.Author.IsBot) &&
                            ((xm.Content.ToLower() == "cancelar" && xm.Author == ctx.User) ||
                            (elegido.TitleRomaji != null && (xm.Content.ToLower().Trim() == elegido.TitleRomaji.ToLower().Trim())) || (elegido.TitleEnglish != null && (xm.Content.ToLower().Trim() == elegido.TitleEnglish.ToLower().Trim())) ||
                            (elegido.TitleRomajiFormatted != null && (xm.Content.ToLower().Trim() == elegido.TitleRomajiFormatted.ToLower().Trim())) || (elegido.TitleEnglishFormatted != null && (xm.Content.ToLower().Trim() == elegido.TitleEnglishFormatted.ToLower().Trim())) ||
                            (elegidoG.Sinonimos.Find(y => y.ToLower().Trim() == xm.Content.ToLower().Trim()) != null)
                            ));
                        break;
                    default:
                        await funciones.GrabarLogError(ctx, $"No existe case del switch de FuncionesJuegos - Jugar, utilizado: {juego}");
                        return;
                }
                desc = funciones.NormalizarDescription(desc);
                var msg = await interactivity.WaitForMessageAsync(predicate, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["GuessTimeGames"])));
                if (!msg.TimedOut)
                {
                    if (msg.Result.Author == ctx.User && msg.Result.Content.ToLower() == "cancelar")
                    {
                        await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = $"¡Juego cancelado por {ctx.Member.Username}#{ctx.Member.Discriminator}!",
                            Description = desc,
                            Color = DiscordColor.Red
                        }).ConfigureAwait(false);
                        await GetResultados(ctx, participantes, lastRonda, settings.Dificultad, juego);
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

        public async Task<string> GetEstadisticasDificultad(Context ctx, string tipoStats, string dificultad)
        {
            List<StatsJuego> res = await leaderboard.GetLeaderboard(ctx, dificultad, tipoStats);
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

        public async Task<DiscordEmbedBuilder> GetEstadisticas(Context ctx, string juego)
        {
            string facil = await GetEstadisticasDificultad(ctx, juego, "Fácil");
            string media = await GetEstadisticasDificultad(ctx, juego, "Media");
            string dificil = await GetEstadisticasDificultad(ctx, juego, "Dificil");
            string extremo = await GetEstadisticasDificultad(ctx, juego, "Extremo");

            string titulo;
            if (juego.Contains("ahorcado"))
                titulo = "Ahorcado";
            else
                titulo = $"Adivina el {juego}";
            var builder = CrearEmbedStats(ctx, $"Estadisticas - {titulo}", facil, media, dificil, extremo);
            return builder;
        }

        public async Task<DiscordEmbedBuilder> GetEstadisticasTag(Context ctx)
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
                var embed = new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    Title = "Elije el tag escribiendo su número"
                };
                var pages = interactivity.GeneratePagesInEmbed(tags, DSharpPlus.Interactivity.Enums.SplitType.Line, embed);
                _ = interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages, token: new CancellationTokenSource(TimeSpan.FromSeconds(300)).Token).ConfigureAwait(false);
                var msgElegirTagInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGames"])));
                if (!msgElegirTagInter.TimedOut)
                {
                    bool result = int.TryParse(msgElegirTagInter.Result.Content, out int numTagElegir);
                    if (result)
                    {
                        if (numTagElegir > 0 && (numTagElegir <= tagsList.Count))
                        {
                            //await funciones.BorrarMensaje(ctx, msgOpciones.Id);
                            await funciones.BorrarMensaje(ctx, msgElegirTagInter.Result.Id);
                            List<Anime> animeList = new();
                            string elegido = tagsList[numTagElegir - 1];
                            string stats = await GetEstadisticasDificultad(ctx, "tag", elegido);
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
                //await funciones.BorrarMensaje(ctx, msgOpciones.Id);
                
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

        public async Task<DiscordEmbedBuilder> GetEstadisticasGenero(Context ctx, string genero)
        {
            string stats = await GetEstadisticasDificultad(ctx, "genero", genero);
            return new DiscordEmbedBuilder
            {
                Title = $"Estadisticas - Adivina el {genero}",
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor(),
                Description = stats
            };
        }

        public DiscordEmbedBuilder CrearEmbedStats(Context ctx, string titulo, string facil, string media, string dificil, string extremo)
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

        public async Task EliminarEstadisticas(CommandContext ctx)
        {
            await leaderboard.EliminarEstadisticas(ctx, "personaje");
            await leaderboard.EliminarEstadisticas(ctx, "anime");
            await leaderboard.EliminarEstadisticas(ctx, "manga");
            await leaderboard.EliminarEstadisticasTag(ctx);
            await leaderboard.EliminarEstadisticas(ctx, "estudio");
            await leaderboard.EliminarEstadisticas(ctx, "protagonista");
        }

        public async Task<List<Anime>> GetMedia(Context ctx, string tipo, SettingsJuego settings, bool personajes, bool estudios, bool tag, bool genero)
        {
            List<Anime> animeList = new();
            DiscordMessage mensaje = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = $"Obteniendo {tipo.ToLower()}s",
                Description = "Por favor espera mientras se prepara todo",
                Color = funciones.GetColor(),
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = ConfigurationManager.AppSettings["AnilistAvatar"],
                    Text = "Obtenido desde AniList"
                }
            }).ConfigureAwait(false);
            string mediaFiltros;
            if (tag)
            {
                if (ctx.Channel.IsNSFW)
                {
                    mediaFiltros = $"type: ANIME, sort: POPULARITY_DESC, tag: \"{settings.Tag}\", minimumTagRank:{settings.PorcentajeTag}, format_not_in:[MUSIC]";
                }
                else
                {
                    mediaFiltros = $"type: ANIME, sort: POPULARITY_DESC, tag: \"{settings.Tag}\", minimumTagRank:{settings.PorcentajeTag}, isAdult:false, format_not_in:[MUSIC]";
                }
            }
            else if (genero)
            {
                if (ctx.Channel.IsNSFW)
                {
                    mediaFiltros = $"type: ANIME, sort: POPULARITY_DESC, genre: \"{settings.Genero}\", format_not_in:[MUSIC]";
                }
                else
                {
                    mediaFiltros = $"type: ANIME, sort: POPULARITY_DESC, genre: \"{settings.Genero}\", isAdult:false, format_not_in:[MUSIC]";
                }
            }
            else
            {
                if (ctx.Channel.IsNSFW)
                {
                    mediaFiltros = $"type: {tipo}, sort: FAVOURITES_DESC, format_not_in:[MUSIC]";
                }
                else
                {
                    mediaFiltros = $"type: {tipo}, sort: FAVOURITES_DESC, isAdult:false, format_not_in:[MUSIC]";
                }
            }
                
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
            int iter = 0;
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
                    iter++;
                    hasNextValue = data.Data.Page.pageInfo.hasNextPage;
                    foreach (var x in data.Data.Page.media)
                    {
                        string titleEnglish = x.title.english;
                        string titleRomaji = x.title.romaji;
                        Anime anim = new()
                        {
                            Image = x.coverImage.large,
                            TitleEnglish = titleEnglish,
                            TitleRomaji = titleRomaji,
                            TitleEnglishFormatted = funciones.QuitarCaracteresEspeciales(titleEnglish),
                            TitleRomajiFormatted = funciones.QuitarCaracteresEspeciales(titleRomaji),
                            SiteUrl = x.siteUrl,
                            Favoritos = x.favourites,
                            Popularidad = popularidad,
                            Estudios = new List<Estudio>(),
                            Sinonimos = new List<string>(),
                            Personajes = new List<Character>()
                        };
                        foreach (var syn in x.synonyms)
                        {
                            string value = syn.Value;
                            string bien = funciones.QuitarCaracteresEspeciales(value);
                            anim.Sinonimos.Add(bien);
                        }
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
                        if ((!estudios && !personajes) || (estudios && anim.Estudios.Count > 0) || (personajes && anim.Personajes.Count > 0))
                        {
                            animeList.Add(anim);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await funciones.GrabarLogError(ctx, $"{ex.Message}");
                    DiscordMessage msg = ex.Message switch
                    {
                        _ => await ctx.Channel.SendMessageAsync($"Error inesperado en GetMedia: {ex.Message}").ConfigureAwait(false),
                    };
                    await Task.Delay(3000);
                    await funciones.BorrarMensaje(ctx, msg.Id);
                    return animeList;
                }
                i++;
            } while (hasNextValue.ToLower() == "true" && (((tag || genero) && iter < 10) || (!tag && !genero && i <= settings.IterFin)));
            await funciones.BorrarMensaje(ctx, mensaje.Id);
            return animeList;
        }

        public async Task<List<Character>> GetCharacters(Context ctx, SettingsJuego settings, bool animes)
        {
            var characterList = new List<Character>();
            DiscordMessage mensaje = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder{ 
                Title = "Obteniendo personajes",
                Description = "Por favor espera mientras se prepara todo",
                Color = funciones.GetColor(),
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = ConfigurationManager.AppSettings["AnilistAvatar"],
                    Text = "Obtenido desde AniList"
                }
            }).ConfigureAwait(false);
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
                    foreach (var x in data.Data.Page.characters)
                    {
                        Character c = new()
                        {
                            Image = x.image.large,
                            NameFirst = x.name.first,
                            NameLast = x.name.last,
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
                                Anime anim = new()
                                {
                                    TitleEnglish = titleEnglish,
                                    TitleRomaji = titleRomaji,
                                    TitleEnglishFormatted = funciones.QuitarCaracteresEspeciales(titleEnglish),
                                    TitleRomajiFormatted = funciones.QuitarCaracteresEspeciales(titleRomaji),
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
                        if (!animes || (animes && c.Animes.Count > 0))
                        {
                            characterList.Add(c);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await funciones.GrabarLogError(ctx, $"Error inesperado en GetCharacters: {ex.Message}");
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

        public async Task JugarAhorcado(CommandContext ctx, dynamic elegido, string juego)
        {
            var interactivity = ctx.Client.GetInteractivity();
            int errores = 0;
            string nameFull = string.Empty;
            switch (juego)
            {
                case "personaje":
                    nameFull = elegido.NameFull.ToLower().Trim();
                    break;
                case "anime":
                    nameFull = elegido.TitleRomaji.ToLower().Trim();
                    break;
                default:
                    await funciones.GrabarLogError(ctx, $"No existe case del switch de FuncionesJuegos - Jugar, utilizado: {juego}");
                    return;
            }
            string nameFullParsed = Regex.Replace(nameFull, @"\s+", " ");
            char[] charsPj = nameFullParsed.ToCharArray();
            var charsEsp = charsPj.Distinct();
            var chars = charsEsp.ToList();
            chars.Remove((char)32);
            List<CaracterBool> caracteres = new();
            List<string> letrasUsadas = new();
            foreach (char c in chars)
            {
                if (Char.IsLetter(c))
                {
                    caracteres.Add(new CaracterBool()
                    {
                        Caracter = c,
                        Acertado = false
                    });
                }
                else
                {
                    caracteres.Add(new CaracterBool()
                    {
                        Caracter = c,
                        Acertado = true
                    });
                }
            }
            bool partidaTerminada = false;
            List<UsuarioJuego> participantes = new();
            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder()
            {
                Title = $"Ahorcado ({juego}s)",
                Description = "¡Escribe una letra!\n\nPuedes terminar la partida en cualquier momento escribiendo `cancelar`",
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor()
            }).ConfigureAwait(false);
            DiscordMember ganador = ctx.Member;
            string titRonda;
            DiscordColor colRonda;
            int rondas = 0;
            int rondasPerdidas = 0;
            dynamic predicate;
            string desc1 = string.Empty;
            switch (juego)
            {
                case "personaje":
                    desc1 = $"El personaje es [{elegido.NameFull}]({elegido.SiteUrl}) de [{elegido.AnimePrincipal.TitleRomaji}]({elegido.AnimePrincipal.SiteUrl})";
                    predicate = new Func<DiscordMessage, bool>(
                        xm => (xm.Channel == ctx.Channel) && (xm.Author.Id != ctx.Client.CurrentUser.Id) && (!xm.Author.IsBot) &&
                        ((xm.Content.ToLower().Trim() == "cancelar") ||
                        ((xm.Content.ToLower().Trim().Length == 1) && (xm.Content.ToLower().Trim().All(Char.IsLetter)) && (letrasUsadas.Find(x => x == xm.Content.ToLower().Trim()) == null)) ||
                        (xm.Content.ToLower().Trim() == elegido.NameFull.ToLower().Trim())
                        ));
                    break;
                case "anime":
                    desc1 = $"El anime es [{elegido.TitleRomaji}]({elegido.SiteUrl})";
                    predicate = new Func<DiscordMessage, bool>(
                        xm => (xm.Channel == ctx.Channel) && (xm.Author.Id != ctx.Client.CurrentUser.Id) && (!xm.Author.IsBot) &&
                        ((xm.Content.ToLower().Trim() == "cancelar") ||
                        ((xm.Content.ToLower().Trim().Length == 1) && (xm.Content.ToLower().Trim().All(Char.IsLetter)) && (letrasUsadas.Find(x => x == xm.Content.ToLower().Trim()) == null)) ||
                        (elegido.TitleRomaji != null && elegido.TitleRomaji.ToLower().Trim() == xm.Content.ToLower().Trim())
                        ));
                    break;
                default:
                    await funciones.GrabarLogError(ctx, $"No existe case del switch de FuncionesJuegos - Jugar, utilizado: {juego}");
                    return;
            }
            do
            {
                var msg = await interactivity.WaitForMessageAsync(predicate, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["GuessTimeGames"])));
                if (!msg.TimedOut)
                {
                    DiscordMember acertador = await ctx.Guild.GetMemberAsync(msg.Result.Author.Id);
                    if (msg.Result.Content.ToLower().Trim().Length == 1)
                    {
                        var acierto = caracteres.Find(x => x.Caracter.ToString() == msg.Result.Content.ToLower().Trim());
                        if (acierto != null)
                        {
                            acierto.Acertado = true;
                            titRonda = $"¡{acertador.DisplayName} ha acertado!";
                            colRonda = DiscordColor.Green;
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
                        }
                        else
                        {
                            errores++;
                            rondasPerdidas++;
                            titRonda = $"¡{acertador.DisplayName} le ha errado!";
                            colRonda = DiscordColor.Red;
                            UsuarioJuego usr = participantes.Find(x => x.Usuario == msg.Result.Author);
                            if (usr == null)
                            {
                                participantes.Add(new UsuarioJuego()
                                {
                                    Usuario = msg.Result.Author,
                                    Puntaje = 0
                                });
                            }
                        }

                        var letraUsada = letrasUsadas.Find(x => x.ToLower().Trim() == msg.Result.Content.ToLower().Trim());
                        if (letraUsada == null)
                        {
                            letrasUsadas.Add(msg.Result.Content.ToLower().Trim());
                        }
                        string letras = "`";
                        foreach (var c in charsPj)
                        {
                            var registro = caracteres.Find(x => x.Caracter == c);
                            if (registro != null) // Por los espacios del string original
                            {
                                if (registro.Acertado)
                                    letras += c + " ";
                                else
                                    letras += "_ ";
                            }
                            else
                            {
                                letras += "` `";
                            }
                        }
                        letras += "`\n\n";
                        string desc = string.Empty;
                        desc += letras;
                        switch (errores)
                        {
                            case 0:
                                desc += ". ┌─────┐\n" +
                                ".┃...............┋\n" +
                                ".┃...............┋\n" +
                                ".┃\n" +
                                ".┃\n" +
                                ".┃\n" +
                                "/-\\" +
                                "\n";
                                break;
                            case 1:
                                desc += ". ┌─────┐\n" +
                                ".┃...............┋\n" +
                                ".┃...............┋\n" +
                                ".┃.............:dizzy_face: \n" +
                                ".┃\n" +
                                ".┃\n" +
                                "/-\\" +
                                "\n";
                                break;
                            case 2:
                                desc += ". ┌─────┐\n" +
                                ".┃...............┋\n" +
                                ".┃...............┋\n" +
                                ".┃.............:dizzy_face: \n" +
                                ".┃............./\n" +
                                ".┃\n" +
                                "/-\\" +
                                "\n";
                                break;
                            case 3:
                                desc += ". ┌─────┐\n" +
                                ".┃...............┋\n" +
                                ".┃...............┋\n" +
                                ".┃.............:dizzy_face: \n" +
                                ".┃............./ |\n" +
                                ".┃\n" +
                                "/-\\" +
                                "\n";
                                break;
                            case 4:
                                desc += ". ┌─────┐\n" +
                                ".┃...............┋\n" +
                                ".┃...............┋\n" +
                                ".┃.............:dizzy_face: \n" +
                                ".┃............./ | \\   \n" +
                                ".┃\n" +
                                "/-\\" +
                                "\n";
                                break;
                            case 5:
                                desc += ". ┌─────┐\n" +
                                ".┃...............┋\n" +
                                ".┃...............┋\n" +
                                ".┃.............:dizzy_face: \n" +
                                ".┃............./ | \\   \n" +
                                ".┃............../\n" +
                                "/-\\" +
                                "\n";
                                break;
                            case 6:
                                desc += ". ┌─────┐\n" +
                                ".┃...............┋\n" +
                                ".┃...............┋\n" +
                                ".┃.............:dizzy_face: \n" +
                                ".┃............./ | \\   \n" +
                                ".┃............../\\ \n" +
                                "/-\\" +
                                "\n";
                                break;
                        }
                        desc += "\n**Letras usadas:**\n";
                        foreach (var cc in letrasUsadas)
                        {
                            desc += $"`{cc}` ";
                        }
                        await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder()
                        {
                            Title = titRonda,
                            Description = desc,
                            Color = colRonda
                        }).ConfigureAwait(false);
                    }
                    else
                    {
                        if (msg.Result.Content.ToLower().Trim() == "cancelar")
                        {
                            errores = 6;
                            partidaTerminada = true;
                        }
                        else
                        {
                            int aciertos = 0;
                            foreach (var ccc in caracteres)
                            {
                                if (!ccc.Acertado)
                                {
                                    ccc.Acertado = true;
                                    aciertos++;
                                }
                            }
                            UsuarioJuego usr = participantes.Find(x => x.Usuario == msg.Result.Author);
                            if (usr != null)
                            {
                                usr.Puntaje += aciertos;
                            }
                            else
                            {
                                participantes.Add(new UsuarioJuego()
                                {
                                    Usuario = msg.Result.Author,
                                    Puntaje = aciertos
                                });
                            }
                            partidaTerminada = true;
                        }
                    }
                    if ((caracteres.Find(x => x.Acertado == false) == null) || ((juego == "personaje" && msg.Result.Content.ToLower().Trim() == elegido.NameFull.ToLower().Trim()) || (juego == "anime" && msg.Result.Content.ToLower().Trim() == elegido.TitleRomaji.ToLower().Trim())))
                    {
                        ganador = acertador;
                        partidaTerminada = true;
                    }
                }
                else
                {
                    errores++;
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder()
                    {
                        Title = "¡Nadie ha escrito!",
                        Description = "Escribe una letra cualquiera para seguir con el juego.",
                        Footer = funciones.GetFooter(ctx),
                        Color = DiscordColor.Red
                    }).ConfigureAwait(false);
                }
                rondas++;
                if (errores >= 6 || (caracteres.Find(x => x.Acertado == false) == null))
                    partidaTerminada = true;
            } while (!partidaTerminada);

            if (errores == 6)
            {
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder()
                {
                    Title = "¡Derrota!",
                    Description = $"{desc1}",
                    ImageUrl = elegido.Image,
                    Footer = funciones.GetFooter(ctx),
                    Color = DiscordColor.Red
                }).ConfigureAwait(false);
            }
            else
            {
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder()
                {
                    Title = "¡Victoria!",
                    Description = $"{desc1}\n\n" +
                    $"Ganador: {ganador.Mention}",
                    ImageUrl = elegido.Image,
                    Footer = funciones.GetFooter(ctx),
                    Color = DiscordColor.Green
                }).ConfigureAwait(false);
            }
        }

        public async Task<SettingsJuego> ElegirGenero(Context ctx, InteractivityExtension interactivity)
        {
            string query =
                    "query{" +
                    "   GenreCollection" +
                    "}";
            var request = new GraphQLRequest
            {
                Query = query
            };
            List<string> generos = new();
            try
            {
                var data = await graphQLClient.SendQueryAsync<dynamic>(request);
                foreach (var x in data.Data.GenreCollection)
                {
                    string nombre = x;
                    if (ctx.Channel.IsNSFW || (!ctx.Channel.IsNSFW && nombre.ToLower().Trim() != "hentai"))
                    {
                        generos.Add(nombre);
                    }
                }
            }
            catch (Exception ex)
            {
                await funciones.GrabarLogError(ctx, $"{ex.Message}");
                return new SettingsJuego()
                {
                    Ok = false,
                    MsgError = "Error inesperado eligiendo el genero"
                };
            }

            DiscordMessageBuilder mensajeBuild = new()
            {
                Embed = new DiscordEmbedBuilder
                {
                    Title = "Elije el genero",
                    Description = $"{ctx.User.Mention}, haz click en un boton para continuar"
                }
            };
            int iterInterna = 0;
            int iterReal = 0;
            List<DiscordComponent> componentes = new();
            foreach (var nomGenero in generos)
            {
                iterInterna++;
                iterReal++;

                DiscordButtonComponent button = new(ButtonStyle.Primary, $"{iterReal}", $"{nomGenero}");
                componentes.Add(button);

                if (iterInterna == 5)
                {
                    mensajeBuild.AddComponents(componentes);
                    iterInterna = 0;
                    componentes.Clear();
                }
            }
            if (componentes.Count > 0)
            {
                mensajeBuild.AddComponents(componentes);
            }

            DiscordMessage msgGenero = await mensajeBuild.SendAsync(ctx.Channel);
            var interGenero = await interactivity.WaitForButtonAsync(msgGenero, ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGeneral"])));
            if (!interGenero.TimedOut)
            {
                var resultado = interGenero.Result;
                int id = int.Parse(resultado.Id);
                string generoElegido = generos[id - 1];
                if (msgGenero != null)
                    await funciones.BorrarMensaje(ctx, msgGenero.Id);
                return new SettingsJuego()
                {
                    Genero = generoElegido,
                    Ok = true
                };
            }
            else
            {
                if (msgGenero != null)
                    await funciones.BorrarMensaje(ctx, msgGenero.Id);
                return new SettingsJuego()
                {
                    Ok = false,
                    MsgError = "Tiempo agotado esperando el genero"
                };
            }
        }
    }
}
