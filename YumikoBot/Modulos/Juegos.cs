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

namespace Discord_Bot.Modulos
{
    public class Juegos : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();
        private readonly FuncionesJuegos funcionesJuegos = new FuncionesJuegos();
        private readonly GraphQLHttpClient graphQLClient = new GraphQLHttpClient("https://graphql.anilist.co", new NewtonsoftJsonSerializer());

        [Command("quiz"), Description("Empieza el juego de adivinar algo relacionado con el anime."), RequireGuild]
        public async Task Quiz(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            string opcion;
            string juegos =
                $"**1-** Adivina el personaje\n" +
                $"**2-** Adivina el anime\n" +
                $"**3-** Adivina el manga\n" +
                $"**4-** Adivina el tag\n" +
                $"**5-** Adivina el estudio\n" +
                $"**6-** Adivina el protagonista\n";
            var msgElegir = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Elige el juego",
                Description = juegos,
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor(),
            });
            var msgElegirInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGeneral"])));
            if (!msgElegirInter.TimedOut)
            {
                opcion = msgElegirInter.Result.Content;
                if (msgElegir != null)
                    await funciones.BorrarMensaje(ctx, msgElegir.Id);
                if (msgElegirInter.Result != null)
                    await funciones.BorrarMensaje(ctx, msgElegirInter.Result.Id);
                opcion = opcion.ToLower();
                switch (opcion)
                {
                    case "1":
                    case "1- adivina el personaje":
                    case "adivina el personaje":
                        await QuizCharactersGlobal(ctx);
                        break;
                    case "2":
                    case "2- adivina el anime":
                    case "adivina el anime":
                        await QuizAnimeGlobal(ctx);
                        break;
                    case "3":
                    case "3- adivina el manga":
                    case "adivina el manga":
                        await QuizMangaGlobal(ctx);
                        break;
                    case "4":
                    case "4- adivina el tag":
                    case "adivina el tag":
                        await QuizAnimeTagGlobal(ctx);
                        break;
                    case "5":
                    case "5- adivina el estudio":
                    case "adivina el estudio":
                        await QuizStudioGlobal(ctx);
                        break;
                    case "6":
                    case "6- adivina el protagonista":
                    case "adivina el protagonista":
                        await QuizProtagonistGlobal(ctx);
                        break;
                    default:
                        var msgError = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = "Error",
                            Description = "Opcion de juego incorrecta",
                            Footer = funciones.GetFooter(ctx),
                            Color = DiscordColor.Red,
                        });
                        await Task.Delay(3000);
                        if (msgError != null)
                            await funciones.BorrarMensaje(ctx, msgError.Id);
                        break;
                }
            }
            else
            {
                var msgError = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Description = "Tiempo agotado esperando el juego",
                    Footer = funciones.GetFooter(ctx),
                    Color = DiscordColor.Red,
                });
                await Task.Delay(3000);
                if (msgError != null)
                    await funciones.BorrarMensaje(ctx, msgError.Id);
                if (msgElegir != null)
                    await funciones.BorrarMensaje(ctx, msgElegir.Id);
            }
        }

        [Command("quizC"), Aliases("adivinaelpersonaje", "characterquiz"), Description("Empieza el juego de adivina el personaje."), RequireGuild]
        public async Task QuizCharactersGlobal(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            SettingsJuego settings = await funcionesJuegos.InicializarJuego(ctx, interactivity);
            if (settings.Ok)
            {
                int rondas = settings.Rondas;
                string dificultadStr = settings.Dificultad;
                int iterIni = settings.IterIni;
                int iterFin = settings.IterFin;
                DiscordEmbed embebido = new DiscordEmbedBuilder
                {
                    Title = "Adivina el personaje",
                    Description = $"Sesión iniciada por {ctx.User.Mention}\n\nPuedes escribir `cancelar` en cualquiera de las rondas para terminar la partida.",
                    Color = funciones.GetColor()
                }.AddField("Rondas", $"{rondas}").AddField("Dificultad", $"{dificultadStr}");
                await ctx.Channel.SendMessageAsync(embed: embebido).ConfigureAwait(false);
                List<Character> characterList = new List<Character>();
                DiscordMessage mensaje = await ctx.Channel.SendMessageAsync($"Obteniendo personajes...").ConfigureAwait(false);
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
                        "           }" +
                        "       }" +
                        "   }" +
                        "}";
                int popularidad;
                if (iterIni == 1)
                    popularidad = 1;
                else
                    popularidad = iterIni * 50;
                for (int i = iterIni; i <= iterFin; i++)
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
                        foreach (var x in data.Data.Page.characters)
                        {
                            characterList.Add(new Character()
                            {
                                Image = x.image.large,
                                NameFull = x.name.full,
                                NameFirst = x.name.first,
                                NameLast = x.name.last,
                                SiteUrl = x.siteUrl,
                                Favoritos = x.favourites,
                                Popularidad = popularidad,
                            });
                            popularidad++;
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
                        return;
                    }
                }
                await funciones.BorrarMensaje(ctx, mensaje.Id);
                await funcionesJuegos.Jugar(ctx, "personaje", rondas, characterList, settings, interactivity);
            }
            else
            {
                var error = await ctx.Channel.SendMessageAsync(settings.MsgError).ConfigureAwait(false);
                await Task.Delay(5000);
                await funciones.BorrarMensaje(ctx, error.Id);
            }
        }

        [Command("quizA"), Aliases("adivinaelanime", "animequiz"), Description("Empieza el juego de adivina el anime."), RequireGuild]
        public async Task QuizAnimeGlobal(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            SettingsJuego settings = await funcionesJuegos.InicializarJuego(ctx, interactivity);
            if (settings.Ok)
            {
                int rondas = settings.Rondas;
                string dificultadStr = settings.Dificultad;
                int iterIni = settings.IterIni;
                int iterFin = settings.IterFin;
                DiscordEmbed embebido = new DiscordEmbedBuilder
                {
                    Title = "Adivina el anime",
                    Description = $"Sesión iniciada por {ctx.User.Mention}\n\nPuedes escribir `cancelar` en cualquiera de las rondas para terminar la partida.",
                    Color = funciones.GetColor()
                }.AddField("Rondas", $"{rondas}").AddField("Dificultad", $"{dificultadStr}");
                await ctx.Channel.SendMessageAsync(embed: embebido).ConfigureAwait(false);
                DiscordMessage mensaje = await ctx.Channel.SendMessageAsync($"Obteniendo personajes...").ConfigureAwait(false);
                var characterList = new List<Character>();
                string query = "query($pagina : Int){" +
                        "   Page(page: $pagina){" +
                        "       characters(sort: FAVOURITES_DESC){" +
                        "           siteUrl," +
                        "           name{" +
                        "               full" +
                        "           }," +
                        "           image{" +
                        "               large" +
                        "           }," +
                        "           favourites," +
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
                if(iterIni == 1)
                    popularidad = 1;
                else
                    popularidad = iterIni * 50;
                for (int i = iterIni; i <= iterFin; i++)
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
                            if (c.Animes.Count() > 0)
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
                        return;
                    }
                }
                await funciones.BorrarMensaje(ctx, mensaje.Id);
                await funcionesJuegos.Jugar(ctx, "anime", rondas, characterList, settings, interactivity);
            }
            else
            {
                var error = await ctx.Channel.SendMessageAsync(settings.MsgError).ConfigureAwait(false);
                await Task.Delay(5000);
                await funciones.BorrarMensaje(ctx, error.Id);
            }
        }

        [Command("quizM"), Aliases("adivinaelmanga"), Description("Empieza el juego de adivina el manga."), RequireGuild]
        public async Task QuizMangaGlobal(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            SettingsJuego settings = await funcionesJuegos.InicializarJuego(ctx, interactivity);
            if (settings.Ok)
            {
                int rondas = settings.Rondas;
                string dificultadStr = settings.Dificultad;
                int iterIni = settings.IterIni;
                int iterFin = settings.IterFin;
                DiscordEmbed embebido = new DiscordEmbedBuilder
                {
                    Title = "Adivina el manga",
                    Description = $"Sesión iniciada por {ctx.User.Mention}\n\nPuedes escribir `cancelar` en cualquiera de las rondas para terminar la partida.",
                    Color = funciones.GetColor()
                }.AddField("Rondas", $"{rondas}").AddField("Dificultad", $"{dificultadStr}");
                await ctx.Channel.SendMessageAsync(embed: embebido).ConfigureAwait(false);
                List<Anime> animeList = new List<Anime>();
                DiscordMessage mensaje = await ctx.Channel.SendMessageAsync($"Obteniendo mangas...").ConfigureAwait(false);
                string query = "query($pagina : Int){" +
                        "   Page(page: $pagina){" +
                        "       media(type: MANGA, sort: FAVOURITES_DESC, isAdult:false){" +
                        "           siteUrl," +
                        "           favourites," +
                        "           title{" +
                        "               romaji," +
                        "               english" +
                        "           }," +
                        "           coverImage{" +
                        "               large" +
                        "           }," +
                        "           synonyms" +
                        "       }" +
                        "   }" +
                        "}";
                int popularidad;
                if (iterIni == 1)
                    popularidad = 1;
                else
                    popularidad = iterIni * 50;
                for (int i = iterIni; i <= iterFin; i++)
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
                                Sinonimos = new List<string>(),
                                Popularidad = popularidad
                            };
                            popularidad++;
                            foreach (var syn in x.synonyms)
                            {
                                string value = syn.Value;
                                string bien = funciones.QuitarCaracteresEspeciales(value);
                                anim.Sinonimos.Add(bien);
                            }
                            animeList.Add(anim);
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
                        return;
                    }
                }
                await funciones.BorrarMensaje(ctx, mensaje.Id);
                await funcionesJuegos.Jugar(ctx, "manga", rondas, animeList, settings, interactivity);
            }
            else
            {
                var error = await ctx.Channel.SendMessageAsync(settings.MsgError).ConfigureAwait(false);
                await Task.Delay(5000);
                await funciones.BorrarMensaje(ctx, error.Id);
            }
        }

        [Command("quizT"), Aliases("adivinaeltag"), Description("Empieza el juego de adivina el anime de cierto tag."), RequireGuild]
        public async Task QuizAnimeTagGlobal(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            bool ok = true;
            string msgError = "";
            var msgCntRondas = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Elige la cantidad de rondas (máximo 100)",
                Description = "Por ejemplo: 10"
            });
            var msgRondasInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGames"])));
            if (!msgRondasInter.TimedOut)
            {
                bool resultR = int.TryParse(msgRondasInter.Result.Content, out int rondas);
                if (resultR)
                {
                    if (rondas > 0 && rondas <= 100)
                    {
                        await funciones.BorrarMensaje(ctx, msgCntRondas.Id);
                        await funciones.BorrarMensaje(ctx, msgRondasInter.Result.Id);
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
                                    tags.Add(new Tag() { 
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
                                string tagResp = "";
                                List<Tag> tagsFiltrados = tags.Where(x => x.Nombre.ToLower().Trim().Contains(msgTagInter.Result.Content.ToLower().Trim())).ToList();
                                if(tagsFiltrados.Count > 0)
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
                                        bool result = int.TryParse(msgElegirTagInter.Result.Content, out int numTagElegir);
                                        if (result)
                                        {
                                            if (numTagElegir > 0 && (numTagElegir <= tagsFiltrados.Count))
                                            {
                                                await funciones.BorrarMensaje(ctx, preguntaTag.Id);
                                                await funciones.BorrarMensaje(ctx, msgTagInter.Result.Id);
                                                await funciones.BorrarMensaje(ctx, msgOpciones.Id);
                                                await funciones.BorrarMensaje(ctx, msgElegirTagInter.Result.Id);
                                                List<Anime> animeList = new List<Anime>();
                                                Random rnd = new Random();
                                                List<UsuarioJuego> participantes = new List<UsuarioJuego>();
                                                string elegido = tagsFiltrados[numTagElegir - 1].Nombre;
                                                DiscordMessage mensaje = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                                                {
                                                    Title = $"Obteniendo animes...",
                                                    Description = $"**Tag:** {elegido}\n**Descripción:** {tagsFiltrados[numTagElegir - 1].Descripcion}",
                                                    Footer = funciones.GetFooter(ctx),
                                                    Color = funciones.GetColor()
                                                }).ConfigureAwait(false);
                                                int porcentajeTag = 70;
                                                string query1 = "query($pagina : Int){" +
                                                        "   Page(page: $pagina){" +
                                                        "       media(type: ANIME, sort: POPULARITY_DESC, tag: \"" + elegido + "\", minimumTagRank:" + porcentajeTag + ", isAdult:false){" +
                                                        "           siteUrl," +
                                                        "           favourites," +
                                                        "           title{" +
                                                        "               romaji," +
                                                        "               english" +
                                                        "           }," +
                                                        "           coverImage{" +
                                                        "               large" +
                                                        "           }," +
                                                        "           synonyms," +
                                                        "           isAdult" +
                                                        "       }," +
                                                        "       pageInfo{" +
                                                        "           hasNextPage" +
                                                        "       }" +
                                                        "   }" +
                                                        "}";
                                                int cont = 1;
                                                int popularidad = 1;
                                                bool seguir = true;
                                                do
                                                {
                                                    var request1 = new GraphQLRequest
                                                    {
                                                        Query = query1,
                                                        Variables = new
                                                        {
                                                            pagina = cont
                                                        }
                                                    };
                                                    try
                                                    {
                                                        var data1 = await graphQLClient.SendQueryAsync<dynamic>(request1);
                                                        string hasNextValue = data1.Data.Page.pageInfo.hasNextPage;
                                                        if (hasNextValue.ToLower() == "false")
                                                        {
                                                            seguir = false;
                                                        }
                                                        foreach (var x in data1.Data.Page.media)
                                                        {
                                                            if (x.isAdult == "False")
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
                                                                    Sinonimos = new List<string>(),
                                                                    Popularidad = popularidad
                                                                };
                                                                popularidad++;
                                                                foreach (var syn in x.synonyms)
                                                                {
                                                                    string value = syn.Value;
                                                                    string bien = funciones.QuitarCaracteresEspeciales(value);
                                                                    anim.Sinonimos.Add(bien);
                                                                }
                                                                animeList.Add(anim);
                                                            }
                                                        }
                                                        cont++;
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        DiscordMessage msg = ex.Message switch
                                                        {
                                                            _ => await ctx.Channel.SendMessageAsync($"Error inesperado: {ex.Message}").ConfigureAwait(false),
                                                        };
                                                        await Task.Delay(3000);
                                                        await funciones.BorrarMensaje(ctx, msg.Id);
                                                        return;
                                                    }
                                                } while (seguir);
                                                await funciones.BorrarMensaje(ctx, mensaje.Id);
                                                int lastRonda;
                                                int cantidadAnimes = animeList.Count();
                                                if (cantidadAnimes > 0)
                                                {
                                                    if (cantidadAnimes < rondas)
                                                    {
                                                        rondas = cantidadAnimes;
                                                        await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                                                        {
                                                            Color = DiscordColor.Yellow,
                                                            Title = $"Rondas reducidas",
                                                            Description = $"Se han reducido el numero de rondas a {rondas} ya que esta es la cantidad de animes con al menos un {porcentajeTag}% de {elegido}",
                                                        }).ConfigureAwait(false);
                                                    }
                                                    SettingsJuego settings = new SettingsJuego()
                                                    {
                                                        Rondas = rondas,
                                                        Dificultad = elegido,
                                                        Ok = true
                                                    };
                                                    await funcionesJuegos.Jugar(ctx, "tag", rondas, animeList, settings, interactivity);
                                                }
                                                else
                                                {
                                                    ok = false;
                                                    msgError = "No hay ningun anime con este tag con al menos 70%";
                                                }
                                            }
                                            else
                                            {
                                                ok = false;
                                                msgError = "El numero indicado del tag debe ser válido";
                                            }
                                        }
                                        else
                                        {
                                            ok = false;
                                            msgError = "Debes indicar un numero para elegir el tag";
                                        }
                                    }
                                    else
                                    {
                                        ok = false;
                                        msgError = "Tiempo agotado esperando la elección del tag";
                                    }
                                }
                                else
                                {
                                    ok = false;
                                    msgError = "No se encontro ningun tag";
                                }
                            }
                            else
                            {
                                ok = false;
                                msgError = "Tiempo agotado esperando el tag";
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
                            return;
                        }
                    }
                    else
                    {
                        ok = false;
                        msgError = "La cantidad de rondas no puede ser mayor a 100";
                    }
                }
                else
                {
                    ok = false;
                    msgError = "La cantidad de rondas debe ser un numero";
                }
            }
            else
            {
                ok = false;
                msgError = "Tiempo agotado esperando la cantidad de rondas";
            }
            if (!ok)
            {
                var error = await ctx.Channel.SendMessageAsync(msgError);
                await Task.Delay(3000);
                await funciones.BorrarMensaje(ctx, error.Id);    
            }
        }

        [Command("quizS"), Aliases("adivinaelestudio"), Description("Empieza el juego de adivina el estudio."), RequireGuild]
        public async Task QuizStudioGlobal(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            SettingsJuego settings = await funcionesJuegos.InicializarJuego(ctx, interactivity);
            if (settings.Ok)
            {
                int rondas = settings.Rondas;
                string dificultadStr = settings.Dificultad;
                int iterIni = settings.IterIni;
                int iterFin = settings.IterFin;
                DiscordEmbed embebido = new DiscordEmbedBuilder
                {
                    Title = "Adivina el estudio del anime",
                    Description = $"Sesión iniciada por {ctx.User.Mention}\n\nPuedes escribir `cancelar` en cualquiera de las rondas para terminar la partida.",
                    Color = funciones.GetColor()
                }.AddField("Rondas", $"{rondas}").AddField("Dificultad", $"{dificultadStr}");
                await ctx.Channel.SendMessageAsync(embed: embebido).ConfigureAwait(false);
                List<Anime> animeList = new List<Anime>();
                Random rnd = new Random();
                List<UsuarioJuego> participantes = new List<UsuarioJuego>();
                DiscordMessage mensaje = await ctx.Channel.SendMessageAsync($"Obteniendo animes...").ConfigureAwait(false);
                string query = "query($pagina : Int){" +
                        "   Page(page: $pagina){" +
                        "       media(type: ANIME, sort: FAVOURITES_DESC, isAdult:false){" +
                        "           siteUrl," +
                        "           favourites," +
                        "           title{" +
                        "               romaji," +
                        "               english" +
                        "           }," +
                        "           coverImage{" +
                        "               large" +
                        "           }," +
                        "           studios{" +
                        "               nodes{" +
                        "                   name," +
                        "                   siteUrl," +
                        "                   favourites," +
                        "                   isAnimationStudio" +
                        "               }" +
                        "           }" +
                        "       }" +
                        "   }" +
                        "}";
                int popularidad;
                if (iterIni == 1)
                    popularidad = 1;
                else
                    popularidad = iterIni * 50;
                for (int i = iterIni; i <= iterFin; i++)
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
                            foreach (var estudio in x.studios.nodes)
                            {
                                if(estudio.isAnimationStudio == "true")
                                {
                                    anim.Estudios.Add(new Estudio()
                                    {
                                        Nombre = estudio.name,
                                        SiteUrl = estudio.siteUrl,
                                        Favoritos = estudio.favourites
                                    });
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
                        return;
                    }
                }
                await funciones.BorrarMensaje(ctx, mensaje.Id);
                await funcionesJuegos.Jugar(ctx, "estudio", rondas, animeList, settings, interactivity);
            }
            else
            {
                var error = await ctx.Channel.SendMessageAsync(settings.MsgError).ConfigureAwait(false);
                await Task.Delay(5000);
                await funciones.BorrarMensaje(ctx, error.Id);
            }
        }

        [Command("quizP"), Aliases("adivinaelprotagonista"), Description("Empieza el juego de adivina el protagonista."), RequireGuild]
        public async Task QuizProtagonistGlobal(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            SettingsJuego settings = await funcionesJuegos.InicializarJuego(ctx, interactivity);
            if (settings.Ok)
            {
                int rondas = settings.Rondas;
                string dificultadStr = settings.Dificultad;
                int iterIni = settings.IterIni;
                int iterFin = settings.IterFin;
                DiscordEmbed embebido = new DiscordEmbedBuilder
                {
                    Title = "Adivina el protagonista del anime",
                    Description = $"Sesión iniciada por {ctx.User.Mention}\n\nPuedes escribir `cancelar` en cualquiera de las rondas para terminar la partida.",
                    Color = funciones.GetColor()
                }.AddField("Rondas", $"{rondas}").AddField("Dificultad", $"{dificultadStr}");
                await ctx.Channel.SendMessageAsync(embed: embebido).ConfigureAwait(false);
                List<Anime> animeList = new List<Anime>();
                Random rnd = new Random();
                List<UsuarioJuego> participantes = new List<UsuarioJuego>();
                DiscordMessage mensaje = await ctx.Channel.SendMessageAsync($"Obteniendo animes...").ConfigureAwait(false);
                string query = "query($pagina : Int){" +
                        "   Page(page: $pagina){" +
                        "       media(type: ANIME, sort: FAVOURITES_DESC, isAdult:false){" +
                        "           siteUrl," +
                        "           favourites," +
                        "           title{" +
                        "               romaji," +
                        "               english" +
                        "           }," +
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
                        "           }" +
                        "       }" +
                        "   }" +
                        "}";
                int popularidad;
                if (iterIni == 1)
                    popularidad = 1;
                else
                    popularidad = iterIni * 50;
                for (int i = iterIni; i <= iterFin; i++)
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
                                Personajes = new List<Character>()
                            };
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
                            popularidad++;
                            if (anim.Personajes.Count() > 0)
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
                        return;
                    }
                }
                await funciones.BorrarMensaje(ctx, mensaje.Id);
                await funcionesJuegos.Jugar(ctx, "protagonista", rondas, animeList, settings, interactivity);
            }
            else
            {
                var error = await ctx.Channel.SendMessageAsync(settings.MsgError).ConfigureAwait(false);
                await Task.Delay(5000);
                await funciones.BorrarMensaje(ctx, error.Id);
            }
        }

        [Command("ahorcado"), Aliases("ahorcadopj"), Description("Empieza el juego del ahorcado con un personaje aleatorio."), RequireGuild]
        public async Task AhorcadoPersonaje(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            string msgError = "";
            DiscordEmoji emojiDado = DiscordEmoji.FromName(ctx.Client, ":game_die:");
            var msgDificultad = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Elije la dificultad",
                Description = $"0- Aleatorio {emojiDado}\n1- Fácil\n2- Media\n3- Dificil\n4- Extremo"
            });
            var msgDificultadInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGames"])));
            if (!msgDificultadInter.TimedOut)
            {
                bool result = int.TryParse(msgDificultadInter.Result.Content, out int dificultad);
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
                                iterFin = 500;
                                dificultadStr = "Fácil";
                                break;
                            case 2:
                                iterIni = 501;
                                iterFin = 1500;
                                dificultadStr = "Media";
                                break;
                            case 3:
                                iterIni = 1500;
                                iterFin = 3000;
                                dificultadStr = "Dificil";
                                break;
                            case 4:
                                iterIni = 3000;
                                iterFin = 5000;
                                dificultadStr = "Extremo";
                                break;
                            default:
                                iterIni = 501;
                                iterFin = 1500;
                                dificultadStr = "Medio";
                                break;
                        }
                        if (msgDificultad != null)
                            await funciones.BorrarMensaje(ctx, msgDificultad.Id);
                        if (msgDificultadInter.Result != null)
                            await funciones.BorrarMensaje(ctx, msgDificultadInter.Result.Id);
                        int pag = funciones.GetNumeroRandom(iterIni, iterFin);
                        Character personaje = await funciones.GetRandomCharacter(ctx, pag);
                        if (personaje != null)
                        {
                            int errores = 0;
                            char[] charsPj = personaje.NameFull.ToLower().ToCharArray();
                            var charsEsp = charsPj.Distinct();
                            var chars = charsEsp.ToList();
                            chars.Remove((char)32);
                            List<CaracterBool> caracteres = new List<CaracterBool>();
                            List<string> letrasUsadas = new List<string>();
                            foreach (char c in chars)
                            {
                                caracteres.Add(new CaracterBool()
                                {
                                    Caracter = c,
                                    Acertado = false
                                });
                            }
                            bool partidaTerminada = false;
                            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder()
                            {
                                Title = "Ahorcado (Peronajes de anime)",
                                Description = $"Dificultad: **{dificultadStr}**\n\n¡Escribe una letra!",
                                Footer = funciones.GetFooter(ctx),
                                Color = funciones.GetColor()
                            }).ConfigureAwait(false);

                            do
                            {
                                dynamic predicate = new Func<DiscordMessage, bool>(
                                    xm => (xm.Channel == ctx.Channel) && (xm.Author.Id != ctx.Client.CurrentUser.Id) &&
                                    ((xm.Content.ToLower().Trim().Length == 1) && 
                                    (letrasUsadas.Find(x => x == xm.Content.ToLower().Trim()) == null)) ||
                                    (xm.Content.ToLower().Trim() == personaje.NameFull.ToLower().Trim())
                                    );
                                var msg = await interactivity.WaitForMessageAsync(predicate, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["GuessTimeGames"])));
                                if (!msg.TimedOut)
                                {
                                    if(msg.Result.Content.ToLower().Trim().Length == 1)
                                    {
                                        DiscordMember acertador = await ctx.Guild.GetMemberAsync(msg.Result.Author.Id);
                                        var acierto = caracteres.Find(x => x.Caracter.ToString() == msg.Result.Content.ToLower().Trim());
                                        if (acierto != null)
                                        {
                                            acierto.Acertado = true;
                                            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder()
                                            {
                                                Title = $"¡{acertador.DisplayName} ha acertado!",
                                                Color = DiscordColor.Green
                                            }).ConfigureAwait(false);
                                        }
                                        else
                                        {
                                            errores++;
                                            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder()
                                            {
                                                Title = $"¡{acertador.DisplayName} le ha errado!",
                                                Color = DiscordColor.Red
                                            }).ConfigureAwait(false);
                                        }
                                        var letraUsada = letrasUsadas.Find(x => x.ToLower().Trim() == msg.Result.Content.ToLower().Trim());
                                        if (letraUsada == null)
                                        {
                                            letrasUsadas.Add(msg.Result.Content.ToLower().Trim());
                                        }
                                    }
                                    else
                                    {
                                        foreach (var ccc in caracteres)
                                        {
                                            ccc.Acertado = true;
                                        }
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
                                string desc = "";
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
                                foreach(var cc in letrasUsadas)
                                {
                                    desc += $"`{cc}` ";
                                }
                                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder()
                                {
                                    Title = "Ahorcado (Peronajes de anime)",
                                    Description = desc,
                                    Color = funciones.GetColor()
                                }).ConfigureAwait(false);

                                if (errores >= 6 || (caracteres.Find(x => x.Acertado == false) == null))
                                    partidaTerminada = true;
                            } while (!partidaTerminada);

                            if (errores == 6)
                            {
                                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder()
                                {
                                    Title = "¡Derrota!",
                                    Description = $"El personaje era [{personaje.NameFull}]({personaje.SiteUrl}) de [{personaje.AnimePrincipal.TitleRomaji}]({personaje.AnimePrincipal.SiteUrl})",
                                    ImageUrl = personaje.Image,
                                    Footer = funciones.GetFooter(ctx),
                                    Color = DiscordColor.Red
                                }).ConfigureAwait(false);
                            }
                            else
                            {
                                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder()
                                {
                                    Title = "¡Victoria!",
                                    Description = $"El personaje es [{personaje.NameFull}]({personaje.SiteUrl}) de [{personaje.AnimePrincipal.TitleRomaji}]({personaje.AnimePrincipal.SiteUrl})",
                                    ImageUrl = personaje.Image,
                                    Footer = funciones.GetFooter(ctx),
                                    Color = DiscordColor.Green
                                }).ConfigureAwait(false);
                            }
                        }
                    }
                    else
                    {
                        msgError = "La dificultad debe ser 0, 1, 2, 3 o 4";
                    }
                }
                else
                {
                    msgError = "La dificultad debe ser un número (0, 1, 2, 3 o 4)";
                }
            }
            else
            {
                msgError = "Tiempo agotado esperando la dificultad";
            }
            if (!String.IsNullOrEmpty(msgError))
            {
                var mensaje = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder()
                {
                    Title = "¡Derrota!",
                    Description = msgError,
                    Footer = funciones.GetFooter(ctx),
                    Color = DiscordColor.Red
                }).ConfigureAwait(false);
                await Task.Delay(5000);
                await funciones.BorrarMensaje(ctx, mensaje.Id);
            }
        }
    }
}
