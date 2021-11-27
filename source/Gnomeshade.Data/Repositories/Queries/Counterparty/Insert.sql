INSERT INTO counterparties
    (owner_id, created_by_user_id, modified_by_user_id, name, normalized_name)
VALUES
    (@OwnerId, @CreatedByUserId, @ModifiedByUserId, @Name, @NormalizedName)
RETURNING id;
