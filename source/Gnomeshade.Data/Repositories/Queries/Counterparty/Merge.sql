BEGIN TRANSACTION;

WITH a AS (
    SELECT accounts.id
    FROM accounts
             INNER JOIN owners ON owners.id = accounts.owner_id
             INNER JOIN ownerships ON owners.id = ownerships.owner_id
             INNER JOIN access ON access.id = ownerships.access_id
    WHERE accounts.counterparty_id = @sourceId
      AND ownerships.user_id = @ownerId
      AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER')
)
UPDATE accounts
SET modified_at         = DEFAULT,
    modified_by_user_id = @ownerId,
    counterparty_id     = @targetId
FROM a
WHERE accounts.id = a.id;

DELETE
FROM counterparties
    USING access, owners
        INNER JOIN ownerships ON owners.id = ownerships.owner_id
        INNER JOIN access a ON a.id = ownerships.access_id
WHERE counterparties.id = @sourceId
  AND ownerships.user_id = @ownerId
  AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER');

COMMIT TRANSACTION;
