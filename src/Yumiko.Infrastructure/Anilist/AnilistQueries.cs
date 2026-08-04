namespace Yumiko.Infrastructure.Anilist;

// Cualquier cambio acá altera los campos que llegan a los embeds.
internal static class AnilistQueries
{
    internal const string MediaSearch = @"
            query ($search: String, $type: MediaType, $perPage: Int){
                Page(perPage: $perPage) {
                    media(search: $search, type: $type) {
                        id
                        title {
                            romaji
                            english
                            native
                        }
                        synonyms
                        description
                        siteUrl
                        coverImage {
                            large
                            medium
                        }
                        bannerImage
                        format
                        volumes
                        chapters
                        episodes
                        status
                        meanScore
                        genres
                        seasonYear
                        startDate {
                            year
                            month
                            day
                        }
                        endDate {
                            year
                            month
                            day
                        }
                        tags {
                            name
                            isMediaSpoiler
                        }
                        studios {
                            nodes {
                                name
                                siteUrl
                                isAnimationStudio
                            }
                        }
                        externalLinks {
                            site,
                            url
                        }
                        isAdult
                    }
                }
            }
        ";

    internal const string MediaById = @"
            query ($id: Int, $type: MediaType, $perPage: Int){
                Page(perPage: $perPage) {
                    media(id: $id, type: $type) {
                        id
                        title {
                            romaji
                            english
                            native
                        }
                        synonyms
                        description
                        siteUrl
                        coverImage {
                            large
                            medium
                        }
                        bannerImage
                        format
                        volumes
                        chapters
                        episodes
                        status
                        meanScore
                        genres
                        seasonYear
                        startDate {
                            year
                            month
                            day
                        }
                        endDate {
                            year
                            month
                            day
                        }
                        tags {
                            name
                            isMediaSpoiler
                        }
                        studios {
                            nodes {
                                name
                                siteUrl
                                isAnimationStudio
                            }
                        }
                        externalLinks {
                            site,
                            url
                        }
                        isAdult
                    }
                }
            }
        ";

    internal const string CharacterSearch = @"
            query ($search: String, $perPage: Int){
                Page(perPage: $perPage) {
                    characters(search: $search) {
                        id
                        name {
                            full
                        }
                        image {
                            large
                        }
                        favourites
                        siteUrl
                        description(asHtml: false)
                        animes: media(type: ANIME) {
                            nodes {
                                id
                                title {
                                    romaji
                                    english
                                    native
                                }
                                siteUrl
                            }
                        }
                        mangas: media(type: MANGA) {
                            nodes {
                                id
                                title {
                                    romaji
                                    english
                                    native
                                }
                                siteUrl
                            }
                        }
                    }
                }
            }
        ";

    internal const string StaffSearch = @"
            query ($search: String, $perPage: Int){
                Page(perPage: $perPage) {
                    staff(search: $search) {
                        id
                        name {
                            full
                        }
                        image {
                            large
                        }
                        languageV2
                        description(asHtml: false)
                        siteUrl
                        gender
                        age
                        dateOfBirth {
                            year
                            month
                            day
                        }
                        dateOfDeath {
                            year
                            month
                            day
                        }
                    }
                }
            }
        ";

    internal const string Profile = @"
            query ($code: Int){
                User(id: $code) {
                    id
                    name
                    siteUrl
                    mediaListOptions {
                        scoreFormat
                    }
                    avatar {
                        medium
                    }
                    bannerImage
                    options {
                        titleLanguage
                        displayAdultContent
                        profileColor
                    }
                    statistics {
                        anime {
                            count
                            episodesWatched
                            meanScore
                        }
                        manga {
                            count
                            chaptersRead
                            meanScore
                        }
                    }
                    favourites {
                        anime(perPage: 3) {
                            nodes {
                                id
                                title {
                                    romaji
                                    english
                                    native
                                }
                                siteUrl
                            }
                        }
                        manga(perPage: 3) {
                            nodes {
                                id
                                title {
                                    romaji
                                    english
                                    native
                                }
                                siteUrl
                            }
                        }
                        characters(perPage: 3) {
                            nodes {
                                name {
                                    full
                                }
                                siteUrl
                            }
                        }
                        staff(perPage: 3) {
                            nodes {
                                name {
                                    full
                                }
                                siteUrl
                            }
                        }
                        studios(perPage: 3) {
                            nodes {
                                name
                                siteUrl
                                isAnimationStudio
                            }
                        }
                    }
                }
            }
        ";

