WITH accessable AS
		 (SELECT transaction_schedules.id
		  FROM transaction_schedules
				   INNER JOIN owners ON owners.id = transaction_schedules.owner_id
				   INNER JOIN ownerships ON owners.id = ownerships.owner_id
				   INNER JOIN access ON access.id = ownerships.access_id
		  WHERE ownerships.user_id = @userId
			AND (access.normalized_name = @access OR access.normalized_name = 'OWNER'))

SELECT transaction_schedules.id                  AS Id,
	   transaction_schedules.created_at          AS CreatedAt,
	   transaction_schedules.created_by_user_id  AS CreatedByUserId,
	   transaction_schedules.deleted_at          AS DeletedAt,
	   transaction_schedules.deleted_by_user_id  AS DeletedByUserId,
	   transaction_schedules.owner_id            AS OwnerId,
	   transaction_schedules.modified_at         AS ModifiedAt,
	   transaction_schedules.modified_by_user_id AS ModifiedByUserId,
	   transaction_schedules.name                AS Name,
	   transaction_schedules.normalized_name     AS NormalizedName,
	   transaction_schedules.starting_at         AS StartingAt,
	   transaction_schedules.period              AS Period,
	   transaction_schedules.count               AS Count
FROM transaction_schedules
WHERE transaction_schedules.id IN (SELECT id from accessable)
