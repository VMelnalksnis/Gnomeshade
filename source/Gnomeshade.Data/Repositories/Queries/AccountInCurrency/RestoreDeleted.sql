WITH accessable AS
		 (SELECT accounts_in_currency.id
		  FROM accounts_in_currency accounts_in_currency
				   INNER JOIN accounts on accounts.id = accounts_in_currency.account_id
				   INNER JOIN owners ON owners.id = accounts.owner_id
				   INNER JOIN ownerships ON owners.id = ownerships.owner_id
				   INNER JOIN access ON access.id = ownerships.access_id
		  WHERE (ownerships.user_id = @ownerId
			  AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER')
			  AND accounts.deleted_at IS NULL)
			AND accounts_in_currency.deleted_at IS NOT NULL
			AND accounts_in_currency.id = @id)

UPDATE accounts_in_currency
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = @ownerId,
	deleted_at          = NULL,
	deleted_by_user_id  = NULL
FROM accessable
WHERE accounts_in_currency.id IN (SELECT id FROM accessable);
