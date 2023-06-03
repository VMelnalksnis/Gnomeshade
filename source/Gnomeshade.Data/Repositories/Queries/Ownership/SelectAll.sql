SELECT o.id                 AS Id,
	   o.created_at            CreatedAt,
	   o.created_by_user_id AS CreatedByUserId,
	   o.owner_id              OwnerId,
	   o.user_id               UserId,
	   o.access_id             AccessId
FROM ownerships o
