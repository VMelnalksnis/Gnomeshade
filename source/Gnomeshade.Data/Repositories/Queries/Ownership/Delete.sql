WITH accessable AS
		 (SELECT ownerships.id                 AS Id,
				 ownerships.created_at            CreatedAt,
				 ownerships.created_by_user_id AS CreatedByUserId,
				 ownerships.owner_id              OwnerId,
				 ownerships.user_id               UserId,
				 ownerships.access_id             AccessId
		  FROM ownerships ownerships
				   INNER JOIN owners on owners.id = ownerships.owner_id
				   INNER JOIN ownerships o on o.owner_id = owners.id
				   INNER JOIN access on access.id = o.access_id
		  WHERE (o.user_id = @ownerId AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER'))
			AND ownerships.id = @id)

DELETE
FROM ownerships
WHERE ownerships.id in (SELECT id from accessable);
