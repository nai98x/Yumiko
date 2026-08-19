-- Creates the link or repoints an existing one to another AniList account.
CREATE OR REPLACE FUNCTION anilist_user_upsert(p_user_id bigint, p_anilist_id integer)
RETURNS void
LANGUAGE sql
AS $$
    INSERT INTO anilist_users (user_id, anilist_id)
    VALUES (p_user_id, p_anilist_id)
    ON CONFLICT (user_id) DO UPDATE
        SET anilist_id = EXCLUDED.anilist_id;
$$;
