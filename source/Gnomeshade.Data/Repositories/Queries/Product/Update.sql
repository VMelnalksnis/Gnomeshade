WITH p AS (
    SELECT products.id
    FROM products
             INNER JOIN owners ON owners.id = products.owner_id
             INNER JOIN ownerships ON owners.id = ownerships.owner_id
             INNER JOIN access ON access.id = ownerships.access_id
    WHERE products.id = @Id
      AND ownerships.user_id = @OwnerId
      AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER')
)
UPDATE products
SET modified_at         = DEFAULT,
    modified_by_user_id = @ModifiedByUserId,
    name                = @Name,
    normalized_name     = @NormalizedName,
    description         = @Description,
    unit_id             = @UnitId
FROM p
WHERE products.id = p.id
RETURNING p.id;
