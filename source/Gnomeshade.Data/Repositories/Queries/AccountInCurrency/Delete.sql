DELETE
FROM accounts_in_currency
    USING owners
        INNER JOIN ownerships ON owners.id = ownerships.owner_id
WHERE accounts_in_currency.id = @id
  AND ownerships.user_id = @ownerId;
