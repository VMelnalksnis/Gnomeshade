DELETE
FROM transactions
    USING owners
        INNER JOIN ownerships ON owners.id = ownerships.owner_id
WHERE transactions.id = @id
  AND ownerships.user_id = @ownerId;
