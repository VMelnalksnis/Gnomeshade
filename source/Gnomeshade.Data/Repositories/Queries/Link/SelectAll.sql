SELECT links.id,
	   links.created_at            CreatedAt,
	   links.created_by_user_id    CreatedByUserId,
	   links.owner_id              OwnerId,
	   links.modified_at           ModifiedAt,
	   links.modified_by_user_id   ModifiedByUserId,
	   links.uri                AS Uri,
	   links.deleted_at         AS DeletedAt,
	   links.deleted_by_user_id AS DeletedByUserId
FROM links
