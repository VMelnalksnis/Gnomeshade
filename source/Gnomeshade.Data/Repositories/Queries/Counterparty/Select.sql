SELECT c.id,
       c.created_at        CreatedAt,
       c.owner_id          OwnerId,
       created_by_user_id  CreatedByUserId,
       modified_at         ModifiedAt,
       modified_by_user_id ModifiedByUserId,
       name,
       normalized_name     NormalizedName
FROM counterparties c
         INNER JOIN owners ON owners.id = c.owner_id
         INNER JOIN ownerships ON owners.id = ownerships.owner_id
