SELECT u.id,
       u.created_at        CreatedAt,
       u.owner_id          OwnerId,
       created_by_user_id  CreatedByUserId,
       modified_at         ModifiedAt,
       modified_by_user_id ModifiedByUserId,
       name,
       normalized_name     NormalizedName,
       parent_unit_id      ParentUnitId,
       multiplier
FROM units u
         INNER JOIN owners ON owners.id = u.owner_id
         INNER JOIN ownerships ON owners.id = ownerships.owner_id
