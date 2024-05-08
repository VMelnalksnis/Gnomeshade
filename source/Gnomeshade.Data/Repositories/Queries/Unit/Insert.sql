INSERT INTO units
    (id, owner_id, created_by_user_id, modified_by_user_id, name, normalized_name, symbol, parent_unit_id, multiplier, inverse_multiplier)
VALUES
    (@Id, @OwnerId, @CreatedByUserId, @ModifiedByUserId, @Name, upper(@Name), @Symbol, @ParentUnitId, @Multiplier, @InverseMultiplier)
RETURNING id;
