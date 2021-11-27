DELETE
FROM transaction_items
    USING owners
        INNER JOIN ownerships ON owners.id = ownerships.owner_id
WHERE transaction_items.id = @id
  AND ownerships.user_id = @ownerId;
