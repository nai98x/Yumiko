using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using YumikoBot;
using DiscordBotsList.Api;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus;
using System.Configuration;
using Google.Cloud.Firestore;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using DSharpPlus.Interactivity;
using static DSharpPlus.Entities.DiscordEmbedBuilder;
using DSharpPlus.SlashCommands;

namespace Discord_Bot
{
    public class FuncionesAuxiliares
    {
        private readonly GraphQLHttpClient graphQLClient = new("https://graphql.anilist.co", new NewtonsoftJsonSerializer());

        public FirestoreDb GetFirestoreClient()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"firebase.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
            return FirestoreDb.Create(ConfigurationManager.AppSettings["NombreDbFirebase"]);
        }

        public Context GetContext(CommandContext ctx)
        {
            return new()
            {
                Client = ctx.Client,
                Command = ctx.Command,
                Channel = ctx.Channel,
                Guild = ctx.Guild,
                Member = ctx.Member,
                Message = ctx.Message,
                Prefix = ctx.Prefix,
                User = ctx.User
            };
        }

        public Context GetContext(InteractionContext itx)
        {
            return new()
            {
                Client = itx.Client,
                Channel = itx.Channel,
                Guild = itx.Guild,
                Member = itx.Member,
                User = itx.User,
                Interaction = itx.Interaction
            };
        }

