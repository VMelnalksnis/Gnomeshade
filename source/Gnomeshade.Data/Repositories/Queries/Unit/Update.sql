WITH u AS (
    SELECT units.id
    FROM units
             INNER JOIN owners ON owners.id = units.owner_id
             INNER JOIN ownerships ON owners.id = ownerships.owner_id
             INNER JOIN access ON access.id = ownerships.access_id
    WHERE units.id = @Id
      AND ownerships.user_id = @OwnerId
      AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER')
)
UPDATE units
SET modified_at         = DEFAULT,
    modified_by_user_id = @ModifiedByUserId,
    name                = @Name,
    normalized_name     = @NormalizedName,
    parent_unit_id      = @ParentUnitId,
    multiplier          = @Multiplier
FROM u
WHERE units.id = u.id
RETURNING u.id;
