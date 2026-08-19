-- Link between a Discord account and an AniList account. Global, not per guild.
CREATE TABLE IF NOT EXISTS anilist_users (
    user_id    bigint PRIMARY KEY,
    anilist_id integer NOT NULL
);
