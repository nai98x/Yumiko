-- Stores the score of the pair only when it beats its stored record. The users can come in any order.
-- Returns true when it was saved, which is what the game uses to announce a new record.
CREATE OR REPLACE FUNCTION higher_or_lower_duo_add_result(
    p_guild_id bigint,
    p_user_id1 bigint,
    p_user_id2 bigint,
    p_score    integer
) RETURNS boolean
LANGUAGE plpgsql
AS $$
DECLARE
    v_saved boolean;
BEGIN
    INSERT INTO higher_or_lower_duo_scores (guild_id, first_user_id, second_user_id, score)
    VALUES (p_guild_id, least(p_user_id1, p_user_id2), greatest(p_user_id1, p_user_id2), p_score)
    ON CONFLICT (guild_id, first_user_id, second_user_id) DO UPDATE
        SET score = EXCLUDED.score
        WHERE higher_or_lower_duo_scores.score < EXCLUDED.score
    RETURNING true INTO v_saved;

    RETURN coalesce(v_saved, false);
END;
$$;
