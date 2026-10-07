-- Best pair records a user takes part of in a guild, best first.
CREATE OR REPLACE FUNCTION higher_or_lower_duo_user_get(p_guild_id bigint, p_user_id bigint, p_limit integer)
RETURNS SETOF higher_or_lower_duo_scores
LANGUAGE sql
STABLE
AS $$
    SELECT *
    FROM higher_or_lower_duo_scores
    WHERE guild_id = p_guild_id AND (first_user_id = p_user_id OR second_user_id = p_user_id)
    ORDER BY score DESC
    LIMIT p_limit;
$$;
