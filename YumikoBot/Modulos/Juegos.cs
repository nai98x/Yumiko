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

        [Command("quizC"), Aliases("adivinaelpersonaje", "characterquiz"), Description("Empieza el juego de adivina el personaje."), RequireGuild/*, RequireBotPermissions(DSharpPlus.Permissions.ManageMessages)*/]
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
                Random rnd = new Random();
                List<UsuarioJuego> participantes = new List<UsuarioJuego>();
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
                int lastRonda;
                for (int ronda = 1; ronda <= rondas; ronda++)
                {
                    lastRonda = ronda;
                    int random = funciones.GetNumeroRandom(0, characterList.Count - 1);
                    Character elegido = characterList[random];
                    DiscordEmoji corazon = DiscordEmoji.FromName(ctx.Client, ":heart:");
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Gold,
                        Title = "Adivina el personaje",
                        Description = $"Ronda {ronda} de {rondas}",
                        ImageUrl = elegido.Image,
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"{elegido.Favoritos} {corazon} (nº {elegido.Popularidad} en popularidad)"
                        }
                    }).ConfigureAwait(false);
                    var msg = await interactivity.WaitForMessageAsync
                        (xm => (xm.Channel == ctx.Channel) &&
                        ((xm.Content.ToLower().Trim() == elegido.NameFull.ToLower().Trim() || xm.Content.ToLower().Trim() == elegido.NameFirst.ToLower().Trim() || (elegido.NameLast != null && xm.Content.ToLower().Trim() == elegido.NameLast.ToLower().Trim())) && xm.Author.Id != ctx.Client.CurrentUser.Id) || (xm.Content.ToLower() == "cancelar" && xm.Author == ctx.User)
                        , TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["GuessTimeGames"])));
                    if (!msg.TimedOut)
                    {
                        if (msg.Result.Author == ctx.User && msg.Result.Content.ToLower() == "cancelar")
                        {
                            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                            {
                                Title = "¡Juego cancelado!",
                                Description = $"El nombre era: [{elegido.NameFull}]({elegido.SiteUrl})",
                                Color = DiscordColor.Red
                            }).ConfigureAwait(false);
                            await funcionesJuegos.GetResultados(ctx, participantes, lastRonda, settings.Dificultad, "personaje");
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
                            Description = $"El nombre es: [{elegido.NameFull}]({elegido.SiteUrl})",
                            Color = DiscordColor.Green
                        }).ConfigureAwait(false);
                    }
                    else
                    {
                        await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = "¡Nadie ha acertado!",
                            Description = $"El nombre era: [{elegido.NameFull}]({elegido.SiteUrl})",
                            Color = DiscordColor.Red
                        }).ConfigureAwait(false);
                    }
                    characterList.Remove(characterList[random]);
                }
                await funcionesJuegos.GetResultados(ctx, participantes, rondas, settings.Dificultad, "personaje");
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
                Random rnd = new Random();
                List<UsuarioJuego> participantes = new List<UsuarioJuego>();
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
                int lastRonda;
                for (int ronda = 1; ronda <= rondas; ronda++)
                {
                    lastRonda = ronda;
                    int random = funciones.GetNumeroRandom(0, characterList.Count - 1);
                    Character elegido = characterList[random];
                    DiscordEmoji corazon = DiscordEmoji.FromName(ctx.Client, ":heart:");
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Gold,
                        Title = $"Adivina el anime del personaje",
                        Description = $"Ronda {ronda} de {rondas}",
                        ImageUrl = elegido.Image,
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"{elegido.Favoritos} {corazon}"
                        }
                    }).ConfigureAwait(false);
                    var msg = await interactivity.WaitForMessageAsync
                        (xm => (xm.Channel == ctx.Channel) &&
                        ((xm.Content.ToLower() == "cancelar" && xm.Author == ctx.User) || (
                        (elegido.Animes.Find(x => x.TitleEnglish != null && x.TitleEnglish.ToLower().Trim() == xm.Content.ToLower().Trim()) != null) ||
                        (elegido.Animes.Find(x => x.TitleRomaji != null && x.TitleRomaji.ToLower().Trim() == xm.Content.ToLower().Trim()) != null) ||
                        (elegido.Animes.Find(x => x.Sinonimos.Find(y => y.ToLower().Trim() == xm.Content.ToLower().Trim()) != null) != null)
                        ) && xm.Author.Id != ctx.Client.CurrentUser.Id),
                        TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["GuessTimeGames"])));
                    string descAnimes = $"Los animes de [{elegido.NameFull}]({elegido.SiteUrl}) son:\n\n";
                    foreach (Anime anim in elegido.Animes)
                    {
                        descAnimes += $"- [{anim.TitleRomaji}]({anim.SiteUrl})\n";
                    }
                    descAnimes = funciones.NormalizarDescription(descAnimes);
                    if (!msg.TimedOut)
                    {
                        if (msg.Result.Author == ctx.User && msg.Result.Content.ToLower() == "cancelar")
                        {
                            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                            {
                                Title = "¡Juego cancelado!",
                                Description = descAnimes,
                                Color = DiscordColor.Red
                            }).ConfigureAwait(false);
                            await funcionesJuegos.GetResultados(ctx, participantes, lastRonda, settings.Dificultad, "anime");
                            await ctx.Channel.SendMessageAsync($"El juego ha sido cancelado por **{ctx.User.Username}#{ctx.User.Discriminator}**").ConfigureAwait(false);
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
                            Description = descAnimes,
                            Color = DiscordColor.Green
                        }).ConfigureAwait(false);
                    }
                    else
                    {
                        await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = "¡Nadie ha acertado!",
                            Description = descAnimes,
                            Color = DiscordColor.Red
                        }).ConfigureAwait(false);
                    }
                    characterList.Remove(characterList[random]);
                }
                await funcionesJuegos.GetResultados(ctx, participantes, rondas, settings.Dificultad, "anime");
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
                Random rnd = new Random();
                List<UsuarioJuego> participantes = new List<UsuarioJuego>();
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
                int lastRonda;
                for (int ronda = 1; ronda <= rondas; ronda++)
                {
                    lastRonda = ronda;
                    int random = funciones.GetNumeroRandom(0, animeList.Count - 1);
                    Anime elegido = animeList[random];
                    DiscordEmoji corazon = DiscordEmoji.FromName(ctx.Client, ":heart:");
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Gold,
                        Title = "Adivina el manga",
                        Description = $"Ronda {ronda} de {rondas}",
                        ImageUrl = elegido.Image,
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"{elegido.Favoritos} {corazon} (nº {elegido.Popularidad} en popularidad)"
                        }
                    }).ConfigureAwait(false);
                    var msg = await interactivity.WaitForMessageAsync
                        (xm => (xm.Channel == ctx.Channel) &&
                        (elegido.TitleRomaji != null && (xm.Content.ToLower().Trim() == elegido.TitleRomaji.ToLower().Trim()) || elegido.TitleEnglish != null && (xm.Content.ToLower().Trim() == elegido.TitleEnglish.ToLower().Trim()) ||
                        (elegido.Sinonimos.Find(y => y.ToLower().Trim() == xm.Content.ToLower().Trim()) != null)
                        ) && xm.Author.Id != ctx.Client.CurrentUser.Id || (xm.Content.ToLower() == "cancelar" && xm.Author == ctx.User)
                        , TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["GuessTimeGames"])));
                    if (!msg.TimedOut)
                    {
                        if (msg.Result.Author == ctx.User && msg.Result.Content.ToLower() == "cancelar")
                        {
                            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                            {
                                Title = "¡Juego cancelado!",
                                Description = $"El nombre era: [{elegido.TitleRomaji}]({elegido.SiteUrl})",
                                Color = DiscordColor.Red
                            }).ConfigureAwait(false);
                            await funcionesJuegos.GetResultados(ctx, participantes, lastRonda, settings.Dificultad, "manga");
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
                            Description = $"El nombre es: [{elegido.TitleRomaji}]({elegido.SiteUrl})",
                            Color = DiscordColor.Green
                        }).ConfigureAwait(false);
                    }
                    else
                    {
                        await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = "¡Nadie ha acertado!",
                            Description = $"El nombre era: [{elegido.TitleRomaji}]({elegido.SiteUrl})",
                            Color = DiscordColor.Red
                        }).ConfigureAwait(false);
                    }
                    animeList.Remove(animeList[random]);
                }
                await funcionesJuegos.GetResultados(ctx, participantes, rondas, settings.Dificultad, "manga");
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
                                                    for (int ronda = 1; ronda <= rondas; ronda++)
                                                    {
                                                        lastRonda = ronda;
                                                        int random = funciones.GetNumeroRandom(0, animeList.Count - 1);
                                                        Anime elegido1 = animeList[random];
                                                        DiscordEmoji corazon = DiscordEmoji.FromName(ctx.Client, ":heart:");
                                                        await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                                                        {
                                                            Color = DiscordColor.Gold,
                                                            Title = $"Adivina el {elegido}",
                                                            Description = $"Ronda {ronda} de {rondas}",
                                                            ImageUrl = elegido1.Image,
                                                            Footer = new DiscordEmbedBuilder.EmbedFooter
                                                            {
                                                                Text = $"{elegido1.Favoritos} {corazon} (nº {elegido1.Popularidad} en popularidad)"
                                                            }
                                                        }).ConfigureAwait(false);
                                                        var msg = await interactivity.WaitForMessageAsync
                                                            (xm => (xm.Channel == ctx.Channel) && (xm.Author.Id != ctx.Client.CurrentUser.Id) &&
                                                            ((xm.Content.ToLower() == "cancelar" && xm.Author == ctx.User) ||
                                                            (elegido1.TitleRomaji != null && (xm.Content.ToLower().Trim() == elegido1.TitleRomaji.ToLower().Trim())) || (elegido1.TitleEnglish != null && (xm.Content.ToLower().Trim() == elegido1.TitleEnglish.ToLower().Trim())) ||
                                                            (elegido1.Sinonimos.Find(y => y.ToLower().Trim() == xm.Content.ToLower().Trim()) != null)
                                                            ), TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["GuessTimeGames"])));
                                                        if (!msg.TimedOut)
                                                        {
                                                            if (msg.Result.Author == ctx.User && msg.Result.Content.ToLower() == "cancelar")
                                                            {
                                                                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                                                                {
                                                                    Title = "¡Juego cancelado!",
                                                                    Description = $"El nombre era: [{elegido1.TitleRomaji}]({elegido1.SiteUrl})",
                                                                    Color = DiscordColor.Red
                                                                }).ConfigureAwait(false);
                                                                await funcionesJuegos.GetResultados(ctx, participantes, lastRonda, elegido, "tag");
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
                                                                Description = $"El nombre es: [{elegido1.TitleRomaji}]({elegido1.SiteUrl})",
                                                                Color = DiscordColor.Green
                                                            }).ConfigureAwait(false);
                                                        }
                                                        else
                                                        {
                                                            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                                                            {
                                                                Title = "¡Nadie ha acertado!",
                                                                Description = $"El nombre era: [{elegido1.TitleRomaji}]({elegido1.SiteUrl})",
                                                                Color = DiscordColor.Red
                                                            }).ConfigureAwait(false);
                                                        }
                                                        animeList.Remove(animeList[random]);
                                                    }
                                                    await funcionesJuegos.GetResultados(ctx, participantes, rondas, elegido, "tag");
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
                int lastRonda;
                for (int ronda = 1; ronda <= rondas; ronda++)
                {
                    lastRonda = ronda;
                    int random = funciones.GetNumeroRandom(0, animeList.Count - 1);
                    Anime elegido = animeList[random];
                    DiscordEmoji corazon = DiscordEmoji.FromName(ctx.Client, ":heart:");
                    string estudiosStr = $"Los estudios de [{elegido.TitleRomaji}]({elegido.SiteUrl}) son:\n";
                    foreach (var studio in elegido.Estudios)
                    {
                        estudiosStr += $"- [{studio.Nombre}]({studio.SiteUrl})\n";
                    }
                    string estudiosStrGood = funciones.NormalizarDescription(estudiosStr);
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Gold,
                        Title = "Adivina el estudio del anime",
                        Description = $"Ronda {ronda} de {rondas}",
                        ImageUrl = elegido.Image,
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"{elegido.Favoritos} {corazon} (nº {elegido.Popularidad} en popularidad)"
                        }
                    }).ConfigureAwait(false);
                    var msg = await interactivity.WaitForMessageAsync
                        (xm => (xm.Channel == ctx.Channel) && (xm.Author.Id != ctx.Client.CurrentUser.Id) &&
                        ((xm.Content.ToLower() == "cancelar" && xm.Author == ctx.User) ||
                        (elegido.Estudios.Find(y => y.Nombre.ToLower().Trim() == xm.Content.ToLower().Trim()) != null)
                        ), TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["GuessTimeGames"])));
                    if (!msg.TimedOut)
                    {
                        if (msg.Result.Author == ctx.User && msg.Result.Content.ToLower() == "cancelar")
                        {
                            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                            {
                                Title = "¡Juego cancelado!", 
                                Description = $"{estudiosStrGood}",
                                Color = DiscordColor.Red
                            }).ConfigureAwait(false);
                            await funcionesJuegos.GetResultados(ctx, participantes, lastRonda, settings.Dificultad, "estudio");
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
                            Description = $"{estudiosStrGood}",
                            Color = DiscordColor.Green
                        }).ConfigureAwait(false);
                    }
                    else
                    {
                        await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = "¡Nadie ha acertado!",
                            Description = $"{estudiosStrGood}",
                            Color = DiscordColor.Red
                        }).ConfigureAwait(false);
                    }
                    animeList.Remove(animeList[random]);
                }
                await funcionesJuegos.GetResultados(ctx, participantes, rondas, settings.Dificultad, "estudio");
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
                int lastRonda;
                for (int ronda = 1; ronda <= rondas; ronda++)
                {
                    lastRonda = ronda;
                    int random = funciones.GetNumeroRandom(0, animeList.Count - 1);
                    Anime elegido = animeList[random];
                    DiscordEmoji corazon = DiscordEmoji.FromName(ctx.Client, ":heart:");
                    string protagonistasStr = $"Los protagonistas de [{elegido.TitleRomaji}]({elegido.SiteUrl}) son:\n";
                    foreach (var personaje in elegido.Personajes)
                    {
                        protagonistasStr += $"- [{personaje.NameFull}]({personaje.SiteUrl})\n";
                    }
                    string protagonistasStrGood = funciones.NormalizarDescription(protagonistasStr);
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Gold,
                        Title = "Adivina el protagonista del anime",
                        Description = $"Ronda {ronda} de {rondas}",
                        ImageUrl = elegido.Image,
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"{elegido.Favoritos} {corazon} (nº {elegido.Popularidad} en popularidad)"
                        }
                    }).ConfigureAwait(false);
                    var msg = await interactivity.WaitForMessageAsync
                        (xm => (xm.Channel == ctx.Channel) && (xm.Author.Id != ctx.Client.CurrentUser.Id) &&
                        ((xm.Content.ToLower() == "cancelar" && xm.Author == ctx.User) || 
                        (elegido.Personajes.Find(x => x.NameFull != null && x.NameFull.ToLower().Trim() == xm.Content.ToLower().Trim()) != null) ||
                        (elegido.Personajes.Find(x => x.NameFirst != null && x.NameFirst.ToLower().Trim() == xm.Content.ToLower().Trim()) != null) ||
                        (elegido.Personajes.Find(x => x.NameLast != null && x.NameLast.ToLower().Trim() == xm.Content.ToLower().Trim()) != null)
                        ), TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["GuessTimeGames"])));
                    if (!msg.TimedOut)
                    {
                        if (msg.Result.Author == ctx.User && msg.Result.Content.ToLower() == "cancelar")
                        {
                            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                            {
                                Title = "¡Juego cancelado!",
                                Description = $"{protagonistasStrGood}",
                                Color = DiscordColor.Red
                            }).ConfigureAwait(false);
                            await funcionesJuegos.GetResultados(ctx, participantes, lastRonda, settings.Dificultad, "protagonista");
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
                            Description = $"{protagonistasStrGood}",
                            Color = DiscordColor.Green
                        }).ConfigureAwait(false);
                    }
                    else
                    {
                        await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = "¡Nadie ha acertado!",
                            Description = $"{protagonistasStrGood}",
                            Color = DiscordColor.Red
                        }).ConfigureAwait(false);
                    }
                    animeList.Remove(animeList[random]);
                }
                await funcionesJuegos.GetResultados(ctx, participantes, rondas, settings.Dificultad, "protagonista");
            }
            else
            {
                var error = await ctx.Channel.SendMessageAsync(settings.MsgError).ConfigureAwait(false);
                await Task.Delay(5000);
                await funciones.BorrarMensaje(ctx, error.Id);
            }
        }

        [Command("rankingC"), Aliases("statsC", "leaderboardC"), Description("Estadisticas de adivina el personaje."), RequireGuild]
        public async Task EstadisticasAdivinaPersonaje(CommandContext ctx, string flag = null)
        {
            var builder = await funcionesJuegos.GetEstadisticas(ctx, "personaje", flag);
            await ctx.Channel.SendMessageAsync(embed: builder);
            await funciones.ChequearVotoTopGG(ctx);
        }

        [Command("rankingA"), Aliases("statsA", "leaderboardA"), Description("Estadisticas de adivina el anime."), RequireGuild]
        public async Task EstadisticasAdivinaAnime(CommandContext ctx, string flag = null)
        {
            var builder = await funcionesJuegos.GetEstadisticas(ctx, "anime", flag);
            await ctx.Channel.SendMessageAsync(embed: builder);
            await funciones.ChequearVotoTopGG(ctx);
        }

        [Command("rankingM"), Aliases("statsM", "leaderboardM"), Description("Estadisticas de adivina el anime."), RequireGuild]
        public async Task EstadisticasAdivinaManga(CommandContext ctx, string flag = null)
        {
            var builder = await funcionesJuegos.GetEstadisticas(ctx, "manga", flag);
            await ctx.Channel.SendMessageAsync(embed: builder);
            await funciones.ChequearVotoTopGG(ctx);
        }

        [Command("rankingT"), Aliases("statsT", "leaderboardT"), Description("Estadisticas de adivina el anime."), RequireGuild]
        public async Task EstadisticasAdivinaTag(CommandContext ctx, string flag = null)
        {
            var mensaje = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder { 
                Title = "Comando deshabilitado temporalmente",
                Description = "Por mantenimientos",
                Color = DiscordColor.Red,
                Footer = funciones.GetFooter(ctx)
            });
            await Task.Delay(5000);
            await funciones.BorrarMensaje(ctx, mensaje.Id);
            /*
            var builder = await funcionesJuegos.GetEstadisticasTag(ctx, flag);
            var msg = await ctx.Channel.SendMessageAsync(embed: builder);
            await funciones.ChequearVotoTopGG(ctx);
            if (builder.Title == "Error")
            {
                await Task.Delay(5000);
                await funciones.BorrarMensaje(ctx, msg.Id);
            }
            */
        }

        [Command("rankingS"), Aliases("statsS", "leaderboardS"), Description("Estadisticas de adivina el estudio."), RequireGuild]
        public async Task EstadisticasAdivinaEstudio(CommandContext ctx, string flag = null)
        {
            var builder = await funcionesJuegos.GetEstadisticas(ctx, "estudio", flag);
            await ctx.Channel.SendMessageAsync(embed: builder);
            await funciones.ChequearVotoTopGG(ctx);
        }

        [Command("rankingP"), Aliases("statsP", "leaderboardP"), Description("Estadisticas de adivina el protagonista."), RequireGuild]
        public async Task EstadisticasAdivinaProtagonista(CommandContext ctx, string flag = null)
        {
            var builder = await funcionesJuegos.GetEstadisticas(ctx, "protagonista", flag);
            await ctx.Channel.SendMessageAsync(embed: builder);
            await funciones.ChequearVotoTopGG(ctx);
        }
    }
}
