WITH a AS (SELECT accounts_in_currency.id
		   FROM accounts_in_currency
					INNER JOIN owners ON owners.id = accounts_in_currency.owner_id
					INNER JOIN ownerships ON owners.id = ownerships.owner_id
					INNER JOIN access ON access.id = ownerships.access_id
		   WHERE accounts_in_currency.id = @id
			 AND ownerships.user_id = @ownerId
			 AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER'))
UPDATE accounts_in_currency
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = @ownerId,
	deleted_at          = NULL,
	deleted_by_user_id  = NULL
FROM a
WHERE accounts_in_currency.id IN (SELECT id FROM a);
