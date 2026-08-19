-- Unlinks the account. Returns false when there was nothing to unlink.
CREATE OR REPLACE FUNCTION anilist_user_delete(p_user_id bigint)
RETURNS boolean
LANGUAGE plpgsql
AS $$
DECLARE
    v_deleted integer;
BEGIN
    DELETE FROM anilist_users WHERE user_id = p_user_id;
    GET DIAGNOSTICS v_deleted = ROW_COUNT;
    RETURN v_deleted > 0;
END;
$$;
