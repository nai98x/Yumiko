-- Stores the score only when it beats the stored record. Returns true when it was saved, which is
-- what the game uses to announce a new record.
CREATE OR REPLACE FUNCTION higher_or_lower_add_result(
    p_guild_id bigint,
    p_user_id  bigint,
    p_score    integer
) RETURNS boolean
LANGUAGE plpgsql
AS $$
DECLARE
    v_saved boolean;
BEGIN
    INSERT INTO higher_or_lower_scores (guild_id, user_id, score)
    VALUES (p_guild_id, p_user_id, p_score)
    ON CONFLICT (guild_id, user_id) DO UPDATE
        SET score = EXCLUDED.score
        WHERE higher_or_lower_scores.score < EXCLUDED.score
    RETURNING true INTO v_saved;

    RETURN coalesce(v_saved, false);
END;
$$;
