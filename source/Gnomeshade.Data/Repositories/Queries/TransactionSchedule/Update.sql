WITH accessable AS
		 (SELECT transaction_schedules.id
		  FROM transaction_schedules
				   INNER JOIN owners ON owners.id = transaction_schedules.owner_id
				   INNER JOIN ownerships ON owners.id = ownerships.owner_id
				   INNER JOIN access ON access.id = ownerships.access_id
		  WHERE ownerships.user_id = @ModifiedByUserId
			AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER')
			AND transaction_schedules.deleted_at IS NULL
			AND transaction_schedules.id = @Id)

UPDATE transaction_schedules
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = @ModifiedByUserId,
	name                = @Name,
	normalized_name     = upper(@Name),
	starting_at         = @StartingAt,
	period              = @Period,
	count               = @Count
FROM accessable
WHERE transaction_schedules.id IN (SELECT id FROM accessable);
