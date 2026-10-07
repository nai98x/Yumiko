-- Higher or Lower duo record per pair of users and guild. Only the best score is kept, never the
-- history. The pair is stored ordered (first_user_id < second_user_id) so A+B and B+A share a row.
CREATE TABLE IF NOT EXISTS higher_or_lower_duo_scores (
    guild_id       bigint  NOT NULL,
    first_user_id  bigint  NOT NULL,
    second_user_id bigint  NOT NULL,
    score          integer NOT NULL,
    PRIMARY KEY (guild_id, first_user_id, second_user_id),
    CHECK (first_user_id < second_user_id)
);

CREATE INDEX IF NOT EXISTS higher_or_lower_duo_scores_leaderboard
    ON higher_or_lower_duo_scores (guild_id, score DESC);

CREATE INDEX IF NOT EXISTS higher_or_lower_duo_scores_second_user
    ON higher_or_lower_duo_scores (guild_id, second_user_id);
