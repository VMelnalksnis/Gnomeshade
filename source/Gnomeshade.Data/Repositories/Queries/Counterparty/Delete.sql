WITH c AS (SELECT counterparties.id
		   FROM counterparties
					INNER JOIN owners ON owners.id = counterparties.owner_id
					INNER JOIN ownerships ON owners.id = ownerships.owner_id
					INNER JOIN access ON access.id = ownerships.access_id
		   WHERE counterparties.id = @id
			 AND ownerships.user_id = @ownerId
			 AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER'))
UPDATE counterparties
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = @ownerId,
	deleted_at          = CURRENT_TIMESTAMP,
	deleted_by_user_id  = @ownerId
FROM c
WHERE counterparties.id IN (SELECT id FROM c);
