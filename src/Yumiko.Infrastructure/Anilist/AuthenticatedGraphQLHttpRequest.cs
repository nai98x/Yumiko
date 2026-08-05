using System.Net.Http.Headers;
using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;

namespace Yumiko.Infrastructure.Anilist;

/// <summary>
/// GraphQL request that attaches the user OAuth token in the <c>Authorization</c> header.
/// The header is added per request (not on the shared client), so it is safe to use it
/// with the singleton <see cref="GraphQLHttpClient"/> of the executor.
/// </summary>
internal sealed class AuthenticatedGraphQLHttpRequest(GraphQLRequest request, string accessToken) : GraphQLHttpRequest(request)
{
    public override HttpRequestMessage ToHttpRequestMessage(GraphQLHttpClientOptions options, IGraphQLJsonSerializer serializer)
    {
        HttpRequestMessage message = base.ToHttpRequestMessage(options, serializer);
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return message;
    }
}
