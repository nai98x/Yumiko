-- Leaderboard of a gamemode and difficulty. In 'Genres' mode p_difficulty is the genre name.
-- Ties on accuracy are broken by how many rounds were played, so a 100% out of 5 rounds does not
-- outrank a 100% out of 200.
CREATE OR REPLACE FUNCTION quiz_stats_leaderboard(
    p_guild_id   bigint,
    p_gamemode   text,
    p_difficulty text,
    p_limit      integer
) RETURNS SETOF quiz_stats
LANGUAGE sql
STABLE
AS $$
    SELECT *
    FROM quiz_stats
    WHERE guild_id = p_guild_id AND gamemode = p_gamemode AND difficulty = p_difficulty
    ORDER BY accuracy_percentage DESC, total_rounds DESC
    LIMIT p_limit;
$$;
