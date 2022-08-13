SELECT p.id,
	   p.created_at          CreatedAt,
	   p.owner_id            OwnerId,
	   p.created_by_user_id  CreatedByUserId,
	   p.modified_at         ModifiedAt,
	   p.modified_by_user_id ModifiedByUserId,
	   p.name AS             Name,
	   p.normalized_name     NormalizedName,
	   p.sku                 Sku,
	   p.description,
	   p.unit_id             UnitId,
	   p.category_id         CategoryId
FROM products p
		 INNER JOIN owners ON owners.id = p.owner_id
		 INNER JOIN ownerships ON owners.id = ownerships.owner_id
		 INNER JOIN access ON access.id = ownerships.access_id
