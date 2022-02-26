INSERT INTO tag_tags(created_by_user_id, tag_id, tagged_item_id)
SELECT @ownerId, @tagId, tags.id
FROM tags
         INNER JOIN owners ON owners.id = tags.owner_id
         INNER JOIN ownerships ON owners.id = ownerships.owner_id
         INNER JOIN access ON access.id = ownerships.access_id
WHERE tags.id = @id
  AND ownerships.user_id = @ownerId
  AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER');
