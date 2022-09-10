SELECT u.id,
	   u.created_at            CreatedAt,
	   u.owner_id              OwnerId,
	   u.created_by_user_id    CreatedByUserId,
	   u.modified_at           ModifiedAt,
	   u.modified_by_user_id   ModifiedByUserId,
	   u.name               AS Name,
	   u.normalized_name       NormalizedName,
	   u.symbol             AS Symbol,
	   u.deleted_at         AS DeletedAt,
	   u.deleted_by_user_id AS DeletedByUserId,
	   u.parent_unit_id        ParentUnitId,
	   u.multiplier
FROM units u
		 INNER JOIN owners ON owners.id = u.owner_id
		 INNER JOIN ownerships ON owners.id = ownerships.owner_id
		 INNER JOIN access ON access.id = ownerships.access_id
