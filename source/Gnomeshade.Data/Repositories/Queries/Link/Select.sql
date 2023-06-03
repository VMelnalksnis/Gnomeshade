SELECT links.id,
	   links.created_at            CreatedAt,
	   links.created_by_user_id    CreatedByUserId,
	   links.owner_id              OwnerId,
	   links.modified_at           ModifiedAt,
	   links.modified_by_user_id   ModifiedByUserId,
	   links.uri                AS Uri,
	   links.deleted_at         AS DeletedAt,
	   links.deleted_by_user_id AS DeletedByUserId
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
WHERE ((link_own.user_id = @userId AND (link_acc.normalized_name = @access OR link_acc.normalized_name = 'OWNER'))
	OR (acc_own.user_id = @userId AND (acc_access.normalized_name = @access OR acc_access.normalized_name = 'OWNER')
		AND transfers.deleted_at IS NULL
		AND accounts_in_currency.deleted_at IS NULL
		AND accounts.deleted_at IS NULL)
	OR (tran_own.user_id = @userId AND (tran_acc.normalized_name = @access OR tran_acc.normalized_name = 'OWNER')
		AND transactions.deleted_at IS NULL))
