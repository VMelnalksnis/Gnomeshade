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
		  WHERE (own.user_id = @userId AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER')
			  OR (tran_own.user_id = @userId
				  AND (tran_acc.normalized_name = 'DELETE' OR tran_acc.normalized_name = 'OWNER')
				  AND products.deleted_at IS NULL
				  AND purchases.deleted_at IS NULL
				  AND transactions.deleted_at IS NULL))
			AND categories.deleted_at IS NULL
			AND categories.id = @id),

	 referencing_products AS
		 (SELECT products.id
		  FROM products
				   INNER JOIN categories on products.category_id = categories.id
		  WHERE products.deleted_at IS NULL
			and categories.id = @id),

	 referencing_categories AS
		 (SELECT categories.id
		  FROM categories
		  where categories.deleted_at IS NULL
			and categories.category_id = @id)

UPDATE categories
SET deleted_at         = CURRENT_TIMESTAMP,
	deleted_by_user_id = @userId
FROM accessable
WHERE categories.id IN (SELECT id FROM accessable)
  AND (SELECT count(id) FROM referencing_products) = 0
  AND (SELECT count(id) FROM referencing_categories) = 0;
