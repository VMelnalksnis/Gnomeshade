SELECT t.id,
       t.created_at          CreatedAt,
       t.owner_id            OwnerId,
       t.created_by_user_id  CreatedByUserId,
       t.modified_at         ModifiedAt,
       t.modified_by_user_id ModifiedByUserId,
       t.name AS             Name,
       t.normalized_name     NormalizedName,
       t.description
FROM tags t
         INNER JOIN owners ON owners.id = t.owner_id
         INNER JOIN ownerships ON owners.id = ownerships.owner_id
         INNER JOIN access ON access.id = ownerships.access_id
