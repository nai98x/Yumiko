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

namespace Discord_Bot.Modulos
{
    public class Juegos : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();
        private readonly GraphQLHttpClient graphQLClient = new GraphQLHttpClient("https://graphql.anilist.co", new NewtonsoftJsonSerializer());

        [Command("quizC"), Aliases("adivinaelpersonaje"), Description("Empieza el juego de adivina el personaje."), RequireGuild]
        public async Task QuizCharactersGlobal(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            SettingsJuego settings = await funciones.InicializarJuego(ctx, interactivity);
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
                await ctx.RespondAsync(embed: embebido).ConfigureAwait(false);
                List<Character> characterList = new List<Character>();
                Random rnd = new Random();
                List<UsuarioJuego> participantes = new List<UsuarioJuego>();
                DiscordMessage mensaje = await ctx.RespondAsync($"Obteniendo personajes...").ConfigureAwait(false);
                string query = "query($pagina : Int){" +
                        "   Page(page: $pagina){" +
                        "       characters(sort:";
                query += settings.Orden;
                query +="){" +
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
                        DiscordMessage msg;
                        switch (ex.Message)
                        {
                            default:
                                msg = await ctx.RespondAsync($"Error inesperado: {ex.Message}").ConfigureAwait(false);
                                break;
                        }
                        await Task.Delay(3000);
                        await msg.DeleteAsync("Auto borrado de Yumiko");
                        return;
                    }
                }
                await mensaje.DeleteAsync("Auto borrado de Yumiko");
                int lastRonda;
                for (int ronda = 1; ronda <= rondas; ronda++)
                {
                    lastRonda = ronda;
                    int random = funciones.GetNumeroRandom(0, characterList.Count - 1);
                    Character elegido = characterList[random];
                    DiscordEmoji corazon = DiscordEmoji.FromName(ctx.Client, ":heart:");
                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder
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
                            await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                            {
                                Title = "¡Juego cancelado!",
                                Description = $"El nombre era: [{elegido.NameFull}]({elegido.SiteUrl})",
                                Color = DiscordColor.Red
                            }).ConfigureAwait(false);
                            await funciones.GetResultados(ctx, participantes, lastRonda, settings.Dificultad, "personaje");
                            await ctx.RespondAsync($"El juego ha sido **cancelado** por **{ctx.User.Username}#{ctx.User.Discriminator}**").ConfigureAwait(false);
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
                        await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = $"¡**{acertador.DisplayName}** ha acertado!",
                            Description = $"El nombre es: [{elegido.NameFull}]({elegido.SiteUrl})",
                            Color = DiscordColor.Green
                        }).ConfigureAwait(false);
                    }
                    else
                    {
                        await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = "¡Nadie ha acertado!",
                            Description = $"El nombre era: [{elegido.NameFull}]({elegido.SiteUrl})",
                            Color = DiscordColor.Red
                        }).ConfigureAwait(false);
                    }
                    characterList.Remove(characterList[random]);
                }
                await funciones.GetResultados(ctx, participantes, rondas, settings.Dificultad, "personaje");
            }
            else
            {
                var error = await ctx.RespondAsync(settings.MsgError).ConfigureAwait(false);
            }
        }

        [Command("quizA"), Aliases("adivinaelanime"), Description("Empieza el juego de adivina el anime."), RequireGuild]
        public async Task QuizAnimeGlobal(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            SettingsJuego settings = await funciones.InicializarJuego(ctx, interactivity);
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
                await ctx.RespondAsync(embed: embebido).ConfigureAwait(false);
                Random rnd = new Random();
                List<UsuarioJuego> participantes = new List<UsuarioJuego>();
                DiscordMessage mensaje = await ctx.RespondAsync($"Obteniendo personajes...").ConfigureAwait(false);
                var characterList = new List<Character>();
                string query = "query($pagina : Int){" +
                        "   Page(page: $pagina){" +
                        "       characters(sort: ";
                query += settings.Orden;
                query += "){" +
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
                        DiscordMessage msg;
                        switch (ex.Message)
                        {
                            default:
                                msg = await ctx.RespondAsync($"Error inesperado: {ex.Message}").ConfigureAwait(false);
                                break;
                        }
                        await Task.Delay(3000);
                        await msg.DeleteAsync("Auto borrado de Yumiko");
                        return;
                    }
                }
                await mensaje.DeleteAsync("Auto borrado de Yumiko");
                int lastRonda;
                for (int ronda = 1; ronda <= rondas; ronda++)
                {
                    lastRonda = ronda;
                    int random = funciones.GetNumeroRandom(0, characterList.Count - 1);
                    Character elegido = characterList[random];
                    DiscordEmoji corazon = DiscordEmoji.FromName(ctx.Client, ":heart:");
                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder
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
                            await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                            {
                                Title = "¡Juego cancelado!",
                                Description = descAnimes,
                                Color = DiscordColor.Red
                            }).ConfigureAwait(false);
                            await funciones.GetResultados(ctx, participantes, lastRonda, settings.Dificultad, "anime");
                            await ctx.RespondAsync($"El juego ha sido cancelado por **{ctx.User.Username}#{ctx.User.Discriminator}**").ConfigureAwait(false);
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
                        await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = $"¡**{acertador.DisplayName}** ha acertado!",
                            Description = descAnimes,
                            Color = DiscordColor.Green
                        }).ConfigureAwait(false);
                    }
                    else
                    {
                        await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = "¡Nadie ha acertado!",
                            Description = descAnimes,
                            Color = DiscordColor.Red
                        }).ConfigureAwait(false);
                    }
                    characterList.Remove(characterList[random]);
                }
                await funciones.GetResultados(ctx, participantes, rondas, settings.Dificultad, "anime");
            }
            else
            {
                var error = await ctx.RespondAsync(settings.MsgError).ConfigureAwait(false);
            }
        }

        [Command("quizM"), Aliases("adivinaelmanga"), Description("Empieza el juego de adivina el manga."), RequireGuild]
        public async Task QuizMangaGlobal(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            SettingsJuego settings = await funciones.InicializarJuego(ctx, interactivity);
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
                await ctx.RespondAsync(embed: embebido).ConfigureAwait(false);
                List<Anime> animeList = new List<Anime>();
                Random rnd = new Random();
                List<UsuarioJuego> participantes = new List<UsuarioJuego>();
                DiscordMessage mensaje = await ctx.RespondAsync($"Obteniendo mangas...").ConfigureAwait(false);
                string query = "query($pagina : Int){" +
                        "   Page(page: $pagina){" +
                        "       media(type: MANGA, sort: ";
                query += settings.Orden;
                query += "){" +
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
                        DiscordMessage msg;
                        switch (ex.Message)
                        {
                            default:
                                msg = await ctx.RespondAsync($"Error inesperado: {ex.Message}").ConfigureAwait(false);
                                break;
                        }
                        await Task.Delay(3000);
                        await msg.DeleteAsync("Auto borrado de Yumiko");
                        return;
                    }
                }
                await mensaje.DeleteAsync("Auto borrado de Yumiko");
                int lastRonda;
                for (int ronda = 1; ronda <= rondas; ronda++)
                {
                    lastRonda = ronda;
                    int random = funciones.GetNumeroRandom(0, animeList.Count - 1);
                    Anime elegido = animeList[random];
                    DiscordEmoji corazon = DiscordEmoji.FromName(ctx.Client, ":heart:");
                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder
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
                            await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                            {
                                Title = "¡Juego cancelado!",
                                Description = $"El nombre era: [{elegido.TitleRomaji}]({elegido.SiteUrl})",
                                Color = DiscordColor.Red
                            }).ConfigureAwait(false);
                            await funciones.GetResultados(ctx, participantes, lastRonda, settings.Dificultad, "manga");
                            await ctx.RespondAsync($"El juego ha sido **cancelado** por **{ctx.User.Username}#{ctx.User.Discriminator}**").ConfigureAwait(false);
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
                        await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = $"¡**{acertador.DisplayName}** ha acertado!",
                            Description = $"El nombre es: [{elegido.TitleRomaji}]({elegido.SiteUrl})",
                            Color = DiscordColor.Green
                        }).ConfigureAwait(false);
                    }
                    else
                    {
                        await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = "¡Nadie ha acertado!",
                            Description = $"El nombre era: [{elegido.TitleRomaji}]({elegido.SiteUrl})",
                            Color = DiscordColor.Red
                        }).ConfigureAwait(false);
                    }
                    animeList.Remove(animeList[random]);
                }
                await funciones.GetResultados(ctx, participantes, rondas, settings.Dificultad, "manga");
            }
            else
            {
                var error = await ctx.RespondAsync(settings.MsgError).ConfigureAwait(false);
            }
        }

        [Command("quizT"), Aliases("adivinaeltag"), Description("Empieza el juego de adivina el anime de cierto tag."), RequireGuild]
        public async Task QuizAnimeTagGlobal(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            bool ok = true;
            string msgError = "";
            var msgCntRondas = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
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
                        string query =
                "query{" +
                "   MediaTagCollection{" +
                "       name," +
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
                            List<string> tags = new List<string>();
                            foreach (var x in data.Data.MediaTagCollection)
                            {
                                if (x.isAdult == "false")
                                {
                                    string nombre = x.name;
                                    tags.Add(nombre);
                                }
                            }
                            var preguntaTag = await ctx.RespondAsync("Escribe un tag");
                            var msgTagInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGames"])));
                            if (!msgTagInter.TimedOut)
                            {
                                int numTag = 0;
                                string tagResp = "";
                                List<string> tagsFiltrados = tags.Where(x => x.ToLower().Trim().Contains(msgTagInter.Result.Content.ToLower().Trim())).ToList();
                                foreach (string t in tagsFiltrados)
                                {
                                    numTag++;
                                    tagResp += $"{numTag} - {t}\n";
                                }
                                var msgOpciones = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
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
                                            List<Anime> animeList = new List<Anime>();
                                            Random rnd = new Random();
                                            List<UsuarioJuego> participantes = new List<UsuarioJuego>();
                                            DiscordMessage mensaje = await ctx.RespondAsync($"Obteniendo animes...").ConfigureAwait(false);
                                            string elegido = tagsFiltrados[numTagElegir - 1];
                                            string query1 = "query($pagina : Int){" +
                                                    "   Page(page: $pagina){" +
                                                    "       media(type: ANIME, sort: POPULARITY_DESC, tag: \"" + elegido + "\"){" +
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
                                                        if(x.isAdult == "False")
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
                                                    DiscordMessage msg;
                                                    switch (ex.Message)
                                                    {
                                                        default:
                                                            msg = await ctx.RespondAsync($"Error inesperado: {ex.Message}").ConfigureAwait(false);
                                                            break;
                                                    }
                                                    await Task.Delay(3000);
                                                    await msg.DeleteAsync("Auto borrado de Yumiko");
                                                    return;
                                                }
                                            } while (seguir);
                                            await mensaje.DeleteAsync("Auto borrado de Yumiko");
                                            int lastRonda;
                                            for (int ronda = 1; ronda <= rondas; ronda++)
                                            {
                                                lastRonda = ronda;
                                                int random = funciones.GetNumeroRandom(0, animeList.Count - 1);
                                                Anime elegido1 = animeList[random];
                                                DiscordEmoji corazon = DiscordEmoji.FromName(ctx.Client, ":heart:");
                                                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
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
                                                    (xm => (xm.Channel == ctx.Channel) &&
                                                    (elegido1.TitleRomaji != null && (xm.Content.ToLower().Trim() == elegido1.TitleRomaji.ToLower().Trim()) || elegido1.TitleEnglish != null && (xm.Content.ToLower().Trim() == elegido1.TitleEnglish.ToLower().Trim()) ||
                                                    (elegido1.Sinonimos.Find(y => y.ToLower().Trim() == xm.Content.ToLower().Trim()) != null)
                                                    ) && xm.Author.Id != ctx.Client.CurrentUser.Id || (xm.Content.ToLower() == "cancelar" && xm.Author == ctx.User)
                                                    , TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["GuessTimeGames"])));
                                                if (!msg.TimedOut)
                                                {
                                                    if (msg.Result.Author == ctx.User && msg.Result.Content.ToLower() == "cancelar")
                                                    {
                                                        await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                                                        {
                                                            Title = "¡Juego cancelado!",
                                                            Description = $"El nombre era: [{elegido1.TitleRomaji}]({elegido1.SiteUrl})",
                                                            Color = DiscordColor.Red
                                                        }).ConfigureAwait(false);
                                                        await funciones.GetResultados(ctx, participantes, lastRonda, elegido, "tag");
                                                        await ctx.RespondAsync($"El juego ha sido **cancelado** por **{ctx.User.Username}#{ctx.User.Discriminator}**").ConfigureAwait(false);
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
                                                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                                                    {
                                                        Title = $"¡**{acertador.DisplayName}** ha acertado!",
                                                        Description = $"El nombre es: [{elegido1.TitleRomaji}]({elegido1.SiteUrl})",
                                                        Color = DiscordColor.Green
                                                    }).ConfigureAwait(false);
                                                }
                                                else
                                                {
                                                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                                                    {
                                                        Title = "¡Nadie ha acertado!",
                                                        Description = $"El nombre era: [{elegido1.TitleRomaji}]({elegido1.SiteUrl})",
                                                        Color = DiscordColor.Red
                                                    }).ConfigureAwait(false);
                                                }
                                                animeList.Remove(animeList[random]);
                                            }
                                            await funciones.GetResultados(ctx, participantes, rondas, elegido, "tag");
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
                                msgError = "Tiempo agotado esperando el tag";
                            }
                        }
                        catch (Exception ex)
                        {
                            DiscordMessage msg;
                            switch (ex.Message)
                            {
                                default:
                                    msg = await ctx.RespondAsync($"Error inesperado: {ex.Message}").ConfigureAwait(false);
                                    break;
                            }
                            await Task.Delay(3000);
                            await msg.DeleteAsync("Auto borrado de Yumiko");
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
                var error = await ctx.RespondAsync(msgError);
                await Task.Delay(3000);
                await error.DeleteAsync("Auto borrado de Yumiko");
            }
        }

        [Command("leaderboardC"), Aliases("rankingc"), Description("Estadisticas de adivina el personaje."), RequireGuild]
        public async Task EstadisticasAdivinaPersonaje(CommandContext ctx)
        {
            var builder = await funciones.GetEstadisticas(ctx, "personaje");
            await ctx.RespondAsync(embed: builder);
        }

        [Command("leaderboardA"), Aliases("rankinga"), Description("Estadisticas de adivina el anime."), RequireGuild]
        public async Task EstadisticasAdivinaAnime(CommandContext ctx)
        {
            var builder = await funciones.GetEstadisticas(ctx, "anime");
            await ctx.RespondAsync(embed: builder);
        }

        [Command("leaderboardM"), Aliases("rankingm"), Description("Estadisticas de adivina el anime."), RequireGuild]
        public async Task EstadisticasAdivinaManga(CommandContext ctx)
        {
            var builder = await funciones.GetEstadisticas(ctx, "manga");
            await ctx.RespondAsync(embed: builder);
        }

        [Command("statsC"), Description("Estadisticas de adivina el personaje por usuario."), RequireGuild]
        public async Task EstadisticasAdivinaPersonajeUsuario(CommandContext ctx, DiscordUser usuario = null)
        {
            if (usuario == null)
                usuario = ctx.User;

            var builder = funciones.GetEstadisticasUsuario(ctx, "personaje", usuario);
            await ctx.RespondAsync(embed: builder);
        }

        [Command("statsA"), Description("Estadisticas de adivina el anime por usuario."), RequireGuild]
        public async Task EstadisticasAdivinaAnimeUsuario(CommandContext ctx, DiscordUser usuario = null)
        {
            if (usuario == null)
                usuario = ctx.User;

            var builder = funciones.GetEstadisticasUsuario(ctx, "anime", usuario);
            await ctx.RespondAsync(embed: builder);
        }

        [Command("statsM"), Description("Estadisticas de adivina el manga por usuario."), RequireGuild]
        public async Task EstadisticasAdivinaMangaUsuario(CommandContext ctx, DiscordUser usuario = null)
        {
            if (usuario == null)
                usuario = ctx.User;

            var builder = funciones.GetEstadisticasUsuario(ctx, "manga", usuario);
            await ctx.RespondAsync(embed: builder);
        }
    }
}
