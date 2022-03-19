INSERT INTO units
    (id, owner_id, created_by_user_id, modified_by_user_id, name, normalized_name, parent_unit_id, multiplier)
VALUES
    (@Id, @OwnerId, @CreatedByUserId, @ModifiedByUserId, @Name, @NormalizedName, @ParentUnitId, @Multiplier)
RETURNING id;
