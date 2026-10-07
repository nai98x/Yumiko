-- Top pair scores of a guild, best first.
CREATE OR REPLACE FUNCTION higher_or_lower_duo_leaderboard(p_guild_id bigint, p_limit integer)
RETURNS SETOF higher_or_lower_duo_scores
LANGUAGE sql
STABLE
AS $$
    SELECT *
    FROM higher_or_lower_duo_scores
    WHERE guild_id = p_guild_id
    ORDER BY score DESC
    LIMIT p_limit;
$$;
