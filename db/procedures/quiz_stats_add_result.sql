-- Accumulates the result of a finished game onto the row of that gamemode and difficulty.
-- The accuracy is recomputed over the accumulated totals with integer division (see quiz_stats).
CREATE OR REPLACE FUNCTION quiz_stats_add_result(
    p_guild_id       bigint,
    p_user_id        bigint,
    p_gamemode       text,
    p_difficulty     text,
    p_correct_rounds integer,
    p_total_rounds   integer
) RETURNS void
LANGUAGE sql
AS $$
    INSERT INTO quiz_stats (
        guild_id, user_id, gamemode, difficulty,
        games_played, correct_rounds, total_rounds, accuracy_percentage)
    VALUES (
        p_guild_id, p_user_id, p_gamemode, p_difficulty,
        1, p_correct_rounds, p_total_rounds, p_correct_rounds * 100 / p_total_rounds)
    ON CONFLICT (guild_id, user_id, gamemode, difficulty) DO UPDATE
        SET games_played        = quiz_stats.games_played + 1,
            correct_rounds      = quiz_stats.correct_rounds + EXCLUDED.correct_rounds,
            total_rounds        = quiz_stats.total_rounds + EXCLUDED.total_rounds,
            accuracy_percentage = (quiz_stats.correct_rounds + EXCLUDED.correct_rounds) * 100
                                  / (quiz_stats.total_rounds + EXCLUDED.total_rounds);
$$;
