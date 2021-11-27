DELETE
FROM counterparties
    USING owners
        INNER JOIN ownerships ON owners.id = ownerships.owner_id
WHERE counterparties.id = @id
  AND ownerships.user_id = @ownerId;
