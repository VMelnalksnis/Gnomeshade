SELECT o.id                 AS Id,
	   o.created_at            CreatedAt,
	   o.created_by_user_id AS CreatedByUserId,
	   o.owner_id              OwnerId,
	   o.user_id               UserId,
	   o.access_id             AccessId
FROM ownerships o
	INNER JOIN owners on owners.id = o.owner_id
    INNER JOIN ownerships on ownerships.owner_id = owners.id
	INNER JOIN access on access.id = ownerships.access_id
