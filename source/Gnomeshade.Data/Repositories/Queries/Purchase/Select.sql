SELECT purchases.id                  AS Id,
	   purchases.created_at          AS CreatedAt,
	   purchases.owner_id            AS OwnerId,
	   purchases.created_by_user_id  AS CreatedByUserId,
	   purchases.modified_at         AS ModifiedAt,
	   purchases.modified_by_user_id AS ModifiedByUserId,
	   purchases.deleted_at          AS DeletedAt,
	   purchases.deleted_by_user_id  AS DeletedByUaerId,
	   purchases.transaction_id      AS TransactionId,
	   purchases.price               AS Price,
	   purchases.currency_id         AS CurrencyId,
	   purchases.product_id          AS ProductId,
	   purchases.amount              AS Amount,
	   purchases.delivery_date       AS DeliveryDate,
	   purchases."order"             AS "Order"
FROM purchases
		 INNER JOIN transactions ON transactions.id = purchases.transaction_id
		 LEFT JOIN owners tran_o ON tran_o.id = transactions.owner_id
		 LEFT JOIN ownerships tran_own ON tran_own.owner_id = tran_o.id
		 LEFT JOIN access tran_acc ON tran_own.access_id = tran_acc.id

		 LEFT JOIN transfers ON transactions.id = transfers.transaction_id
		 LEFT JOIN accounts_in_currency on transfers.source_account_id = accounts_in_currency.id OR
										   transfers.target_account_id = accounts_in_currency.id
		 LEFT JOIN accounts ON accounts_in_currency.account_id = accounts.id
		 LEFT JOIN owners accounts_owners ON accounts_owners.id = accounts.owner_id
		 LEFT JOIN ownerships acc_own ON accounts_owners.id = acc_own.owner_id
		 LEFT JOIN access acc_acc ON acc_acc.id = acc_own.access_id
WHERE ((acc_own.user_id = @userId AND (acc_acc.normalized_name = @access OR acc_acc.normalized_name = 'OWNER')
	AND transfers.deleted_at IS NULL
	AND accounts_in_currency.deleted_at IS NULL
	AND accounts.deleted_at IS NULL)
	OR (tran_own.user_id = @userId AND (tran_acc.normalized_name = @access OR tran_acc.normalized_name = 'OWNER')
		AND transactions.deleted_at IS NULL))
