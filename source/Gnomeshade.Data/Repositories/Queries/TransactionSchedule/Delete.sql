WITH accessable AS
		 (SELECT transaction_schedules.id
		  FROM transaction_schedules
				   INNER JOIN owners ON owners.id = transaction_schedules.owner_id
				   INNER JOIN ownerships ON owners.id = ownerships.owner_id
				   INNER JOIN access ON access.id = ownerships.access_id
		  WHERE ownerships.user_id = @userId
			AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER')
			AND transaction_schedules.deleted_at IS NULL
			AND transaction_schedules.id = @id)

UPDATE transaction_schedules
SET deleted_at         = CURRENT_TIMESTAMP,
	deleted_by_user_id = @userId
FROM accessable
WHERE transaction_schedules.id IN (SELECT id FROM accessable);
