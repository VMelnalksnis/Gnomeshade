SELECT loans.id,
	   loans.created_at                CreatedAt,
	   loans.owner_id                  OwnerId,
	   loans.created_by_user_id        CreatedByUserId,
	   loans.modified_at               ModifiedAt,
	   loans.modified_by_user_id       ModifiedByUserId,
	   loans.transaction_id            TransactionId,
	   loans.issuing_counterparty_id   IssuingCounterpartyId,
	   loans.receiving_counterparty_id ReceivingCounterpartyId,
	   loans.amount AS                 Amount,
	   loans.currency_id               CurrencyId,
	   loans.deleted_at                DeletedAt,
	   loans.deleted_by_user_id        DeletedByUserId
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
WHERE ((acc_own.user_id = @userId
	AND (acc_access.normalized_name = @access OR acc_access.normalized_name = 'OWNER')
	AND transfers.deleted_at IS NULL
	AND accounts_in_currency.deleted_at IS NULL
	AND accounts.deleted_at IS NULL)
	OR (tran_own.user_id = @userId
		AND (tran_acc.normalized_name = @access OR tran_acc.normalized_name = 'OWNER')
		AND transactions.deleted_at IS NULL))
