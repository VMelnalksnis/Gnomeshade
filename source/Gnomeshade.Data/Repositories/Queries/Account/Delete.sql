DELETE
FROM accounts
    USING owners
        INNER JOIN ownerships ON owners.id = ownerships.owner_id
WHERE accounts.id = @id
  AND ownerships.user_id = @ownerId;
