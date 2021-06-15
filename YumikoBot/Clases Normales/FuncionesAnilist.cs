using DSharpPlus.CommandsNext;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using System;
using System.Threading.Tasks;

namespace Discord_Bot
{
    public class FuncionesAnilist
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();
        private readonly GraphQLHttpClient graphQLClient = new GraphQLHttpClient("https://graphql.anilist.co", new NewtonsoftJsonSerializer());

        public async Task<dynamic> GetAnilistMedia(CommandContext ctx, string busqueda, string tipo)
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
                    busqueda = busqueda
                }
            };
            try
            {
                var data = await graphQLClient.SendQueryAsync<dynamic>(request);
                if (data.Data != null)
                {
                    //return data;
                }
                return data;
            }
            catch(Exception e)
            {
                await funciones.GrabarLogError(ctx, $"Error en query en FuncionesAnilist - GetAnilistMedia, utilizado: {tipo}\nError: {e.Message}");
            }
            return null;
        }

        public Media DecodeMedia(dynamic datos)
        {
            if(datos != null)
            {
                Media media = new Media();

                media.IsAdult = datos.isAdult;
                media.Descripcion = datos.description;
                media.Descripcion = funciones.NormalizarDescription(funciones.LimpiarTexto(media.Descripcion));
                if (media.Descripcion == "")
                    media.Descripcion = "(Sin descripción)";
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
                media.Titulos = string.Empty;
                foreach (var title in datos.synonyms)
                {
                    media.Titulos += $"`{title}`, ";
                }
                if (media.Titulos.Length >= 2)
                    media.Titulos = media.Titulos.Remove(media.Titulos.Length - 2);
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
                        media.Fechas = $"En emisión desde {datos.startDate.day}/{datos.startDate.month}/{datos.startDate.year}";
                }
                else
                {
                    media.Fechas = $"Este anime no tiene fecha de emisión";
                }
                media.Titulo = datos.title.romaji;
                media.UrlAnilist = datos.siteUrl;

                return media;
            }
            else
            {
                return null;
            }
        }
    }
}
