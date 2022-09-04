WITH p AS (SELECT purchases.id
		   FROM purchases
					INNER JOIN owners ON owners.id = purchases.owner_id
					INNER JOIN ownerships ON owners.id = ownerships.owner_id
					INNER JOIN access ON access.id = ownerships.access_id
		   WHERE purchases.id = @id
			 AND ownerships.user_id = @ownerId
			 AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER'))
UPDATE purchases
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = @ownerId,
	deleted_at          = CURRENT_TIMESTAMP,
	deleted_by_user_id  = @ownerId
FROM p
WHERE purchases.id IN (SELECT id FROM p);
