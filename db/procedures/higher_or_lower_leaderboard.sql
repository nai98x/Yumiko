-- Top scores of a guild, best first.
CREATE OR REPLACE FUNCTION higher_or_lower_leaderboard(p_guild_id bigint, p_limit integer)
RETURNS SETOF higher_or_lower_scores
LANGUAGE sql
STABLE
AS $$
    SELECT *
    FROM higher_or_lower_scores
    WHERE guild_id = p_guild_id
    ORDER BY score DESC
    LIMIT p_limit;
$$;
