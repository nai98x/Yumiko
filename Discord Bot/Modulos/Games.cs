using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;
using Miki.GraphQL;
using Newtonsoft.Json;
using System.Collections.Generic;
using Miki.Anilist;
using DSharpPlus.Entities;
using System;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace Discord_Bot.Modulos
{
    public class Games : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("quizC"), Aliases("adivinaelpersonaje")]
        [Description("Adivina el personaje")]
        public async Task QuizCharactersGlobal(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            var msgBienvenida = await ctx.RespondAsync($"Adivina el personaje! Sesión inciada por **{ctx.User.Username}#{ctx.User.Discriminator}**!");
            var msgCntRondas = await ctx.RespondAsync(embed : new DiscordEmbedBuilder { 
                Title = "Elige la cantidad de rondas",
                Description = "Por ejemplo: 10"
            });
            var msgRondasInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(10));
            if (!msgRondasInter.TimedOut)
            {
                int rondas;
                bool result = int.TryParse(msgRondasInter.Result.Content, out rondas);
                if (result)
                {
                    var msgDificultad = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "Elije la dificultad",
                        Description = "1- Fácil (300 personajes)\n2- Medio (1000 personajes)\n3- Dificil (3000 personajes)"
                    });
                    var msgDificultadInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(10));
                    if (!msgDificultadInter.TimedOut)
                    {
                        int dificultad;
                        result = int.TryParse(msgDificultadInter.Result.Content, out dificultad);
                        if (result)
                        {
                            if( dificultad == 1 || dificultad == 2 ||dificultad == 3)
                            {
                                int iteraciones;
                                switch (dificultad)
                                {
                                    case 1:
                                        iteraciones = 6;
                                        break;
                                    case 2:
                                        iteraciones = 20;
                                        break;
                                    case 3:
                                        iteraciones = 60;
                                        break;
                                    default:
                                        iteraciones = 20;
                                        break;
                                }
                                await ctx.Message.DeleteAsync("Auto borrado de Yumiko");
                                await ctx.RespondAsync($"Adivina el personaje! Sesión inciada por **{ctx.User.Username}#{ctx.User.Discriminator}**! Rondas: **{rondas}** Personajes: **{iteraciones*50}**");
                                List<Character> characterList = new List<Character>();
                                var graphQLClient = new GraphQLClient("https://graphql.anilist.co");
                                Random rnd = new Random();

                                var joinEmbed = new DiscordEmbedBuilder
                                {
                                    Title = "Adivina el personaje",
                                    Description = "Reacciona para participar!"
                                };
                                var joinMessage = await ctx.Channel.SendMessageAsync(embed: joinEmbed).ConfigureAwait(false);
                                var emojiReaccion = DiscordEmoji.FromName(ctx.Client, ":pencil:");
                                await joinMessage.CreateReactionAsync(emojiReaccion).ConfigureAwait(false);
                                var resultado = await interactivity.CollectReactionsAsync(joinMessage, TimeSpan.FromSeconds(15));
                                List<UsuarioJuego> participantes = new List<UsuarioJuego>();
                                foreach (Reaction rec in resultado)
                                {
                                    if (rec.Emoji == emojiReaccion)
                                    {
                                        foreach (DiscordUser partic in rec.Users)
                                        {
                                            if (!partic.IsBot)
                                            {
                                                participantes.Add(new UsuarioJuego()
                                                {
                                                    usuario = partic,
                                                    puntaje = 0
                                                });
                                            }
                                        }
                                    }
                                }
                                string participantes1 = "";
                                foreach (UsuarioJuego uj in participantes)
                                {
                                    participantes1 += $"- {uj.usuario.Username}#{uj.usuario.Discriminator}\n";
                                }
                                await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
                                {
                                    Title = "Jugadores",
                                    Description = participantes1
                                }).ConfigureAwait(false);
                                if(participantes.Count > 0)
                                {
                                    DiscordMessage mensaje = await ctx.RespondAsync($"Obteniendo pesonajes...").ConfigureAwait(false);
                                    for (int i = 1; i <= iteraciones; i++)
                                    {
                                        string queryIni = "{" +
                                        "Page (page: ";
                                        string queryFin = ") { " +
                                            "characters(sort: FAVOURITES_DESC){" +
                                                "siteUrl," +
                                                "name{" +
                                                    "first," +
                                                    "last," +
                                                    "full" +
                                                "}" +
                                                "image{" +
                                                    "large" +
                                                "}" +
                                            "}" +
                                        "}" +
                                        "}";
                                        string query = queryIni + i + queryFin;
                                        var response = await graphQLClient.QueryAsync(query, new { page = 1 });
                                        var data = JsonConvert.DeserializeObject<dynamic>(response);
                                        foreach (var x in data.data.Page.characters)
                                        {
                                            characterList.Add(new Character()
                                            {
                                                Image = x.image.large,
                                                NameFull = x.name.full,
                                                NameFirst = x.name.first,
                                                NameLast = x.name.last,
                                                SiteUrl = x.siteUrl
                                            });
                                        }
                                    }
                                    await mensaje.DeleteAsync("Auto borrado de Yumiko");
                                    for (int ronda = 1; ronda <= rondas; ronda++)
                                    {
                                        int random = funciones.GetNumeroRandom(0, characterList.Count - 1);
                                        Character elegido = characterList[random];
                                        await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                                        {
                                            Color = DiscordColor.Gold,
                                            Title = "Adivina el personaje",
                                            Description = $"Ronda {ronda} de {rondas}",
                                            ImageUrl = elegido.Image
                                        }).ConfigureAwait(false);
                                        var msg = await interactivity.WaitForMessageAsync
                                            (xm => xm.Channel == ctx.Channel &&
                                            (xm.Content.ToLower() == elegido.NameFull.ToLower() || xm.Content.ToLower() == elegido.NameFirst.ToLower() || (elegido.NameLast != null && xm.Content.ToLower() == elegido.NameLast.ToLower())) || (xm.Content.ToLower() == "cancelar" && xm.Author == ctx.User)
                                            , TimeSpan.FromSeconds(20));
                                        if (!msg.TimedOut)
                                        {
                                            if(msg.Result.Author == ctx.User && msg.Result.Content.ToLower() == "cancelar")
                                            {
                                                await ctx.RespondAsync($"El juego ha sido cancelado por **{ctx.User.Username}#{ctx.User.Discriminator}**").ConfigureAwait(false);
                                                string results = "";
                                                int total1 = 0;
                                                participantes.OrderBy(x => x.puntaje > x.puntaje);
                                                foreach (UsuarioJuego uj in participantes)
                                                {
                                                    results += $"- {uj.usuario.Username}#{uj.usuario.Discriminator}: {uj.puntaje} aciertos\n";
                                                    total1 += uj.puntaje;
                                                }
                                                results += $"\nTotal ({total1}/{rondas})";
                                                await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
                                                {
                                                    Title = "Resultados",
                                                    Description = results
                                                }).ConfigureAwait(false);
                                                return;
                                            }
                                            DiscordMember acertador = await ctx.Guild.GetMemberAsync(msg.Result.Author.Id); 
                                            UsuarioJuego usr = participantes.Find(x => x.usuario == msg.Result.Author);
                                            if (usr != null)
                                            {
                                                usr.puntaje++;
                                            }
                                            else
                                            {
                                                participantes.Add(new UsuarioJuego()
                                                {
                                                    usuario = msg.Result.Author,
                                                    puntaje = 1
                                                });
                                            }
                                            await ctx.RespondAsync($"**{acertador.DisplayName}** ha acertado! {elegido.NameFull}").ConfigureAwait(false);
                                        }
                                        else
                                        {
                                            await ctx.RespondAsync($"Nadie ha acertado! El nombre era **{elegido.NameFull}**").ConfigureAwait(false);
                                        }
                                    }
                                    string resultados = "";
                                    participantes.OrderBy(x => x.puntaje > x.puntaje);
                                    int tot = 0;
                                    foreach (UsuarioJuego uj in participantes)
                                    {
                                        resultados += $"- {uj.usuario.Username}#{uj.usuario.Discriminator}: {uj.puntaje} aciertos\n";
                                        tot += uj.puntaje;
                                    }
                                    resultados += $"\nTotal ({tot}/{rondas})";
                                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
                                    {
                                        Title = "Resultados",
                                        Description = resultados
                                    }).ConfigureAwait(false);
                                }
                                else
                                {
                                    var error = await ctx.RespondAsync("Nadie se ha inscripto para jugar").ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                var error = await ctx.RespondAsync("La dificultad debe ser 1, 2 o 3").ConfigureAwait(false);
                                // Msg de pusiste el numero mal (ingresar dificultad out of range) borrar msg anteriores
                            }
                        }
                        else
                        {
                            var error = await ctx.RespondAsync("La dificultad debe ser un número (1, 2 o 3)").ConfigureAwait(false);
                            // Msg de pusiste el numero mal (ingresar dificultad) borrar msg anteriores
                        }
                    }
                    else
                    {
                        var error = await ctx.RespondAsync("Tiempo agotado esperando la dificultad").ConfigureAwait(false);
                        // Msg de timeout (ingresar dificultad) borrar msg anteriores
                    }
                }
                else
                {
                    var error = await ctx.RespondAsync("La cantidad de rondas debe ser un numero").ConfigureAwait(false);
                    // Msg de pusiste el numero mal (ingresar rondas) borrar msg anteriores
                }
            }
            else
            {
                var error = await ctx.RespondAsync("Tiempo agotado esperando la cantidad de rondas").ConfigureAwait(false);
                // Msg de timeout (ingresar rondas) borrar msg anteriores
            }
        }

        [Command("quizA"), Aliases("adivinaelanime")]
        [Description("Adivina el anime")]
        public async Task QuizAnimeGlobal(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            var msgBienvenida = await ctx.RespondAsync($"Adivina el anime! Sesión inciada por **{ctx.User.Username}#{ctx.User.Discriminator}**!");
            var msgCntRondas = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Elige la cantidad de rondas",
                Description = "Por ejemplo: 10"
            });
            var msgRondasInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(10));
            if (!msgRondasInter.TimedOut)
            {
                int rondas;
                bool result = int.TryParse(msgRondasInter.Result.Content, out rondas);
                if (result)
                {
                    var msgDificultad = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "Elije la dificultad",
                        Description = "1- Fácil (300 animes)\n2- Medio (1000 animes)\n3- Dificil (3000 animes)"
                    });
                    var msgDificultadInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(10));
                    if (!msgDificultadInter.TimedOut)
                    {
                        int dificultad;
                        result = int.TryParse(msgDificultadInter.Result.Content, out dificultad);
                        if (result)
                        {
                            if (dificultad == 1 || dificultad == 2 || dificultad == 3)
                            {
                                int iteraciones;
                                switch (dificultad)
                                {
                                    case 1:
                                        iteraciones = 6;
                                        break;
                                    case 2:
                                        iteraciones = 20;
                                        break;
                                    case 3:
                                        iteraciones = 60;
                                        break;
                                    default:
                                        iteraciones = 20;
                                        break;
                                }
                                await ctx.Message.DeleteAsync("Auto borrado de Yumiko");
                                await ctx.RespondAsync($"Adivina el anime! Sesión inciada por **{ctx.User.Username}#{ctx.User.Discriminator}**! Rondas: **{rondas}** Animes: **{iteraciones * 50}**");
                                List<Anime> animeList = new List<Anime>();
                                var graphQLClient = new GraphQLClient("https://graphql.anilist.co");
                                Random rnd = new Random();
                                var joinEmbed = new DiscordEmbedBuilder
                                {
                                    Title = "Adivina el anime",
                                    Description = "Reacciona para participar!"
                                };
                                var joinMessage = await ctx.Channel.SendMessageAsync(embed: joinEmbed).ConfigureAwait(false);
                                var emojiReaccion = DiscordEmoji.FromName(ctx.Client, ":pencil:");
                                await joinMessage.CreateReactionAsync(emojiReaccion).ConfigureAwait(false);
                                var resultado = await interactivity.CollectReactionsAsync(joinMessage, TimeSpan.FromSeconds(15));
                                List<UsuarioJuego> participantes = new List<UsuarioJuego>();
                                foreach (Reaction rec in resultado)
                                {
                                    if (rec.Emoji == emojiReaccion)
                                    {
                                        foreach (DiscordUser partic in rec.Users)
                                        {
                                            if (!partic.IsBot)
                                            {
                                                participantes.Add(new UsuarioJuego()
                                                {
                                                    usuario = partic,
                                                    puntaje = 0
                                                });
                                            }
                                        }
                                    }
                                }
                                string participantes1 = "";
                                foreach (UsuarioJuego uj in participantes)
                                {
                                    participantes1 += $"- {uj.usuario.Username}#{uj.usuario.Discriminator}\n";
                                }
                                await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
                                {
                                    Title = "Jugadores",
                                    Description = participantes1
                                }).ConfigureAwait(false);
                                if(participantes.Count > 0)
                                {
                                    DiscordMessage mensaje = await ctx.RespondAsync($"Obteniendo animes...").ConfigureAwait(false);
                                    for (int i = 1; i <= iteraciones; i++)
                                    {
                                        string queryIni = "{" +
                                        "Page (page: ";
                                        string queryFin = ") { " +
                                            "media(sort: POPULARITY_DESC, type: ANIME){" +
                                                "siteUrl," +
                                                "title{" +
                                                    "romaji," +
                                                    "english," +
                                                "}" +
                                                "coverImage{" +
                                                    "large" +
                                                "}" +
                                            "}" +
                                        "}" +
                                        "}";
                                        string query = queryIni + i + queryFin;
                                        var response = await graphQLClient.QueryAsync(query, new { page = 1 });
                                        var data = JsonConvert.DeserializeObject<dynamic>(response);
                                        foreach (var x in data.data.Page.media)
                                        {
                                            animeList.Add(new Anime()
                                            {
                                                Image = x.coverImage.large,
                                                TitleEnglish = x.title.romaji,
                                                TitleRomaji = x.title.english,
                                                SiteUrl = x.siteUrl
                                            });
                                        }
                                    }
                                    await mensaje.DeleteAsync("Auto borrado de Yumiko");
                                    for (int ronda = 1; ronda <= rondas; ronda++)
                                    {
                                        int random = funciones.GetNumeroRandom(0, animeList.Count - 1);
                                        Anime elegido = animeList[random];
                                        await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                                        {
                                            Color = DiscordColor.Gold,
                                            Title = "Adivina el anime",
                                            Description = $"Ronda {ronda} de {rondas}",
                                            ImageUrl = elegido.Image
                                        }).ConfigureAwait(false);
                                        var msg = await interactivity.WaitForMessageAsync
                                            (xm => xm.Channel == ctx.Channel &&
                                            ((elegido.TitleEnglish != null && elegido.TitleEnglish.ToLower() == xm.Content.ToLower()) || elegido.TitleRomaji.ToLower() == xm.Content.ToLower()) || (xm.Content.ToLower() == "cancelar" && xm.Author == ctx.User)
                                            , TimeSpan.FromSeconds(20));
                                        if (!msg.TimedOut)
                                        {
                                            if (msg.Result.Author == ctx.User && msg.Result.Content.ToLower() == "cancelar")
                                            {
                                                await ctx.RespondAsync($"El juego ha sido cancelado por **{ctx.User.Username}#{ctx.User.Discriminator}**").ConfigureAwait(false);
                                                string results = "";
                                                participantes.OrderBy(x => x.puntaje > x.puntaje);
                                                int totall = 0;
                                                foreach (UsuarioJuego uj in participantes)
                                                {
                                                    results += $"- {uj.usuario.Username}#{uj.usuario.Discriminator}: {uj.puntaje} aciertos\n";
                                                    totall += uj.puntaje;
                                                }
                                                results += $"\nTotal ({totall}/{rondas})";
                                                await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
                                                {
                                                    Title = "Resultados",
                                                    Description = results
                                                }).ConfigureAwait(false);
                                                return;
                                            }
                                            DiscordMember acertador = await ctx.Guild.GetMemberAsync(msg.Result.Author.Id);
                                            UsuarioJuego usr = participantes.Find(x => x.usuario == msg.Result.Author);
                                            if (usr != null)
                                            {
                                                usr.puntaje++;
                                            }
                                            else
                                            {
                                                participantes.Add(new UsuarioJuego()
                                                {
                                                    usuario = msg.Result.Author,
                                                    puntaje = 1
                                                });
                                            }
                                            await ctx.RespondAsync($"**{acertador.DisplayName}** ha acertado! {elegido.TitleRomaji}").ConfigureAwait(false);
                                        }
                                        else
                                        {
                                            await ctx.RespondAsync($"Nadie ha acertado! El nombre era **{elegido.TitleRomaji}**").ConfigureAwait(false);
                                        }
                                    }
                                    string resultados = "";
                                    participantes.OrderBy(x => x.puntaje > x.puntaje);
                                    int tot = 0;
                                    foreach (UsuarioJuego uj in participantes)
                                    {
                                        resultados += $"- {uj.usuario.Username}#{uj.usuario.Discriminator}: {uj.puntaje} aciertos\n";
                                        tot += uj.puntaje;
                                    }
                                    resultados += $"\nTotal ({tot}/{rondas})";
                                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
                                    {
                                        Title = "Resultados",
                                        Description = resultados
                                    }).ConfigureAwait(false);
                                }
                                else
                                {
                                    var error = await ctx.RespondAsync("Nadie se ha inscripto para jugar").ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                var error = await ctx.RespondAsync("La dificultad debe ser 1, 2 o 3").ConfigureAwait(false);
                                // Msg de pusiste el numero mal (ingresar dificultad out of range) borrar msg anteriores
                            }
                        }
                        else
                        {
                            var error = await ctx.RespondAsync("La dificultad debe ser un número (1, 2 o 3)").ConfigureAwait(false);
                            // Msg de pusiste el numero mal (ingresar dificultad) borrar msg anteriores
                        }
                    }
                    else
                    {
                        var error = await ctx.RespondAsync("Tiempo agotado esperando la dificultad").ConfigureAwait(false);
                        // Msg de timeout (ingresar dificultad) borrar msg anteriores
                    }
                }
                else
                {
                    var error = await ctx.RespondAsync("La cantidad de rondas debe ser un numero").ConfigureAwait(false);
                    // Msg de pusiste el numero mal (ingresar rondas) borrar msg anteriores
                }
            }
            else
            {
                var error = await ctx.RespondAsync("Tiempo agotado esperando la cantidad de rondas").ConfigureAwait(false);
                // Msg de timeout (ingresar rondas) borrar msg anteriores
            }
        }
    }
}
