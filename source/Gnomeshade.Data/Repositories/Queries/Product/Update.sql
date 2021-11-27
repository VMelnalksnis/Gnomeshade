UPDATE products
SET modified_at         = DEFAULT,
    modified_by_user_id = @ModifiedByUserId,
    name                = @Name,
    normalized_name     = @NormalizedName,
    description         = @Description,
    unit_id             = @UnitId
WHERE id = @Id
RETURNING id;
