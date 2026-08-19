-- Every row of a user in a guild: all gamemodes, all difficulties and all genres in one round trip.
CREATE OR REPLACE FUNCTION quiz_stats_user(p_guild_id bigint, p_user_id bigint)
RETURNS SETOF quiz_stats
LANGUAGE sql
STABLE
AS $$
    SELECT * FROM quiz_stats WHERE guild_id = p_guild_id AND user_id = p_user_id;
$$;
