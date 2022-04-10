SELECT links.id,
       links.created_at          CreatedAt,
       links.owner_id            OwnerId,
       links.created_by_user_id  CreatedByUserId,
       links.modified_at         ModifiedAt,
       links.modified_by_user_id ModifiedByUserId,
       links.uri AS              Uri
FROM links
         INNER JOIN owners ON owners.id = links.owner_id
         INNER JOIN ownerships ON owners.id = ownerships.owner_id
         INNER JOIN access ON access.id = ownerships.access_id
