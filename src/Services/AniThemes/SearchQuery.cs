namespace Yumiko.Services
{
    using DSharpPlus.SlashCommands;
    using Newtonsoft.Json;
    using RestSharp;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Yumiko.Datatypes;

    public static class SearchQuery
    {
        public static async Task<AnithemeData?> Search(InteractionContext ctx, double timeout, string search)
        {
            try
            {
                var client = new RestClient($"{Constants.AniThemesAPIUrl}/anime?q={search}&include=animethemes.animethemeentries.videos");
                var request = new RestRequest();

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful && response.Content != null)
                {
                    var data = JsonConvert.DeserializeObject<SearchResponse>(response.Content);
                    if (data == null || data.Anime == null || data.Anime.Count == 0) return null;

                    var anime = await ChooseAnimeAsync(ctx, timeout, data.Anime);
                    if (anime == null) return null;

                    var theme = await ChooseThemeAsync(ctx, timeout, anime.Animethemes);
                    if (theme == null) return null;

                    var song = await ChooseSongAsync(ctx, timeout, theme.Animethemeentries);
                    if (song == null) return null;

                    var video = song.Videos.FirstOrDefault();
                    if (video == null) return null;

                    return new AnithemeData(anime, theme, song, video);
                }

                return null;
            }
            catch (Exception e)
            {
                await Common.GrabarLogErrorAsync(ctx.Guild, ctx.Channel, $"Unknown error: {e.Message}\n{Formatter.BlockCode(e.StackTrace)}");
                return null;
            }
        }

        private static async Task<AnimeAniTheme?> ChooseAnimeAsync(InteractionContext ctx, double timeout, List<AnimeAniTheme> list)
        {
            if (list.Count == 1) return list.First();

            list.Sort((x, y) => $"{x.Name} ({x.Season} {x.Year})".CompareTo($"{y.Name} ({y.Season} {y.Year})"));

            List<TitleDescription> opc = new();
            foreach (var item in list)
            {
                opc.Add(new TitleDescription
                {
                    Title = $"{item.Name}",
                    Description = $"{item.Season} {item.Year}"
                });
            }

            var elegido = await Common.GetElegidoAsync(ctx, timeout, opc);
            if (elegido > 0) return list[elegido - 1];
            else return null;
        }

        private static async Task<Animetheme?> ChooseThemeAsync(InteractionContext ctx, double timeout, List<Animetheme> list)
        {
            if (list.Count == 1) return list.First();

            list.Sort((x, y) => $"{x.Type} {x.Sequence ?? 0}".CompareTo($"{y.Type} {y.Sequence ?? 0}"));

            List<TitleDescription> opc = new();
            foreach (var item in list)
            {
                string title = $"{item.Type}";
                if (item.Sequence is not null) title += $" {item.Sequence}";

                opc.Add(new TitleDescription
                {
                    Title = title
                });
            }

            var elegido = await Common.GetElegidoAsync(ctx, timeout, opc);
            if (elegido > 0) return list[elegido - 1];
            else return null;
        }

        private static async Task<AnimeThemeEntry?> ChooseSongAsync(InteractionContext ctx, double timeout, List<AnimeThemeEntry> list)
        {
            if (list.Count == 1) return list.First();

            list.Sort((x, y) => $"v{x.Version}".CompareTo($"v{y.Version}"));

            List<TitleDescription> opc = new();
            foreach (var item in list)
            {
                string title = $"v{item.Version}";
                if (item.Spoiler) title += $" (SPOILER)";
                if (item.Nsfw) title += $" (NSFW)";

                opc.Add(new TitleDescription
                {
                    Title = title
                });
            }

            var elegido = await Common.GetElegidoAsync(ctx, timeout, opc);
            if (elegido > 0) return list[elegido - 1];
            else return null;
        }
    }
}
