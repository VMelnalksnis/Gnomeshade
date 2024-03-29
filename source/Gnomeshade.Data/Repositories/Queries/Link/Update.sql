﻿WITH accessable AS
		 (SELECT links.id
		  FROM links
				   LEFT JOIN owners link_o ON link_o.id = links.owner_id
				   LEFT JOIN ownerships link_own ON link_o.id = link_own.owner_id
				   LEFT JOIN access link_acc ON link_acc.id = link_own.access_id

				   LEFT JOIN transaction_links ON links.id = transaction_links.link_id
				   LEFT JOIN transactions ON transactions.id = transaction_links.transaction_id
				   LEFT JOIN owners transactions_owners ON transactions_owners.id = transactions.owner_id
				   LEFT JOIN ownerships tran_own ON tran_own.owner_id = transactions_owners.id
				   LEFT JOIN access tran_acc ON tran_own.access_id = tran_acc.id

				   LEFT JOIN transfers ON transactions.id = transfers.transaction_id
				   LEFT JOIN accounts_in_currency on transfers.source_account_id = accounts_in_currency.id OR
													 transfers.target_account_id = accounts_in_currency.id
				   LEFT JOIN accounts ON accounts_in_currency.account_id = accounts.id
				   LEFT JOIN owners accounts_owners ON accounts_owners.id = accounts.owner_id
				   LEFT JOIN ownerships acc_own ON accounts_owners.id = acc_own.owner_id
				   LEFT JOIN access acc_access ON acc_access.id = acc_own.access_id
		  WHERE ((link_own.user_id = @ModifiedByUserId
			  AND (link_acc.normalized_name = 'WRITE' OR link_acc.normalized_name = 'OWNER'))
			  OR (acc_own.user_id = @ModifiedByUserId
				  AND (acc_access.normalized_name = 'WRITE' OR acc_access.normalized_name = 'OWNER')
				  AND transfers.deleted_at IS NULL
				  AND accounts_in_currency.deleted_at IS NULL
				  AND accounts.deleted_at IS NULL)
			  OR (tran_own.user_id = @ModifiedByUserId
				  AND (tran_acc.normalized_name = 'WRITE' OR tran_acc.normalized_name = 'OWNER')
				  AND transactions.deleted_at IS NULL))
			AND links.deleted_at IS NULL
			AND links.id = @Id)

UPDATE links
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = @ModifiedByUserId,
	uri                 = @Uri
FROM accessable
WHERE links.id IN (SELECT id FROM accessable);
