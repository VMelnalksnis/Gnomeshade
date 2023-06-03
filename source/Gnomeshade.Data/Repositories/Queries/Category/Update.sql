WITH accessable AS
		 (SELECT categories.id
		  FROM categories
				   INNER JOIN owners o ON o.id = categories.owner_id
				   INNER JOIN ownerships own ON o.id = own.owner_id
				   INNER JOIN access ON access.id = own.access_id

				   LEFT JOIN products ON categories.id = products.category_id
				   LEFT JOIN purchases ON products.id = purchases.product_id
				   LEFT JOIN transactions ON purchases.transaction_id = transactions.id
				   LEFT JOIN owners transactions_owners ON transactions_owners.id = transactions.owner_id
				   LEFT JOIN ownerships tran_own ON transactions_owners.id = tran_own.owner_id
				   LEFT JOIN access tran_acc ON tran_acc.id = tran_own.access_id
		  WHERE (own.user_id = @ModifiedByUserId AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER')
			  OR (tran_own.user_id = @ModifiedByUserId
				  AND (tran_acc.normalized_name = 'WRITE' OR tran_acc.normalized_name = 'OWNER')
				  AND products.deleted_at IS NULL
				  AND purchases.deleted_at IS NULL
				  AND transactions.deleted_at IS NULL))
			AND categories.deleted_at IS NULL
			AND categories.id = @Id)

UPDATE categories
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = @ModifiedByUserId,
	name                = @Name,
	normalized_name     = UPPER(@Name),
	description         = @Description,
	category_id         = @CategoryId,
	linked_product_id   = @LinkedProductId
FROM accessable
WHERE categories.id IN (SELECT id FROM accessable);
