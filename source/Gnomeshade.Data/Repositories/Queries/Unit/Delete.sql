DELETE
FROM units
    USING owners
        INNER JOIN ownerships ON owners.id = ownerships.owner_id
WHERE units.id = @id
  AND ownerships.user_id = @ownerId;
