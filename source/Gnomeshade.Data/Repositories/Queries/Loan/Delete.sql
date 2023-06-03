WITH accessable AS
		 (SELECT loans.id
		  FROM loans
				   INNER JOIN transactions ON transactions.id = loans.transaction_id
				   LEFT JOIN owners tran_o ON tran_o.id = transactions.owner_id
				   LEFT JOIN ownerships tran_own ON tran_own.owner_id = tran_o.id
				   LEFT JOIN access tran_acc ON tran_own.access_id = tran_acc.id

				   LEFT JOIN transfers ON transactions.id = transfers.transaction_id
				   LEFT JOIN accounts_in_currency on transfers.source_account_id = accounts_in_currency.id OR
													 transfers.target_account_id = accounts_in_currency.id
				   LEFT JOIN accounts ON accounts_in_currency.account_id = accounts.id
				   LEFT JOIN owners accounts_owners ON accounts_owners.id = accounts.owner_id
				   LEFT JOIN ownerships acc_own ON accounts_owners.id = acc_own.owner_id
				   LEFT JOIN access acc_access ON acc_access.id = acc_own.access_id
		  WHERE ((acc_own.user_id = @ownerId
			  AND (acc_access.normalized_name = 'DELETE' OR acc_access.normalized_name = 'OWNER')
			  AND transfers.deleted_at IS NULL
			  AND accounts_in_currency.deleted_at IS NULL
			  AND accounts.deleted_at IS NULL)
			  OR (tran_own.user_id = @ownerId
				  AND (tran_acc.normalized_name = 'DELETE' OR tran_acc.normalized_name = 'OWNER')
				  AND transactions.deleted_at IS NULL))
			AND loans.deleted_at IS NULL
			AND loans.id = @id)

UPDATE loans
SET deleted_at         = CURRENT_TIMESTAMP,
	deleted_by_user_id = @ownerId
FROM accessable
WHERE loans.id IN (SELECT id FROM accessable);
