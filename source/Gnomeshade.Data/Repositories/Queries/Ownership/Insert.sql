INSERT INTO ownerships
    (id, owner_id, user_id)
VALUES
    (@id, @id, @id)
RETURNING id;
