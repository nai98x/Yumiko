namespace Yumiko.Services
{
    using DSharpPlus.SlashCommands;
    using GraphQL;
    using GraphQL.Client.Http;
    using GraphQL.Client.Serializer.Newtonsoft;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public static class MediaUserQuery
    {
        private static readonly GraphQLHttpClient GraphQlClient = new(Constants.AnilistAPIUrl, new NewtonsoftJsonSerializer());

        public static async Task<MediaUser?> GetMediaFromUser(InteractionContext ctx, int userId, int mediaId)
        {
            try
            {
                var request = new GraphQLRequest
                {
                    Query = searchQuery,
                    Variables = new
                    {
                        userId,
                        mediaId
                    }
                };
                var response = await GraphQlClient.SendQueryAsync<MediaListResponse>(request);
                var result = response.Data.MediaList;

                return result;
            }
            catch (GraphQLHttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound) return null;
                await Common.GrabarLogErrorAsync(ctx, $"Unknown error: {ex.StatusCode}: {ex.Message}\n{Formatter.BlockCode(ex.StackTrace)}");
                return null;
            }
            catch (Exception e)
            {
                await Common.GrabarLogErrorAsync(ctx, $"Unknown error: {e.Message}\n{Formatter.BlockCode(e.StackTrace)}");
                return null;
            }
        }

        public const string searchQuery = @"
            query ($userId: Int, $mediaId: Int){
                MediaList(userId: $userId, mediaId: $mediaId) {
                    status
                    progress,
                    startedAt {
                        year
                        month
                        day
                    }
                    completedAt {
                        year
                        month
                        day
                    }
                    notes
                    score
                    repeat
                    media {
                        episodes
                        chapters
                    }
                    user {
                        name
                        avatar {
                            medium
                        }
                        mediaListOptions {
                            scoreFormat
                        }
                    }
                }
            }
        ";
    }
}
