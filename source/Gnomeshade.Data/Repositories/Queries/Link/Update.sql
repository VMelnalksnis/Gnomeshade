WITH l AS (
    SELECT links.id
    FROM links
             INNER JOIN owners ON owners.id = links.owner_id
             INNER JOIN ownerships ON owners.id = ownerships.owner_id
             INNER JOIN access ON access.id = ownerships.access_id
    WHERE links.id = @Id
      AND ownerships.user_id = @OwnerId
      AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER')
)
UPDATE links
SET modified_at         = DEFAULT,
    modified_by_user_id = @ModifiedByUserId,
    uri                 = @Uri
FROM l
WHERE links.id = l.id
RETURNING l.id;