        public async Task MovidoASlashCommand(CommandContext ctx)
        {
            var msgError = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Comando movido a /",
                Description = $"Para jugar ingresa `/{ctx.Command.Name}` en vez de `{ctx.Prefix}{ctx.Command.Name}`.",
                Footer = GetFooter(ctx),
                Color = DiscordColor.Red,
            });
            await Task.Delay(10000);
            await BorrarMensaje(ctx, msgError.Id);
        }

        public async Task<Imagen> GetImagenDiscordYumiko(CommandContext ctx, ulong idChannel)
        {
            DiscordGuild discordOOC = await ctx.Client.GetGuildAsync(713809173573271613);
            if (discordOOC == null)
            {
                await ctx.Channel.SendMessageAsync("Error al obtener servidor").ConfigureAwait(false);
                return null;
            }
            DiscordChannel channel = discordOOC.GetChannel(idChannel);
            if (channel == null)
            {
                await ctx.Channel.SendMessageAsync("Error al obtener canal del servidor").ConfigureAwait(false);
                return null;
            }
            IReadOnlyList<DiscordMessage> mensajes = await channel.GetMessagesAsync();
            List<DiscordMessage> msgs = mensajes.ToList();
            int cntMensajes = msgs.Count;
            DiscordMessage last = msgs.LastOrDefault();
            while (cntMensajes == 100)
            {
                var mensajesAux = await channel.GetMessagesBeforeAsync(last.Id);

                cntMensajes = mensajesAux.Count;
                last = mensajesAux.LastOrDefault();

                foreach (DiscordMessage mensaje in mensajesAux)
                {
                    msgs.Add(mensaje);
                }
            }
            List<Imagen> opciones = new();
            foreach (DiscordMessage msg in msgs)
            {
                var att = msg.Attachments[0];
                if (att != null && att.Url != null)
                {
                    opciones.Add(new Imagen
                    {
                        Url = att.Url,
                        Autor = msg.Author
                    });
                }
            }
            int rnd = GetNumeroRandom(0, opciones.Count - 1);
            return opciones[rnd];
        }

        public int GetNumeroRandom(int min, int max)
        {
            if (min <= 0 && max <= 0)
                return 0;
            Random rnd = new();
            return rnd.Next(minValue: min, maxValue: max);
        }

        public string NormalizarField(string s)
        {
            if (s.Length > 1024)
            {
                string aux = s.Remove(1024);
                int index = aux.LastIndexOf('[');
                if (index != -1)
                    return aux.Remove(aux.LastIndexOf('[')) + "...";
                else
                    return aux.Remove(aux.Length - 4) + " ...";
            }
            return s;
        }

        public string NormalizarDescription(string s)
        {
            if (s.Length > 4096)
            {
                string aux = s.Remove(4096);
                int index = aux.LastIndexOf('[');
                if(index != -1)
                    return aux.Remove(aux.LastIndexOf('[')) + "...";
                else
                    return aux.Remove(aux.Length-4) + " ...";
            }
            return s;
        }

        public string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }
        
        public EmbedFooter GetFooter(CommandContext ctx) => new()
        {
            Text = $"Invocado por {ctx.Member.DisplayName} ({ctx.Member.Username}#{ctx.Member.Discriminator}) | {ctx.Prefix}{ctx.Command.Name}",
            IconUrl = ctx.Member.AvatarUrl
        };

        public EmbedFooter GetFooter(InteractionContext ctx) => new()
        {
            Text = $"Invocado por {ctx.Member.DisplayName} ({ctx.Member.Username}#{ctx.Member.Discriminator})",
            IconUrl = ctx.Member.AvatarUrl
        };

        public EmbedFooter GetFooter(Context ctx) => new()
        {
            Text = $"Invocado por {ctx.Member.DisplayName} ({ctx.Member.Username}#{ctx.Member.Discriminator})",
            IconUrl = ctx.Member.AvatarUrl
        };

        public EmbedAuthor GetAuthor(string nombre, string avatar, string url)
        {
            return new EmbedAuthor()
            {
                IconUrl = avatar,
                Name = nombre,
                Url = url
            };
        }

        public DiscordColor GetColor() => DiscordColor.Blurple;

        public string QuitarCaracteresEspeciales(string str)
        {
            if(str != null)
                return Regex.Replace(str, @"[^a-zA-Z0-9]+", " ").Trim();
            return null;
        }

        public string LimpiarTexto(string texto)
        {
            if (texto != null)
            {
                texto = texto.Replace("<br>", "");
                texto = texto.Replace("<Br>", "");
                texto = texto.Replace("<bR>", "");
                texto = texto.Replace("<BR>", "");
                texto = texto.Replace("<i>", "*");
                texto = texto.Replace("<I>", "*");
                texto = texto.Replace("</i>", "*");
                texto = texto.Replace("</I>", "*");
                texto = texto.Replace("~!", "||");
                texto = texto.Replace("!~", "||");
                texto = texto.Replace("__", "**");
            }
            else
            {
                texto = string.Empty;
            }
            return texto;
        }

        public Stream CrearArchivo(AnimeLinks links)
        {
            string path = $@"c:\temp\descargaLinks.txt";
            using (FileStream fs = File.Create(path))
            {
                string linksList = $"Links de descarga para {links.name}\n\n";
                var hosts = links.hosts;
                foreach(var host in hosts)
                {
                    linksList += $"Servidor: {host.name}\n";
                    var linkList = host.links;
                    foreach(var l in linkList)
                    {
                        linksList += $"{l.number} - {l.href}\n";
                    }
                    linksList += "\n";
                }
                byte[] info = new UTF8Encoding(true).GetBytes(linksList);
                fs.Write(info, 0, info.Length);
            }
            return File.OpenRead(path);
        }

        public bool ChequearPermisoYumiko(CommandContext ctx, Permissions permiso)
        {
            return PermissionMethods.HasPermission(ctx.Channel.PermissionsFor(ctx.Guild.CurrentMember), permiso);
        }

        public bool ChequearPermisoYumiko(Context ctx, Permissions permiso)
        {
            return PermissionMethods.HasPermission(ctx.Channel.PermissionsFor(ctx.Guild.CurrentMember), permiso);
        }

        public async Task BorrarMensaje(CommandContext ctx, ulong msgId)
        {
            if(ChequearPermisoYumiko(ctx, Permissions.ManageMessages))
            {
                try
                {
                    var mensaje = await ctx.Channel.GetMessageAsync(msgId);
                    if (mensaje != null)
                    {
                        await mensaje.DeleteAsync("Auto borrado de Yumiko");
                    }
                }
                catch (Exception){ }
            }
        }

        public async Task BorrarMensaje(Context ctx, ulong msgId)
        {
            if (ChequearPermisoYumiko(ctx, Permissions.ManageMessages))
            {
                try
                {
                    var mensaje = await ctx.Channel.GetMessageAsync(msgId);
                    if (mensaje != null)
                    {
                        await mensaje.DeleteAsync("Auto borrado de Yumiko");
                    }
                }
                catch (Exception) { }
            }
        }

        public async Task ChequearVotoTopGG(Context ctx)
        {
            IDebuggingService mode = new DebuggingService();
            bool debug = mode.RunningInDebugMode();
            if (!debug)
            {
                if (GetNumeroRandom(1, 10) == 1)
                {
                    /*
                    var json = string.Empty;
                    using (var fs = File.OpenRead("config.json"))
                    {
                        using var sr = new StreamReader(fs, new UTF8Encoding(false));
                        json = await sr.ReadToEndAsync().ConfigureAwait(false);
                    }
                    var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

                    AuthDiscordBotListApi DblApi = new AuthDiscordBotListApi(ctx.Client.CurrentUser.Id, configJson.TopGG_token);
                    bool voto = await DblApi.HasVoted(ctx.User.Id).ConfigureAwait(false);
                    */
                    bool voto = true;

                    if (voto)
                    {
                        string url = "https://top.gg/bot/295182825521545218/vote";
                        var mensaje = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder()
                        {
                            Title = $"¡Votame en Top.gg!",
                            Description = $"Puedes ayudarme votando en [este sitio web]({url}). ¡Gracias!",
                            Color = GetColor()
                        }).ConfigureAwait(false);
                        await Task.Delay(10000);
                        await BorrarMensaje(ctx, mensaje.Id);
                    }
                }
            }
        }

        public async Task UpdateStatsTopGG(DiscordClient c)
        {
            var json = string.Empty;
            using (var fs = File.OpenRead("config.json"))
            {
                using var sr = new StreamReader(fs, new UTF8Encoding(false));
                json = await sr.ReadToEndAsync().ConfigureAwait(false);
            }
            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            AuthDiscordBotListApi DblApi = new(c.CurrentUser.Id, configJson.TopGG_token);
            await DblApi.UpdateStats(guildCount: c.Guilds.Count);
        }

        public async Task<int> GetElegido(Context ctx, List<string> opciones)
        {
            int cantidadOpciones = opciones.Count;
            if (cantidadOpciones == 1)
                return 1;
            if (cantidadOpciones > 1)
            {
                var interactivity = ctx.Client.GetInteractivity();

                DiscordMessageBuilder mensajeRondas = new()
                {
                    Embed = new DiscordEmbedBuilder
                    {
                        Footer = GetFooter(ctx),
                        Color = GetColor(),
                        Title = "Elije la opcion",
                    }
                };
                List<DiscordComponent> componentes = new();
                int i = 0;
                foreach(var opc in opciones)
                {
                    i++;
                    DiscordButtonComponent button = new(ButtonStyle.Primary, $"{i}", $"{opc}");
                    componentes.Add(button);
                }

                mensajeRondas.AddComponents(componentes);

                DiscordMessage elegirMsg = await mensajeRondas.SendAsync(ctx.Channel);
                var msgElegirInter = await interactivity.WaitForButtonAsync(elegirMsg, ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGeneral"])));

                if (!msgElegirInter.TimedOut)
                {
                    var resultElegir = msgElegirInter.Result;
                    await Task.Delay(3000);
                    await BorrarMensaje(ctx, elegirMsg.Id);
                    return int.Parse(resultElegir.Id);
                }
                else
                {
                    var msg = await ctx.Channel.SendMessageAsync($"Tiempo agotado esperando la opción").ConfigureAwait(false);
                    await Task.Delay(3000);
                    await BorrarMensaje(ctx, msg.Id);
                    await BorrarMensaje(ctx, elegirMsg.Id);
                }
            }
            return -1;
        }

        public async Task<bool> GetSiNoInteractivity(Context ctx, InteractivityExtension interactivity, string titulo, string descripcion)
        {
            DiscordButtonComponent buttonSi = new(ButtonStyle.Success, "true", "Si");
            DiscordButtonComponent buttonNo = new(ButtonStyle.Danger, "false", "No");

            DiscordMessageBuilder mensajeRondas = new()
            {
                Embed = new DiscordEmbedBuilder
                {
                    Title = titulo,
                    Description = descripcion
                }
            };

            mensajeRondas.AddComponents(buttonSi, buttonNo);

            DiscordMessage msgElegir = await mensajeRondas.SendAsync(ctx.Channel);
            var msgElegirInter = await interactivity.WaitForButtonAsync(msgElegir, ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGeneral"])));
            await BorrarMensaje(ctx, msgElegir.Id);
            if (!msgElegirInter.TimedOut)
            {
                return bool.Parse(msgElegirInter.Result.Id);
            }
            else
            {
                return false;
            }
        }


        public async Task<Character> GetRandomCharacter(Context ctx, int pag)
        {
            string titleMedia = "", siteUrlMedia = string.Empty;
            string query = "query($pagina: Int){" +
                        "   Page(page: $pagina, perPage: 1){" +
                        "       characters(sort: FAVOURITES_DESC){" +
                        "           name{" +
                        "               full" +
                        "           }," +
                        "           image{" +
                        "               large" +
                        "           }" +
                        "           siteUrl," +
                        "           favourites," +
                        "           media(sort: POPULARITY_DESC, perPage: 1){" +
                        "               nodes{" +
                        "                   title{" +
                        "                       romaji" +
                        "                   }," +
                        "                   siteUrl" +
                        "               }" +
                        "           }" +
                        "       }" +
                        "   }" +
                        "}";
            var request = new GraphQLRequest
            {
                Query = query,
                Variables = new
                {
                    pagina = pag
                }
            };
            try
            {
                var data = await graphQLClient.SendQueryAsync<dynamic>(request);
                foreach (var x in data.Data.Page.characters)
                {
                    string name = x.name.full;
                    string imageUrl = x.image.large;
                    string siteUrl = x.siteUrl;
                    int favoritos = x.favourites;
                    foreach (var m in x.media.nodes)
                    {
                        titleMedia = m.title.romaji;
                        siteUrlMedia = m.siteUrl;
                    }
                    return new Character()
                    {
                        NameFull = name,
                        Image = imageUrl,
                        SiteUrl = siteUrl,
                        Favoritos = favoritos,
                        AnimePrincipal = new Anime()
                        {
                            TitleRomaji = titleMedia,
                            SiteUrl = siteUrlMedia
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                await GrabarLogError(ctx, $"Error inesperado en  GetRandomCharacter");
                DiscordMessage msg = ex.Message switch
                {
                    _ => await ctx.Channel.SendMessageAsync($"Error inesperado: {ex.Message}").ConfigureAwait(false),
                };
                await Task.Delay(3000);
                await BorrarMensaje(ctx, msg.Id);
                return null;
            }
            return null;
        }

        public async Task<Anime> GetRandomMedia(Context ctx, int pag, string tipo)
        {
            string query = "query($pagina: Int){" +
                        "   Page(page: $pagina, perPage: 1){" +
                        "       media(sort: FAVOURITES_DESC, isAdult: false, type:" + tipo.ToUpper() + "){" +
                        "           title{" +
                        "               romaji," +
                        "               english" +
                        "           }," +
                        "           coverImage{" +
                        "               large" +
                        "           }," +
                        "           siteUrl," +
                        "           favourites" +
                        "       }" +
                        "   }" +
                        "}";
            var request = new GraphQLRequest
            {
                Query = query,
                Variables = new
                {
                    pagina = pag
                }
            };
            try
            {
                var data = await graphQLClient.SendQueryAsync<dynamic>(request);
                foreach (var x in data.Data.Page.media)
                {
                    string titleRomaji = x.title.romaji;
                    string titleEnglish = x.title.english;
                    string imageUrl = x.coverImage.large;
                    string siteUrl = x.siteUrl;
                    int favoritos = x.favourites;
                    return new Anime()
                    {
                        TitleRomaji = titleRomaji,
                        TitleEnglish = titleEnglish,
                        Image = imageUrl,
                        SiteUrl = siteUrl,
                        Favoritos = favoritos,
                    };
                }
            }
            catch (Exception ex)
            {
                await GrabarLogError(ctx, $"Error inesperado en  GetRandomMedia");
                DiscordMessage msg = ex.Message switch
                {
                    _ => await ctx.Channel.SendMessageAsync($"Error inesperado: {ex.Message}").ConfigureAwait(false),
                };
                await Task.Delay(3000);
                await BorrarMensaje(ctx, msg.Id);
                return null;
            }
            return null;
        }

        public async Task GrabarLogError(CommandContext ctx, string descripcion)
        {
            var Guild = await ctx.Client.GetGuildAsync(713809173573271613);
            if(Guild != null)
            {
                var ChannelErrores = Guild.GetChannel(840440877565739008);
                if(ChannelErrores != null)
                {
                    await ChannelErrores.SendMessageAsync(new DiscordEmbedBuilder { 
                        Title = "Error no controlado",
                        Description = descripcion,
                        Color = DiscordColor.Red,
                        Footer = GetFooter(ctx),
                        Author= new EmbedAuthor
                        {
                            IconUrl = ctx.Guild.IconUrl,
                            Name = ctx.Guild.Name
                        },
                    }.AddField("Id Servidor", $"{ctx.Guild.Id}", true)
                    .AddField("Id Canal", $"{ctx.Channel.Id}", true)
                    .AddField("Canal", $"#{ctx.Channel.Name}", false)
                    .AddField("Mensaje", $"{ctx.Message.Content}", false));
                }
            }
        }

        public async Task GrabarLogError(Context ctx, string descripcion)
        {
            var Guild = await ctx.Client.GetGuildAsync(713809173573271613);
            if (Guild != null)
            {
                var ChannelErrores = Guild.GetChannel(840440877565739008);
                if (ChannelErrores != null)
                {
                    await ChannelErrores.SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Title = "Error no controlado",
                        Description = descripcion,
                        Color = DiscordColor.Red,
                        Footer = GetFooter(ctx),
                        Author = new EmbedAuthor
                        {
                            IconUrl = ctx.Guild.IconUrl,
                            Name = ctx.Guild.Name
                        },
                    }.AddField("Id Servidor", $"{ctx.Guild.Id}", true)
                    .AddField("Id Canal", $"{ctx.Channel.Id}", true)
                    .AddField("Canal", $"#{ctx.Channel.Name}", false)
                    .AddField("Mensaje", $"{ctx.Message.Content}", false));
                }
            }
        }

        public async Task<string> GetStringInteractivity(Context ctx, string tituloBusqueda, string descBusqueda, string descError, int timeoutSegundos)
        {
            var interactivity = ctx.Client.GetInteractivity();
            var msgUsuario = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = tituloBusqueda,
                Description = descBusqueda,
                Footer = GetFooter(ctx),
                Color = GetColor(),
            });
            var msgUserInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(timeoutSegundos));
            if (!msgUserInter.TimedOut)
            {
                if (msgUsuario != null)
                    await BorrarMensaje(ctx, msgUsuario.Id);
                if (msgUserInter.Result != null)
                    await BorrarMensaje(ctx, msgUserInter.Result.Id);
                return msgUserInter.Result.Content;
            }
            else
            {
                var msgError = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Description = descError,
                    Footer = GetFooter(ctx),
                    Color = DiscordColor.Red,
                });
                await Task.Delay(3000);
                if (msgError != null)
                    await BorrarMensaje(ctx, msgError.Id);
                if (msgUsuario != null)
                    await BorrarMensaje(ctx, msgUsuario.Id);
                return string.Empty;
            }
        }

        public async Task<DiscordMessage> GetInfoComando(CommandContext ctx, Command comando)
        {
            string web = ConfigurationManager.AppSettings["Web"] + "#commands";
            string nomComando = comando.Name;
            var builder = new DiscordEmbedBuilder
            {
                Title = $"Comando {nomComando}",
                Url = web,
                Footer = GetFooter(ctx),
                Color = GetColor()
            };
            string modulo = comando.Module.ModuleType.Name;
            var aliases = comando.Aliases;
            string descripcion = comando.Description;
            if (modulo != null)
                builder.AddField("Modulo", modulo, false);
            if (aliases.Count > 0)
            {
                List<string> listaAliases = new();
                foreach (string a in aliases)
                {
                    listaAliases.Add($"`{a}`");
                }
                string aliasesC = string.Join(", ", listaAliases);
                builder.AddField("Aliases", aliasesC, false);
            }
            if (descripcion != null)
                builder.AddField("Descripcion", descripcion, false);
            foreach (var overload in comando.Overloads)
            {
                string parametros = string.Empty;
                foreach (var argument in overload.Arguments)
                {
                    if (argument.Description != null)
                    {
                        if (argument.IsOptional)
                            parametros += $":arrow_right: **{argument.Name}** | {argument.Description} | Obligatorio: **No**\n";
                        else
                            parametros += $":arrow_right: **{argument.Name}** | {argument.Description} | Obligatorio: **Si**\n";
                    }
                    else
                    {
                        if (argument.IsOptional)
                            parametros += $":arrow_right: **{argument.Name}** | Obligatorio: **No**\n";
                        else
                            parametros += $":arrow_right: **{argument.Name}** | Obligatorio: **Si**\n";
                    }
                }
                if (!string.IsNullOrEmpty(parametros))
                    builder.AddField("Parametros", parametros, false);
            }
            return await ctx.Channel.SendMessageAsync(embed: builder).ConfigureAwait(false);
        }

        public async Task<List<Tag>> GetTags(Context ctx)
        {
            List<Tag> lista = new();
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
                foreach (var x in data.Data.MediaTagCollection)
                {
                    if ((x.isAdult == "false") || (x.isAdult == true && ctx.Channel.IsNSFW))
                    {
                        string nombre = x.name;
                        string descripcion = x.description;
                        lista.Add(new Tag()
                        {
                            Nombre = nombre,
                            Descripcion = descripcion
                        });
                    }
                }
            }
            catch{  }
            return lista;
        }

        public DiscordEmbedBuilder Pregunta(CommandContext ctx, string pregunta)
        {
            Random rnd = new();
            int random = rnd.Next(2);
            return random switch
            {
                0 => new DiscordEmbedBuilder
                {
                    Footer = GetFooter(ctx),
                    Color = DiscordColor.Red,
                    Title = "¿SI O NO?",
                    Description = "**Pregunta:** " + pregunta + "\n**Respuesta:** NO"
                },
                _ => new DiscordEmbedBuilder
                {
                    Footer = GetFooter(ctx),
                    Color = DiscordColor.Green,
                    Title = "¿SI O NO?",
                    Description = "**Pregunta:** " + pregunta + "\n**Respuesta:** SI"
                },
            };
        }

        public DiscordEmbedBuilder Pregunta(string pregunta)
        {
            Random rnd = new();
            int random = rnd.Next(2);
            return random switch
            {
                0 => new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Red,
                    Title = "¿SI O NO?",
                    Description = "**Pregunta:** " + pregunta + "\n**Respuesta:** NO"
                },
                _ => new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Green,
                    Title = "¿SI O NO?",
                    Description = "**Pregunta:** " + pregunta + "\n**Respuesta:** SI"
                },
            };
        }
    }
}
