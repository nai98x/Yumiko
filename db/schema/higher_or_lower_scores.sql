-- Higher or Lower record per user and guild. Only the best score is kept, never the history.
CREATE TABLE IF NOT EXISTS higher_or_lower_scores (
    guild_id bigint  NOT NULL,
    user_id  bigint  NOT NULL,
    score    integer NOT NULL,
    PRIMARY KEY (guild_id, user_id)
);

CREATE INDEX IF NOT EXISTS higher_or_lower_scores_leaderboard
    ON higher_or_lower_scores (guild_id, score DESC);
