SELECT transfers.id                  AS Id,
	   transfers.created_at          AS CreatedAt,
	   transfers.owner_id            AS OwnerId,
	   transfers.created_by_user_id  AS CreatedByUserId,
	   transfers.modified_at         AS ModifiedAt,
	   transfers.modified_by_user_id AS ModifiedByUserId,
	   transfers.deleted_at          AS DeletedAt,
	   transfers.deleted_by_user_id  AS DeletedByUserId,
	   transfers.transaction_id      AS TransactionId,
	   transfers.source_amount       AS SourceAmount,
	   transfers.source_account_id   AS SourceAccountId,
	   transfers.target_amount       AS TargetAmount,
	   transfers.target_account_id   AS TargetAccountId,
	   transfers.bank_reference      AS BankReference,
	   transfers.external_reference  AS ExternalReference,
	   transfers.internal_reference  AS InternalReference,
	   transfers."order"             AS "Order",
	   transfers.booked_at           AS BookedAt,
	   transfers.valued_at           AS ValuedAt
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
WHERE ((acc_own.user_id = @userId AND (acc_access.normalized_name = @access OR acc_access.normalized_name = 'OWNER')
	AND accounts_in_currency.deleted_at IS NULL
	AND accounts.deleted_at IS NULL)
	OR (tran_own.user_id = @userId AND (tran_acc.normalized_name = @access OR tran_acc.normalized_name = 'OWNER')
		AND transactions.deleted_at IS NULL))
