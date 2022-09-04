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
SET modified_at         = CURRENT_TIMESTAMP,
    modified_by_user_id = @ModifiedByUserId,
    name                = @Name,
    normalized_name     = upper(@Name)
FROM c
WHERE counterparties.id IN (SELECT id FROM c)
RETURNING (SELECT id FROM c);
