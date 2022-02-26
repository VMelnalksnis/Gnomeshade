INSERT INTO transaction_item_tags(created_by_user_id, tag_id, tagged_item_id)
SELECT @ownerId, @tagId, transaction_items.id
FROM transaction_items
         INNER JOIN owners ON owners.id = transaction_items.owner_id
         INNER JOIN ownerships ON owners.id = ownerships.owner_id
         INNER JOIN access ON access.id = ownerships.access_id
WHERE transaction_items.id = @id
  AND ownerships.user_id = @ownerId
  AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER');
