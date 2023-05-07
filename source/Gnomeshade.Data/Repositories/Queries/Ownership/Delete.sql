WITH o AS (SELECT o.id
		   FROM ownerships o
					INNER JOIN owners on owners.id = o.owner_id
					INNER JOIN ownerships ownerships on ownerships.owner_id = owners.id
					INNER JOIN access access on access.id = ownerships.access_id
		   WHERE o.id = @id
			 AND ownerships.user_id = @ownerId
			 AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER'))
DELETE
FROM ownerships
WHERE ownerships.id in (SELECT id from o);
