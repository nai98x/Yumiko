namespace Yumiko.Infrastructure.Anilist.Responses;

// The probe queries a single Media, so `data.Media` is an object and not the array that the paged
// responses return. The fields do not matter: what is read are the X-RateLimit-* headers.
public class RateLimitProbeResponse
{
    public MediaIdResponse? Media { get; set; }
}

public class MediaIdResponse
{
    public int Id { get; set; }
}
