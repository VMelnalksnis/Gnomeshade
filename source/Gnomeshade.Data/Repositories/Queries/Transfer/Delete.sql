﻿WITH accessable AS
		 (SELECT transfers.id
		  FROM transfers
				   INNER JOIN transactions ON transactions.id = transfers.transaction_id
				   LEFT JOIN owners tran_o ON tran_o.id = transactions.owner_id
				   LEFT JOIN ownerships tran_own ON tran_own.owner_id = tran_o.id
				   LEFT JOIN access tran_acc ON tran_own.access_id = tran_acc.id

				   LEFT JOIN accounts_in_currency on transfers.source_account_id = accounts_in_currency.id OR
													 transfers.target_account_id = accounts_in_currency.id
				   LEFT JOIN accounts ON accounts_in_currency.account_id = accounts.id
				   LEFT JOIN owners accounts_owners ON accounts_owners.id = accounts.owner_id
				   LEFT JOIN ownerships acc_own ON accounts_owners.id = acc_own.owner_id
				   LEFT JOIN access acc_access ON acc_access.id = acc_own.access_id
		  WHERE ((acc_own.user_id = @userId
			  AND (acc_access.normalized_name = 'DELETE' OR acc_access.normalized_name = 'OWNER')
			  AND accounts_in_currency.deleted_at IS NULL
			  AND accounts.deleted_at IS NULL)
			  OR (tran_own.user_id = @userId
				  AND (tran_acc.normalized_name = 'DELETE' OR tran_acc.normalized_name = 'OWNER')
				  AND transactions.deleted_at IS NULL))
			AND transfers.deleted_at IS NULL
			AND transfers.id = @id)

UPDATE transfers
SET deleted_at         = CURRENT_TIMESTAMP,
	deleted_by_user_id = @userId
FROM accessable
WHERE transfers.id IN (SELECT id FROM accessable);
