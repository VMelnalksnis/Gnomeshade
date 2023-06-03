WITH accessable AS
		 (SELECT accounts.id
		  FROM accounts
				   INNER JOIN owners ON owners.id = accounts.owner_id
				   INNER JOIN ownerships ON owners.id = ownerships.owner_id
				   INNER JOIN access ON access.id = ownerships.access_id
		  WHERE ownerships.user_id = @ownerId
			AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER')
			AND accounts.deleted_at IS NULL
			AND accounts.id = @Id)

UPDATE accounts
SET deleted_at         = CURRENT_TIMESTAMP,
	deleted_by_user_id = @ownerId
FROM accessable
WHERE accounts.id IN (SELECT id FROM accessable);