    internal const string Viewer = @"
            query {
                Viewer {
                    id
                    name
                    siteUrl
                    avatar {
                        medium
                    }
                    bannerImage
                }
            }
        ";

    internal const string MediaUser = @"
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
                        siteUrl
                        mediaListOptions {
                            scoreFormat
                        }
                    }
                }
            }
        ";

    internal const string Recommendations = @"
            query($userId: Int, $type: MediaType) {
                User(id: $userId) {
                    id
                    name
                    avatar {
                        medium
                    }
                    siteUrl
                    options {
                        titleLanguage
                    }
                    statistics {
                        anime {
                            meanScore
                            standardDeviation
                        }
                        manga {
                            meanScore
                            standardDeviation
                        }
                    }
                }
                MediaListCollection(userId: $userId, type: $type, status_not_in: [PLANNING], forceSingleCompletedList: true) {
                    lists {
                        entries {
                            mediaId
                            score(format: POINT_100)
                            status
                            media {
                                relations {
                                    nodes {
                                        id
                                        type
                                    }
                                }
                                recommendations(sort: RATING_DESC, perPage: 5) {
                                    nodes {
                                        rating
                                        mediaRecommendation {
                                            id
                                            title {
                                                romaji
                                                english
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        ";

    internal const string RandomCharacter = @"
            query ($page: Int){
                Page(perPage: 1, page: $page) {
                    characters(sort: FAVOURITES_DESC) {
                        id
                        name {
                            full
                        }
                        image {
                            large
                        }
                        favourites
                        siteUrl
                        description(asHtml: false)
                        animes: media(type: ANIME) {
                            nodes {
                                id
                                title {
                                    romaji
                                    english
                                    native
                                }
                                siteUrl
                            }
                        }
                        mangas: media(type: MANGA) {
                            nodes {
                                id
                                title {
                                    romaji
                                    english
                                    native
                                }
                                siteUrl
                            }
                        }
                    }
                }
            }
        ";

    internal const string MediaList = @"
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

    internal const string RandomCharacterSimple = @"
        query($page: Int){
            Page(page: $page, perPage: 1){
                characters(sort: FAVOURITES_DESC){
                    name{ full },
                    image{ large },
                    siteUrl,
                    favourites,
                    media(sort: POPULARITY_DESC, perPage: 1){
                        nodes{ title{ romaji }, siteUrl }
                    }
                }
            }
        }";

    internal const string RandomMediaSimple = @"
        query($page: Int, $type: MediaType){
            Page(page: $page, perPage: 1){
                media(sort: FAVOURITES_DESC, isAdult: false, type: $type){
                    title{ romaji, english },
                    coverImage{ large },
                    siteUrl,
                    favourites
                }
            }
        }";

    // Consulta mínima: solo interesa leer los headers X-RateLimit-* de la respuesta.
    internal const string RateLimitProbe = @"query { Media(id: 1) { id } }";

    /// <summary>
    /// Pool de medias de los juegos. Los filtros se interpolan porque AniList no acepta variables en
    /// `genre` combinado con los `*_not_in`; el llamador arma el string con GameMediaFilters.
    /// </summary>
    internal static string GamePool(string filtros) => $$"""
        query($page: Int){
            Page(page: $page){
                media({{filtros}}){
                    id
                    siteUrl
                    type
                    favourites
                    title { romaji english }
                    averageScore
                    synonyms
                    coverImage { large }
                    characters(role: MAIN){
                        nodes { name { first last full } siteUrl favourites }
                    }
                    studios {
                        nodes { name siteUrl favourites isAnimationStudio }
                    }
                    relations {
                        edges {
                            relationType
                            node { id type siteUrl title { romaji english } synonyms }
                        }
                    }
                }
                pageInfo { hasNextPage lastPage }
            }
        }
        """;

    /// <summary>Página de personajes ordenada por favoritos, para el pool de trivia/ahorcado.</summary>
    internal const string CharacterPool = @"
        query($page: Int){
            Page(page: $page){
                characters(sort: FAVOURITES_DESC){
                    siteUrl
                    favourites
                    name { first last full }
                    image { large }
                    media(sort: POPULARITY_DESC, perPage: 1){
                        nodes { title { romaji } siteUrl }
                    }
                }
                pageInfo { hasNextPage }
            }
        }";

    internal const string GenreCollection = @"query { GenreCollection }";
}
