namespace Yumiko.Utils
{
    using Humanizer;

    public static class ExtensionMethods
    {
        public static void Shuffle<T>(this IList<T> list, Random rnd)
        {
            for (var i = list.Count; i > 0; i--)
            {
                list.Swap(0, rnd.Next(0, i));
            }
        }

        public static void Swap<T>(this IList<T> list, int i, int j) => (list[j], list[i]) = (list[i], list[j]);

        public static string UppercaseFirst(this string s) => s.Transform(To.LowerCase, To.TitleCase);

        public static string ToYesNo(this bool condition) => condition ? translations.yes : translations.no;

        public static string ToSpanish(this Difficulty difficulty)
        {
            return difficulty switch
            {
                Difficulty.Easy => "Fácil",
                Difficulty.Normal => "Media",
                Difficulty.Hard => "Dificil",
                Difficulty.Extreme => "Extremo",
                _ => throw new ArgumentException("Programming error"),
            };
        }

        public static string ToSpanish(this Gamemode gamemode)
        {
            return gamemode switch
            {
                Gamemode.Characters => "personaje",
                Gamemode.Animes => "anime",
                Gamemode.Mangas => "manga",
                Gamemode.Studios => "estudio",
                Gamemode.Protagonists => "protagonista",
                Gamemode.Genres => "genero",
                _ => throw new ArgumentException("Programming error"),
            };
        }

        public static int ShardCount(this DiscordShardedClient client) => client.ShardClients.Count;

        public static int GuildCount(this DiscordShardedClient client) => client.ShardClients.Values.Sum(x => x.Guilds.Count);

        public static int UserCount(this DiscordShardedClient client) => client.ShardClients.Values.Sum(x => x.Guilds.Sum(y => y.Value.MemberCount));

        public static MemoryStream ToMemoryStream(this byte[] byteArray)
        {
            return new MemoryStream(byteArray)
            {
                Position = 0,
            };
        }

        public static string NormalizeField(this string s)
        {
            if (s.Length > 1024)
            {
                string aux = s.Remove(1024);
                int index = aux.LastIndexOf('[');
                if (index != -1)
                {
                    return aux.Remove(aux.LastIndexOf('[')) + "...";
                }
                else
                {
                    return aux.Remove(aux.Length - 4) + " ...";
                }
            }

            return s;
        }

        public static string NormalizeButton(this string s)
        {
            if (s.Length > 80)
            {
                return s.Remove(76) + " ...";
            }

            return s;
        }

        public static string NormalizeSelectMenuOption(this string s)
        {
            if (s.Length > 100)
            {
                return s.Remove(96) + " ...";
            }

            return s;
        }

        public static string NormalizeDescription(this string s)
        {
            if (s.Length > 4096)
            {
                string aux = s.Remove(4096);
                int index = aux.LastIndexOf('[');
                if (index != -1)
                {
                    return aux.Remove(aux.LastIndexOf('[')) + "...";
                }
                else
                {
                    return aux.Remove(aux.Length - 4) + " ...";
                }
            }

            return s;
        }

        public static string NormalizeDescriptionNewLine(this string s)
        {
            if (s.Length > 4096)
            {
                string aux = s.Remove(4096);
                return aux.Remove(aux.LastIndexOf("\n"));
            }

            return s;
        }

        public static string NormalizeCustomId(this string s)
        {
            if (s.Length > 80)
            {
                return s.Remove(76) + " ...";
            }

            return s;
        }
    }
}
