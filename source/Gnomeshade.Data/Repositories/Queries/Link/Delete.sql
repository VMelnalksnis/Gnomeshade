WITH deleteable_links AS
		 (SELECT links.id
		  FROM links
				   INNER JOIN owners ON links.owner_id = owners.id
				   INNER JOIN ownerships ON owners.id = ownerships.owner_id
				   INNER JOIN access ON ownerships.access_id = access.id
		  WHERE links.id = @id
			AND ownerships.user_id = @ownerId
			AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER'))
DELETE
FROM links USING deleteable_links
WHERE links.id = deleteable_links.id;
