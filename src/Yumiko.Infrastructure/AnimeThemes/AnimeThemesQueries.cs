namespace Yumiko.Infrastructure.AnimeThemes;

internal static class AnimeThemesQueries
{
    internal const string Search = @"
        query ($search: String!, $first: Int) {
            search(search: $search, first: $first) {
                anime {
                    id
                    title {
                        romaji
                        english
                    }
                    slug
                    year
                    seasonLocalized
                    synopsis
                    animethemes {
                        id
                        type
                        sequence
                        slug
                        animethemeentries {
                            id
                            version
                            episodes
                            notes
                            nsfw
                            spoiler
                            videos {
                                nodes {
                                    id
                                    basename
                                    link
                                }
                            }
                        }
                    }
                }
            }
        }
    ";
}
