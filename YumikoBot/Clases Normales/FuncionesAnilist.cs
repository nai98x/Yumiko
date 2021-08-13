using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot
{
    public class FuncionesAnilist
    {
        private readonly FuncionesAuxiliares funciones = new();
        private readonly GraphQLHttpClient graphQLClient = new("https://graphql.anilist.co", new NewtonsoftJsonSerializer());

        public async Task<Media> GetAniListMedia(InteractionContext ctx, string busqueda, string tipo)
        {
            string query = "query($busqueda : String){" +
            "   Page(perPage:5){" +
            "       media(type: " + tipo.ToUpper() + ", search: $busqueda){" +
            "           title{" +
            "               romaji" +
            "           }," +
            "           coverImage{" +
            "               large" +
            "           }," +
            "           siteUrl," +
            "           description," +
            "           format," +
            "           chapters" +
            "           episodes" +
            "           status," +
            "           meanScore," +
            "           genres," +
            "           startDate{" +
            "               year," +
            "               month," +
            "               day" +
            "           }," +
            "           endDate{" +
            "               year," +
            "               month," +
            "               day" +
            "           }," +
            "           genres," +
            "           tags{" +
            "               name," +
            "               isMediaSpoiler" +
            "           }," +
            "           synonyms," +
            "           studios{" +
            "               nodes{" +
            "                   name," +
            "                   siteUrl" +
            "               }" +
            "           }," +
            "           externalLinks{" +
            "               site," +
            "               url" +
            "           }," +
            "           isAdult" +
            "       }" +
            "   }" +
            "}";

            var request = new GraphQLRequest
            {
                Query = query,
                Variables = new
                {
                    busqueda
                }
            };

            try
            {
                var data = await graphQLClient.SendQueryAsync<dynamic>(request);
                if (data.Data != null && data.Data.Page.media != null)
                {
                    int cont = 0;
                    List<string> opc = new();
                    foreach (var animeP in data.Data.Page.media)
                    {
                        cont++;
                        string opcStr = animeP.title.romaji;
                        opc.Add(opcStr);
                    }
                    var elegido = await funciones.GetElegido(ctx, opc);
                    if (elegido > 0)
                    {
                        var datos = data.Data.Page.media[elegido - 1];
                        return DecodeMedia(datos);
                    }
                    else
                    {
                        return new()
                        {
                            Ok = false,
                            MsgError = $"Time out waiting for the option"
                        };
                    } 
                }
                else
                {
                    return new()
                    {
                        Ok = false,
                        MsgError = $"{tipo} not found with `{busqueda}`"
                    };
                }
            }
            catch (Exception e)
            {
                var context = funciones.GetContext(ctx);
                await funciones.GrabarLogError(context, $"Error in query in FuncionesAnilist - GetAnilistMedia, used: {tipo}\nError: {e.Message}");
                return new()
                {
                    Ok = false,
                    MsgError = $"{e.Message}"
                };
            }
        }

        public async Task<Media> GetAniListCharacter(InteractionContext ctx, string busqueda, string tipo)
        {
            var request = new GraphQLRequest
            {
                Query =
                "query($nombre : String){" +
                "   Page(perPage:5){" +
                "       characters(search: $nombre){" +
                "           name{" +
                "               full" +
                "           }," +
                "           image{" +
                "               large" +
                "           }," +
                "           siteUrl," +
                "           description," +
                "           animes: media(type: ANIME){" +
                "               nodes{" +
                "                   title{" +
                "                       romaji" +
                "                   }," +
                "                   siteUrl" +
                "               }" +
                "           }" +
                "           mangas: media(type: MANGA){" +
                "               nodes{" +
                "                   title{" +
                "                       romaji" +
                "                   }," +
                "                   siteUrl" +
                "               }" +
                "           }" +
                "       }" +
                "   }" +
                "}",
                Variables = new
                {
                    nombre = busqueda
                }
            };

            try
            {
                var data = await graphQLClient.SendQueryAsync<dynamic>(request);
                if (data.Data != null && data.Data.Page.media != null)
                {
                    int cont = 0;
                    List<string> opc = new();
                    foreach (var animeP in data.Data.Page.media)
                    {
                        cont++;
                        string opcStr = animeP.title.romaji;
                        opc.Add(opcStr);
                    }
                    var elegido = await funciones.GetElegido(ctx, opc);
                    if (elegido > 0)
                    {
                        var datos = data.Data.Page.characters[elegido - 1];
                        return DecodeCharacter(datos);
                    }
                    else
                    {
                        return new()
                        {
                            Ok = false,
                            MsgError = $"Time out waiting for the option"
                        };
                    }
                }
                else
                {
                    return new()
                    {
                        Ok = false,
                        MsgError = $"{tipo} not found with `{busqueda}`"
                    };
                }
            }
            catch (Exception e)
            {
                var context = funciones.GetContext(ctx);
                await funciones.GrabarLogError(context, $"Error in query in FuncionesAnilist - GetAnilistMedia, used: {tipo}\nError: {e.Message}");
                return new()
                {
                    Ok = false,
                    MsgError = $"{e.Message}"
                };
            }
        }

        public Media DecodeMedia(dynamic datos)
        {
            if(datos != null)
            {
                string isadult = datos.isAdult;

                Media media = new();

                media.Ok = true;
                media.IsAdult = bool.Parse(isadult);
                media.Descripcion = datos.description;
                media.Descripcion = funciones.NormalizarDescription(funciones.LimpiarTexto(media.Descripcion));
                if (media.Descripcion == "")
                    media.Descripcion = "(without description)";
                media.Estado = datos.status;
                media.Episodios = datos.episodes;
                media.Formato = datos.format;
                media.Score = $"{datos.meanScore}/100";
                media.Generos = string.Empty;
                foreach (var genero in datos.genres)
                {
                    media.Generos += genero;
                    media.Generos += ", ";
                }
                if (media.Generos.Length >= 2)
                    media.Generos = media.Generos.Remove(media.Generos.Length - 2);
                media.Tags = string.Empty;
                foreach (var tag in datos.tags)
                {
                    if (tag.isMediaSpoiler == "false")
                    {
                        media.Tags += tag.name;
                    }
                    else
                    {
                        media.Tags += $"||{tag.name}||";
                    }
                    media.Tags += ", ";
                }
                if (media.Tags.Length >= 2)
                    media.Tags = media.Tags.Remove(media.Tags.Length - 2);

                media.Titulos = new();
                foreach (string title in datos.synonyms)
                {
                    media.Titulos.Add(title);
                }

                media.Estudios = string.Empty;
                var nodos = datos.studios.nodes;
                if (nodos.HasValues)
                {
                    foreach (var studio in datos.studios.nodes)
                    {
                        media.Estudios += $"[{studio.name}]({studio.siteUrl}), ";
                    }
                }
                if (media.Estudios.Length >= 2)
                    media.Estudios = media.Estudios.Remove(media.Estudios.Length - 2);
                media.LinksExternos = string.Empty;
                foreach (var external in datos.externalLinks)
                {
                    media.LinksExternos += $"[{external.site}]({external.url}), ";
                }
                if (media.LinksExternos.Length >= 2)
                    media.LinksExternos = media.LinksExternos.Remove(media.LinksExternos.Length - 2);
                if (datos.startDate.day != null)
                {
                    if (datos.endDate.day != null)
                        media.Fechas = $"{datos.startDate.day}/{datos.startDate.month}/{datos.startDate.year} al {datos.endDate.day}/{datos.endDate.month}/{datos.endDate.year}";
                    else
                        media.Fechas = $"Airing since {datos.startDate.day}/{datos.startDate.month}/{datos.startDate.year}";
                }
                else
                {
                    media.Fechas = $"This media has no air date";
                }
                media.TituloRomaji = datos.title.romaji;
                media.UrlAnilist = datos.siteUrl;
                media.CoverImage = datos.coverImage.large;

                return media;
            }
            else
            {
                return null;
            }
        }

        public Character DecodeCharacter(dynamic datos)
        {
            if (datos != null)
            {
                Character character = new();

                string descripcion = datos.description;
                character.Description = funciones.NormalizarDescription(funciones.LimpiarTexto(descripcion));
                if (character.Description == "")
                    character.Description = "(without description)";
                character.NameFull = datos.name.full;
                character.Image = datos.image.large;
                character.SiteUrl = datos.siteUrl;
                character.Animes = new();
                foreach (var anime in datos.animes.nodes)
                {
                    character.Animes.Add(new()
                    {
                        //TituloRomaji = anime.title.romaji,
                        //UrlAnilist = anime.siteUrl
                    });
                }
                string mangas = string.Empty;
                foreach (var manga in datos.mangas.nodes)
                {
                    character.Mangas.Add(new()
                    {
                        //TitleRomaji = anime.title.romaji,
                        //SiteUrl = anime.siteUrl
                    });
                    mangas += $"[{manga.title.romaji}]({manga.siteUrl})\n";
                }

                return character;
            }
            else
            {
                return null;
            }
        }
    }
}
