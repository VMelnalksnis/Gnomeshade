DELETE
FROM tags
    USING access, owners
        INNER JOIN ownerships ON owners.id = ownerships.owner_id
        INNER JOIN access a ON a.id = ownerships.access_id
WHERE tags.id = @id
  AND ownerships.user_id = @ownerId
  AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER');
