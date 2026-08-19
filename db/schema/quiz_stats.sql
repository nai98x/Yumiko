-- Accumulated trivia stats per user, guild, gamemode and difficulty.
--
-- `gamemode` holds the name of the Gamemode enum ('Characters', 'Animes', ...) and `difficulty` the
-- name of the Difficulty enum ('Easy', 'Normal', 'Hard', 'Extreme'), except when gamemode is
-- 'Genres': there `difficulty` holds the genre name as AniList spells it ('Action', 'Comedy', ...).
--
-- `accuracy_percentage` is stored, not computed on read, and uses integer division: it is the value
-- the leaderboard is ordered by and rounding it any other way re-ranks every existing leaderboard.
CREATE TABLE IF NOT EXISTS quiz_stats (
    guild_id            bigint  NOT NULL,
    user_id             bigint  NOT NULL,
    gamemode            text    NOT NULL,
    difficulty          text    NOT NULL,
    games_played        integer NOT NULL,
    correct_rounds      integer NOT NULL,
    total_rounds        integer NOT NULL,
    accuracy_percentage integer NOT NULL,
    PRIMARY KEY (guild_id, user_id, gamemode, difficulty)
);

CREATE INDEX IF NOT EXISTS quiz_stats_leaderboard
    ON quiz_stats (guild_id, gamemode, difficulty, accuracy_percentage DESC, total_rounds DESC);
