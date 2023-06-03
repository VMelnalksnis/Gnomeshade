WITH accessable AS
		 (SELECT purchases.id
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
		  WHERE ((acc_own.user_id = @ModifiedByUserId AND (acc_acc.normalized_name = 'WRITE' OR acc_acc.normalized_name = 'OWNER')
			  AND transfers.deleted_at IS NULL
			  AND accounts_in_currency.deleted_at IS NULL
			  AND accounts.deleted_at IS NULL)
			  OR (tran_own.user_id = @ModifiedByUserId
				  AND (tran_acc.normalized_name = 'WRITE' OR tran_acc.normalized_name = 'OWNER')
				  AND transactions.deleted_at IS NULL))
		    AND purchases.deleted_at IS NULL
			AND purchases.id = @Id)

UPDATE purchases
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = @ModifiedByUserId,
	transaction_id      = @TransactionId,
	price               = @Price,
	currency_id         = @CurrencyId,
	product_id          = @ProductId,
	amount              = @Amount,
	delivery_date       = @DeliveryDate,
	"order"             = @Order
FROM accessable
WHERE purchases.id IN (SELECT id FROM accessable);
