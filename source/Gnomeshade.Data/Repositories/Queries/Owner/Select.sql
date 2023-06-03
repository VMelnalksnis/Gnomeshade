SELECT o.id                 AS Id,
	   o.created_at         AS CreatedAt,
	   o.created_by_user_id AS CreatedByUserId,
	   o.deleted_at         AS DeletedAt,
	   o.deleted_by_user_id AS DeletedByUserId,
	   o.name,
	   o.normalized_name    AS NormalizedName
FROM owners o
		 INNER JOIN ownerships on o.id = ownerships.owner_id
		 INNER JOIN users on users.id = ownerships.user_id
		 INNER JOIN access on access.id = ownerships.access_id
WHERE ownerships.user_id = @userId
  AND (access.normalized_name = @access OR access.normalized_name = 'OWNER')
