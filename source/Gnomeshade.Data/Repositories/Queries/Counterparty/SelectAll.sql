SELECT c.id,
	   c.created_at            CreatedAt,
	   c.owner_id              OwnerId,
	   c.created_by_user_id    CreatedByUserId,
	   c.modified_at           ModifiedAt,
	   c.modified_by_user_id   ModifiedByUserId,
	   c.name               AS Name,
	   c.normalized_name       NormalizedName,
	   c.deleted_at         AS DeletedAt,
	   c.deleted_by_user_id AS DeletedByUserId
FROM counterparties c
