INSERT INTO ownerships
    (id, owner_id, user_id, access_id)
VALUES
    (@id, @id, @id, @AccessId)
RETURNING id;
