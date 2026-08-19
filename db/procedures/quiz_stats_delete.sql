-- Wipes every difficulty (or every genre, in 'Genres' mode) of a user in one gamemode.
CREATE OR REPLACE FUNCTION quiz_stats_delete(p_guild_id bigint, p_user_id bigint, p_gamemode text)
RETURNS void
LANGUAGE sql
AS $$
    DELETE FROM quiz_stats
    WHERE guild_id = p_guild_id AND user_id = p_user_id AND gamemode = p_gamemode;
$$;
