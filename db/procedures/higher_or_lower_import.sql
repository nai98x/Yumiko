-- Bulk import from Firestore. Idempotent: reimporting overwrites the score with the imported one.
CREATE OR REPLACE FUNCTION higher_or_lower_import(
    p_guild_ids bigint[],
    p_user_ids  bigint[],
    p_scores    integer[]
) RETURNS integer
LANGUAGE sql
AS $$
    WITH imported AS (
        INSERT INTO higher_or_lower_scores (guild_id, user_id, score)
        SELECT * FROM unnest(p_guild_ids, p_user_ids, p_scores)
        ON CONFLICT (guild_id, user_id) DO UPDATE
            SET score = EXCLUDED.score
        RETURNING 1
    )
    SELECT count(*)::integer FROM imported;
$$;
