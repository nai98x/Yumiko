-- Wipes the record of a user in a guild.
CREATE OR REPLACE FUNCTION higher_or_lower_delete(p_guild_id bigint, p_user_id bigint)
RETURNS void
LANGUAGE sql
AS $$
    DELETE FROM higher_or_lower_scores WHERE guild_id = p_guild_id AND user_id = p_user_id;
$$;
