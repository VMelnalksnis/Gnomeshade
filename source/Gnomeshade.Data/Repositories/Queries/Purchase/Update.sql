WITH p AS (SELECT purchases.id
		   FROM purchases
					INNER JOIN owners ON owners.id = purchases.owner_id
					INNER JOIN ownerships ON owners.id = ownerships.owner_id
					INNER JOIN access ON access.id = ownerships.access_id
		   WHERE purchases.id = @Id
			 AND ownerships.user_id = @OwnerId
			 AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER'))
UPDATE purchases
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = @ModifiedByUserId,
	transaction_id      = @TransactionId,
	price               = @Price,
	currency_id         = @CurrencyId,
	product_id          = @ProductId,
	amount              = @Amount,
	delivery_date       = @DeliveryDate
FROM p
WHERE purchases.id IN (SELECT id FROM p);
