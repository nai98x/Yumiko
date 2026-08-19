-- Returns the AniList link of a user, or no rows if they never linked their account.
CREATE OR REPLACE FUNCTION anilist_user_get(p_user_id bigint)
RETURNS SETOF anilist_users
LANGUAGE sql
STABLE
AS $$
    SELECT * FROM anilist_users WHERE user_id = p_user_id;
$$;
