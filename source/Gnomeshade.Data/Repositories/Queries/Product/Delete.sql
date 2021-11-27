DELETE
FROM products
    USING owners
        INNER JOIN ownerships ON owners.id = ownerships.owner_id
WHERE products.id = @id
  AND ownerships.user_id = @ownerId;
