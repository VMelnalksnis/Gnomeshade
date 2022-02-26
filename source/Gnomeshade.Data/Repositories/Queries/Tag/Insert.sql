INSERT INTO tags
    (id, owner_id, created_by_user_id, modified_by_user_id, name, normalized_name, description)
VALUES
    (@Id, @OwnerId, @CreatedByUserId, @ModifiedByUserId, @Name, @NormalizedName, @Description)
RETURNING id;
