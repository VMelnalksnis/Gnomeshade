INSERT INTO counterparties
    (id, owner_id, created_by_user_id, modified_by_user_id, name, normalized_name)
VALUES
    (@Id, @OwnerId, @CreatedByUserId, @ModifiedByUserId, @Name, @NormalizedName)
RETURNING id;
