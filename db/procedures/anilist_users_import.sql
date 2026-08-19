-- Bulk import from Firestore. Idempotent: reimporting overwrites the link with the imported value.
CREATE OR REPLACE FUNCTION anilist_users_import(
    p_user_ids    bigint[],
    p_anilist_ids integer[]
) RETURNS integer
LANGUAGE sql
AS $$
    WITH imported AS (
        INSERT INTO anilist_users (user_id, anilist_id)
        SELECT * FROM unnest(p_user_ids, p_anilist_ids)
        ON CONFLICT (user_id) DO UPDATE
            SET anilist_id = EXCLUDED.anilist_id
        RETURNING 1
    )
    SELECT count(*)::integer FROM imported;
$$;
