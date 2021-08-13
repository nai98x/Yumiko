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

        public string GraphQLParse(string queryType, string queryName, string[] subSelection, object @object = null, string objectTypeName = null)
        {
            var query = queryType + "{" + queryName;

            if (@object != null)
            {
                query += "(";

                if (objectTypeName != null)
                {
                    query += objectTypeName + ":" + "{";
                }

                var queryData = string.Empty;
                foreach (var propertyInfo in @object.GetType().GetProperties())
                {
                    var value = propertyInfo.GetValue(@object);
                    if (value != null)
                    {
                        var type = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                        var valueQuotes = type == typeof(string) ? "\"" : string.Empty;

                        var queryPart = char.ToLowerInvariant(propertyInfo.Name[0]) + propertyInfo.Name.Substring(1) + ":" + valueQuotes + value + valueQuotes;
                        queryData += queryData.Length > 0 ? "," + queryPart : queryPart;
                    }
                }
                query += (objectTypeName != null ? queryData + "}" : queryData) + ")";
            }

            if (subSelection.Length > 0)
            {
                query += subSelection.Aggregate("{", (current, s) => current + (current.Length > 1 ? "," + s : s)) + "}";
            }

            query += "}";

            return query;
        }

        public async Task<Imagen> GetImagenDiscordYumiko(CommandContext ctx, ulong idChannel)
        {
            DiscordGuild discordOOC = await ctx.Client.GetGuildAsync(713809173573271613);
            if (discordOOC == null)
            {
                await ctx.Channel.SendMessageAsync("Failed to get server").ConfigureAwait(false);
                return null;
            }
            DiscordChannel channel = discordOOC.GetChannel(idChannel);
            if (channel == null)
            {
                await ctx.Channel.SendMessageAsync("Failed to get channel").ConfigureAwait(false);
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

        public string NormalizarBoton(string s)
        {
            if (s.Length > 80)
            {
                return s.Remove(76) + " ...";
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
            Text = $"Executed by {ctx.Member.DisplayName} ({ctx.Member.Username}#{ctx.Member.Discriminator}) | {ctx.Prefix}{ctx.Command.Name}",
            IconUrl = ctx.Member.AvatarUrl
        };

        public EmbedFooter GetFooter(InteractionContext ctx) => new()
        {
            Text = $"Executed by {ctx.Member.DisplayName} ({ctx.Member.Username}#{ctx.Member.Discriminator})",
            IconUrl = ctx.Member.AvatarUrl
        };

        public EmbedFooter GetFooter(Context ctx) => new()
        {
            Text = $"Executed by {ctx.Member.DisplayName} ({ctx.Member.Username}#{ctx.Member.Discriminator})",
            IconUrl = ctx.Member.AvatarUrl
        };

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
                texto = texto.Replace("<b>", "**");
                texto = texto.Replace("<B>", "**");
                texto = texto.Replace("</b>", "**");
                texto = texto.Replace("</B>", "**");
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
                string linksList = $"Download links for {links.name} (Spanish)\n\n";
                var hosts = links.hosts;
                foreach(var host in hosts)
                {
                    linksList += $"Server: {host.name}\n";
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
                        await mensaje.DeleteAsync("Yumiko's auto erase");
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
                        await mensaje.DeleteAsync("Yumiko's auto erase");
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
                            Title = $"Vote me on Top.gg!",
                            Description = $"You can help me by voting me in this [website]({url}). Thanks!",
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

        public async Task<int> GetElegido(InteractionContext ctx, List<string> opciones)
        {
            int cantidadOpciones = opciones.Count;
            if (cantidadOpciones == 1)
                return 1;
            else
            {
                var interactivity = ctx.Client.GetInteractivity();

                List<DiscordComponent> componentes = new();
                int i = 0;
                foreach(var opc in opciones)
                {
                    if (i > 5)
                    {
                        break;
                    }
                    var aux = NormalizarBoton(opc);
                    i++;
                    DiscordButtonComponent button = new(ButtonStyle.Primary, $"{i}", $"{aux}");
                    componentes.Add(button);
                }

                var embed = new DiscordEmbedBuilder
                {
                    Footer = GetFooter(ctx),
                    Color = GetColor(),
                    Title = "Choose the option",
                };
                DiscordMessage elegirMsg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddComponents(componentes).AddEmbed(embed));

                var msgElegirInter = await interactivity.WaitForButtonAsync(elegirMsg, ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGeneral"])));

                if (!msgElegirInter.TimedOut)
                {
                    var resultElegir = msgElegirInter.Result;
                    return int.Parse(resultElegir.Id);
                }
            }
            return -1;
        }

        public async Task<bool> GetSiNoInteractivity(Context ctx, InteractivityExtension interactivity, string titulo, string descripcion)
        {
            DiscordButtonComponent buttonSi = new(ButtonStyle.Success, "true", "Yes");
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
                await GrabarLogError(ctx, $"Unknow error in GetRandomCharacter");
                DiscordMessage msg = ex.Message switch
                {
                    _ => await ctx.Channel.SendMessageAsync($"Unknow error: {ex.Message}").ConfigureAwait(false),
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
                await GrabarLogError(ctx, $"Unknow error in GetRandomMedia");
                DiscordMessage msg = ex.Message switch
                {
                    _ => await ctx.Channel.SendMessageAsync($"Unknow error: {ex.Message}").ConfigureAwait(false),
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
                    }.AddField("Guild Id", $"{ctx.Guild.Id}", true)
                    .AddField("Channel Id", $"{ctx.Channel.Id}", true)
                    .AddField("Channel", $"#{ctx.Channel.Name}", false)
                    .AddField("Message", $"{ctx.Message.Content}", false));
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
                    }.AddField("Guild Id", $"{ctx.Guild.Id}", true)
                    .AddField("Channel Id", $"{ctx.Channel.Id}", true)
                    .AddField("Channel", $"#{ctx.Channel.Name}", false)
                    );
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
                Title = $"Command {nomComando}",
                Url = web,
                Footer = GetFooter(ctx),
                Color = GetColor()
            };
            string modulo = comando.Module.ModuleType.Name;
            var aliases = comando.Aliases;
            string descripcion = comando.Description;
            if (modulo != null)
                builder.AddField("Module", modulo, false);
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
                builder.AddField("Description", descripcion, false);
            foreach (var overload in comando.Overloads)
            {
                string parametros = string.Empty;
                foreach (var argument in overload.Arguments)
                {
                    if (argument.Description != null)
                    {
                        if (argument.IsOptional)
                            parametros += $":arrow_right: **{argument.Name}** | {argument.Description} | Mandatory: **No**\n";
                        else
                            parametros += $":arrow_right: **{argument.Name}** | {argument.Description} | Mandatory: **Ýes**\n";
                    }
                    else
                    {
                        if (argument.IsOptional)
                            parametros += $":arrow_right: **{argument.Name}** | Mandatory: **No**\n";
                        else
                            parametros += $":arrow_right: **{argument.Name}** | Mandatory: **Yes**\n";
                    }
                }
                if (!string.IsNullOrEmpty(parametros))
                    builder.AddField("Parameters", parametros, false);
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

        public DiscordEmbedBuilder Pregunta(string pregunta)
        {
            Random rnd = new();
            int random = rnd.Next(2);
            return random switch
            {
                0 => new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Red,
                    Title = "YES OR NO?",
                    Description = "**Question:** " + pregunta + "\n**Answer:** NO"
                },
                _ => new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Green,
                    Title = "YES OR NO?",
                    Description = "**Question:** " + pregunta + "\n**Answer:** SI"
                },
            };
        }

        public DiscordEmbedBuilder Waifu(DiscordMember miembro)
        {
            string nombre;
            nombre = miembro.DisplayName;

            int waifuLevel = GetNumeroRandom(0, 100);
            if (waifuLevel < 25)
            {
                return new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Red,
                    Title = "Waifu level",
                    Description = "My love to **" + nombre + "** is **" + waifuLevel + "%**\nI would rather shoot myself than touch you.",
                    ImageUrl = "https://i.imgur.com/BOxbruw.png"
                };
            }
            else if (waifuLevel >= 25 && waifuLevel < 50)
            {
                return new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Orange,
                    Title = "Waifu level",
                    Description = "My love to **" + nombre + "** is **" + waifuLevel + "%**\nYou disgust me, I better get away from you.",
                    ImageUrl = "https://i.imgur.com/ys2HoiL.jpg"
                };
            }
            else if (waifuLevel >= 50 && waifuLevel < 75)
            {
                return new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Yellow,
                    Title = "Waifu level",
                    Description = "My love to **" + nombre + "** is **" + waifuLevel + "%**\nYou're not bad, maybe you have a chance with me.",
                    ImageUrl = "https://i.imgur.com/h7Ic2rk.jpg"
                };
            }
            else if (waifuLevel >= 75 && waifuLevel < 100)
            {
                return new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Green,
                    Title = "Waifu level",
                    Description = "My love to **" + nombre + "** is **" + waifuLevel + "%**\nI'm your waifu, you can do whatever you want with me.",
                    ImageUrl = "https://i.imgur.com/dhXR8mV.png"
                };
            }
            else // 100
            {
                return new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Blue,
                    Title = "Waifu level",
                    Description = "My love to **" + nombre + "** is **" + waifuLevel + "%**\n.I am completely in love with you, when did we get married?",
                    ImageUrl = "https://i.imgur.com/Vk6JMJi.jpg"
                };
            }
        }
    }
}
