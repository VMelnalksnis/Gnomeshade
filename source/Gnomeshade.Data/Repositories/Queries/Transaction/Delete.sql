WITH t AS (SELECT transactions.id
		   FROM transactions
					INNER JOIN owners ON owners.id = transactions.owner_id
					INNER JOIN ownerships ON owners.id = ownerships.owner_id
					INNER JOIN access ON access.id = ownerships.access_id
		   WHERE transactions.id = @id
			 AND ownerships.user_id = @ownerId
			 AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER'))
UPDATE transactions
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = @ownerId,
	deleted_at          = CURRENT_TIMESTAMP,
	deleted_by_user_id  = @ownerId
FROM t
WHERE transactions.id IN (SELECT id FROM t);
