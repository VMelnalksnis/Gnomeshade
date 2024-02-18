WITH accessable AS
		 (SELECT loans2.id
		  FROM loans2
				   INNER JOIN owners ON owners.id = loans2.owner_id
				   INNER JOIN ownerships ON owners.id = ownerships.owner_id
				   INNER JOIN access ON access.id = ownerships.access_id
		  WHERE ownerships.user_id = @userId
			AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER')
			AND loans2.deleted_at IS NULL
			AND loans2.id = @id)

UPDATE loans2
SET deleted_at         = CURRENT_TIMESTAMP,
	deleted_by_user_id = @userId
FROM accessable
WHERE loans2.id IN (SELECT id FROM accessable);
