namespace Yumiko.Services
{
    using GraphQL;
    using GraphQL.Client.Http;
    using GraphQL.Client.Serializer.Newtonsoft;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public static class AnimeListQuery
    {
        private static readonly GraphQLHttpClient GraphQlClient = new(Constants.AnilistAPIUrl, new NewtonsoftJsonSerializer());

        public static async Task<MediaUserList?> GetMediaLists(DiscordGuild guild, DiscordChannel channel, int userId, MediaUserStatus status, MediaUserSort order, MediaTitleType titleLanguage, MediaType type)
        {
            try
            {
                string sort = Enum.GetName(typeof(MediaUserSort), order)!;
                string statusStr = Enum.GetName(typeof(MediaUserStatus), status)!;
                string typeStr = type.GetName();

                if (sort == "MEDIA_TITLE_DESC")
                {
                    sort = titleLanguage switch
                    {
                        MediaTitleType.ROMAJI => "MEDIA_TITLE_ROMAJI",
                        MediaTitleType.ENGLISH => "MEDIA_TITLE_ENGLISH",
                        MediaTitleType.NATIVE => "MEDIA_TITLE_NATIVE",
                        _ => throw new Exception("Missing MediaTitleType"),
                    };
                }

                var request = new GraphQLRequest
                {
                    Query = searchQuery,
                    Variables = new
                    {
                        userId,
                        status = statusStr,
                        sort,
                        type = typeStr
                    }
                };
                var response = await GraphQlClient.SendQueryAsync<MediaUserListResponse>(request);
                var userList = response.Data?.MediaListCollection?.Lists?[0];

                return userList;
            }
            catch (GraphQLHttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound) return null;
                await Common.GrabarLogErrorAsync(guild, channel, $"Unknown error: {ex.StatusCode}: {ex.Message}\n{Formatter.BlockCode(ex.StackTrace)}");
                return null;
            }
            catch (Exception e)
            {
                await Common.GrabarLogErrorAsync(guild, channel, $"Unknown error: {e.Message}\n{Formatter.BlockCode(e.StackTrace)}");
                return null;
            }
        }

        private const string searchQuery = @"
            query ($userId: Int, $type: MediaType, $status: MediaListStatus, $sort: [MediaListSort]) {
                MediaListCollection(userId: $userId, type: $type, status: $status, forceSingleCompletedList: true, sort: $sort) {
                lists {
                    name
                    entries {
                    media {
                        id
                        title {
                        romaji
                        english
                        native
                        }
                        siteUrl
                    }
                    score
                    }
                }
                }
            }
        ";
    }
}
