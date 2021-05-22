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
using System.Text.RegularExpressions;

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
            SettingsJuego settings = await funcionesJuegos.InicializarJuego(ctx, interactivity, true, false);
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
            SettingsJuego settings = await funcionesJuegos.InicializarJuego(ctx, interactivity, true, false);
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
            SettingsJuego settings = await funcionesJuegos.InicializarJuego(ctx, interactivity, true, false);
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
            SettingsJuego settings = await funcionesJuegos.InicializarJuego(ctx, interactivity,false,true);
            if (settings.Ok)
            {
                settings.PorcentajeTag = 70;
                var animeList = await funcionesJuegos.GetMedia(ctx, "ANIME", settings, false, false, true);
                int cantidadAnimes = animeList.Count();
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
                    SettingsJuego settings2 = new SettingsJuego()
                    {
                        Rondas = settings.Rondas,
                        Dificultad = settings.Tag,
                        Ok = true
                    };
                    await funcionesJuegos.Jugar(ctx, "tag", settings.Rondas, animeList, settings2, interactivity);
                }
                else
                {
                    settings.Ok = false;
                    settings.MsgError = "No hay ningun anime con este tag con al menos 70%";
                }
            }
            if(!settings.Ok)
            {
                var error = await ctx.Channel.SendMessageAsync(settings.MsgError).ConfigureAwait(false);
                await Task.Delay(3000);
                await funciones.BorrarMensaje(ctx, error.Id);    
            }
        }

        [Command("quizS"), Aliases("adivinaelestudio"), Description("Empieza el juego de adivina el estudio."), RequireGuild]
        public async Task QuizStudioGlobal(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            SettingsJuego settings = await funcionesJuegos.InicializarJuego(ctx, interactivity, true, false);
            if (settings.Ok)
            {
                DiscordEmbed embebido = new DiscordEmbedBuilder
                {
                    Title = "Adivina el estudio del anime",
                    Description = $"Sesión iniciada por {ctx.User.Mention}\n\nPuedes escribir `cancelar` en cualquiera de las rondas para terminar la partida.",
                    Color = funciones.GetColor()
                }.AddField("Rondas", $"{settings.Rondas}").AddField("Dificultad", $"{settings.Dificultad}");
                await ctx.Channel.SendMessageAsync(embed: embebido).ConfigureAwait(false);
                var animeList = await funcionesJuegos.GetMedia(ctx, "ANIME", settings, false, true, false);
                await funcionesJuegos.Jugar(ctx, "estudio", settings.Rondas, animeList, settings, interactivity);
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
            SettingsJuego settings = await funcionesJuegos.InicializarJuego(ctx, interactivity, true, false);
            if (settings.Ok)
            {
                DiscordEmbed embebido = new DiscordEmbedBuilder
                {
                    Title = "Adivina el protagonista del anime",
                    Description = $"Sesión iniciada por {ctx.User.Mention}\n\nPuedes escribir `cancelar` en cualquiera de las rondas para terminar la partida.",
                    Color = funciones.GetColor()
                }.AddField("Rondas", $"{settings.Rondas}").AddField("Dificultad", $"{settings.Dificultad}");
                await ctx.Channel.SendMessageAsync(embed: embebido).ConfigureAwait(false);
                var animeList = await funcionesJuegos.GetMedia(ctx, "ANIME", settings, true, false, false);
                await funcionesJuegos.Jugar(ctx, "protagonista", settings.Rondas, animeList, settings, interactivity);
            }
            else
            {
                var error = await ctx.Channel.SendMessageAsync(settings.MsgError).ConfigureAwait(false);
                await Task.Delay(5000);
                await funciones.BorrarMensaje(ctx, error.Id);
            }
        }

        [Command("ahorcado"), Aliases("ahorcadopj", "hangman"), Description("Empieza el juego del ahorcado con un personaje aleatorio."), RequireGuild]
        public async Task AhorcadoPersonaje(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            int pag = funciones.GetNumeroRandom(1, 5000);
            Character personaje = await funciones.GetRandomCharacter(ctx, pag);
            if (personaje != null)
            {
                int errores = 0;
                string nameFull = personaje.NameFull.ToLower().Trim();
                string nameFullParsed = Regex.Replace(nameFull, @"\s+", " ");
                char[] charsPj = nameFullParsed.ToCharArray();
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
                List<UsuarioJuego> participantes = new List<UsuarioJuego>();
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder()
                {
                    Title = "Ahorcado (Peronajes de anime)",
                    Description = "¡Escribe una letra!\n\nPuedes terminar la partida en cualquier momento escribiendo `cancelar`",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor()
                }).ConfigureAwait(false);
                DiscordMember ganador = ctx.Member;
                string titRonda;
                DiscordColor colRonda;
                int rondas = 0;
                int rondasPerdidas = 0;
                do
                {
                    dynamic predicate = new Func<DiscordMessage, bool>(
                        xm => (xm.Channel == ctx.Channel) && (xm.Author.Id != ctx.Client.CurrentUser.Id) && (!xm.Author.IsBot) && 
                        ((xm.Content.ToLower().Trim() == "cancelar") ||
                        ((xm.Content.ToLower().Trim().Length == 1) && (letrasUsadas.Find(x => x == xm.Content.ToLower().Trim()) == null)) ||
                        (xm.Content.ToLower().Trim() == personaje.NameFull.ToLower().Trim())
                        ));
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
                        if ((caracteres.Find(x => x.Acertado == false) == null) || msg.Result.Content.ToLower().Trim() == personaje.NameFull.ToLower().Trim())
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
                        Description = $"El personaje es [{personaje.NameFull}]({personaje.SiteUrl}) de [{personaje.AnimePrincipal.TitleRomaji}]({personaje.AnimePrincipal.SiteUrl})\n\n" +
                        $"Ganador: {ganador.Mention}",
                        ImageUrl = personaje.Image,
                        Footer = funciones.GetFooter(ctx),
                        Color = DiscordColor.Green
                    }).ConfigureAwait(false);
                }
                //await funcionesJuegos.GetResultados(ctx, participantes, rondas, dificultadStr, "ahorcadoc");
            }
        }
    }
}
