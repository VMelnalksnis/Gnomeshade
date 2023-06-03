WITH accessable AS
		 (SELECT transactions.id
		  FROM transactions
				   INNER JOIN owners o ON o.id = transactions.owner_id
				   INNER JOIN ownerships own ON o.id = own.owner_id
				   INNER JOIN access acc ON acc.id = own.access_id

				   LEFT JOIN transfers ON transactions.id = transfers.transaction_id
				   LEFT JOIN accounts_in_currency on transfers.source_account_id = accounts_in_currency.id OR
													 transfers.target_account_id = accounts_in_currency.id
				   LEFT JOIN accounts ON accounts_in_currency.account_id = accounts.id
				   LEFT JOIN owners accounts_owners ON accounts_owners.id = accounts.owner_id
				   LEFT JOIN ownerships acc_own ON accounts_owners.id = acc_own.owner_id
				   LEFT JOIN access acc_acc ON acc_acc.id = acc_own.access_id
		  WHERE ((own.user_id = @ownerId AND (acc.normalized_name = 'DELETE' OR acc.normalized_name = 'OWNER'))
			  OR (acc_own.user_id = @ownerId
				  AND (acc_acc.normalized_name = 'DELETE' OR acc_acc.normalized_name = 'OWNER')
				  AND transfers.deleted_at IS NULL
				  AND accounts_in_currency.deleted_at IS NULL
				  AND accounts.deleted_at IS NULL))
			AND transactions.deleted_at IS NULL
			AND transactions.id = @id)

UPDATE transactions
SET deleted_at         = CURRENT_TIMESTAMP,
	deleted_by_user_id = @ownerId
FROM accessable
WHERE transactions.id IN (SELECT id FROM accessable);
