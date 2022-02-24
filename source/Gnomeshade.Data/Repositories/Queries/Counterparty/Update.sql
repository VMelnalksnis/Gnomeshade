WITH c AS (
    SELECT counterparties.id
    FROM counterparties
             INNER JOIN owners ON owners.id = counterparties.owner_id
             INNER JOIN ownerships ON owners.id = ownerships.owner_id
             INNER JOIN access ON access.id = ownerships.access_id
    WHERE counterparties.id = @Id
      AND ownerships.user_id = @OwnerId
      AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER')
)
UPDATE counterparties
SET modified_at         = DEFAULT,
    modified_by_user_id = @ModifiedByUserId,
    name                = @Name,
    normalized_name     = @NormalizedName
FROM c
WHERE counterparties.id = c.id
RETURNING c.id;
