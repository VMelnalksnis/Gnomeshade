WITH t AS (
    SELECT tags.id
    FROM tags
             INNER JOIN owners ON owners.id = tags.owner_id
             INNER JOIN ownerships ON owners.id = ownerships.owner_id
             INNER JOIN access ON access.id = ownerships.access_id
    WHERE tags.id = @Id
      AND ownerships.user_id = @OwnerId
      AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER')
)
UPDATE tags
SET modified_at         = DEFAULT,
    modified_by_user_id = @ModifiedByUserId,
    name                = @Name,
    normalized_name     = @NormalizedName,
    description         = @Description
FROM t
WHERE tags.id = t.id
RETURNING t.id;
