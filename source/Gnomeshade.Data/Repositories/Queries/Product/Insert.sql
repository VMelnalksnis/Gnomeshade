INSERT INTO products
    (id, owner_id, created_by_user_id, modified_by_user_id, name, normalized_name, description, unit_id)
VALUES
    (@Id, @OwnerId, @CreatedByUserId, @ModifiedByUserId, @Name, @NormalizedName, @Description, @UnitId)
RETURNING id;
