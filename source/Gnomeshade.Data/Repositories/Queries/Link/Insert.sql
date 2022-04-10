INSERT INTO links
    (id, owner_id, created_by_user_id, modified_by_user_id, uri)
VALUES
    (@Id, @OwnerId, @CreatedByUserId, @ModifiedByUserId, @Uri)
RETURNING id;
