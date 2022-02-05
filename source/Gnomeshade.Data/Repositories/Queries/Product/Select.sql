SELECT p.id,
       p.created_at        CreatedAt,
       p.owner_id          OwnerId,
       created_by_user_id  CreatedByUserId,
       modified_at         ModifiedAt,
       modified_by_user_id ModifiedByUserId,
       p.name AS           Name,
       p.normalized_name   NormalizedName,
       description,
       unit_id             UnitId
FROM products p
         INNER JOIN owners ON owners.id = p.owner_id
         INNER JOIN ownerships ON owners.id = ownerships.owner_id
         INNER JOIN access ON access.id = ownerships.access_id
