-- Record of a single user, or no rows if they never played in that guild.
CREATE OR REPLACE FUNCTION higher_or_lower_user_get(p_guild_id bigint, p_user_id bigint)
RETURNS SETOF higher_or_lower_scores
LANGUAGE sql
STABLE
AS $$
    SELECT * FROM higher_or_lower_scores WHERE guild_id = p_guild_id AND user_id = p_user_id;
$$;
