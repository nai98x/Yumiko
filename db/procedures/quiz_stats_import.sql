-- Bulk import from Firestore. Idempotent: reimporting overwrites the row with the imported totals,
-- accuracy included, so the value that ordered the old leaderboards is preserved as it was.
CREATE OR REPLACE FUNCTION quiz_stats_import(
    p_guild_ids    bigint[],
    p_user_ids     bigint[],
    p_gamemodes    text[],
    p_difficulties text[],
    p_games_played integer[],
    p_correct      integer[],
    p_total        integer[],
    p_accuracy     integer[]
) RETURNS integer
LANGUAGE sql
AS $$
    WITH imported AS (
        INSERT INTO quiz_stats (
            guild_id, user_id, gamemode, difficulty,
            games_played, correct_rounds, total_rounds, accuracy_percentage)
        SELECT * FROM unnest(
            p_guild_ids, p_user_ids, p_gamemodes, p_difficulties,
            p_games_played, p_correct, p_total, p_accuracy)
        ON CONFLICT (guild_id, user_id, gamemode, difficulty) DO UPDATE
            SET games_played        = EXCLUDED.games_played,
                correct_rounds      = EXCLUDED.correct_rounds,
                total_rounds        = EXCLUDED.total_rounds,
                accuracy_percentage = EXCLUDED.accuracy_percentage
        RETURNING 1
    )
    SELECT count(*)::integer FROM imported;
$$;
