SELECT c.id,
	   c.created_at          CreatedAt,
	   c.owner_id            OwnerId,
	   c.created_by_user_id  CreatedByUserId,
	   c.modified_at         ModifiedAt,
	   c.modified_by_user_id ModifiedByUserId,
	   c.name AS             Name,
	   c.normalized_name     NormalizedName,
	   c.description,
	   c.category_id         CategoryId,
	   c.linked_product_id   LinkedProductId
FROM categories c
		 INNER JOIN owners o ON o.id = c.owner_id
		 INNER JOIN ownerships own ON o.id = own.owner_id
		 INNER JOIN access ON access.id = own.access_id

		 LEFT JOIN products ON c.id = products.category_id
		 LEFT JOIN purchases ON products.id = purchases.product_id
		 LEFT JOIN transactions ON purchases.transaction_id = transactions.id
		 LEFT JOIN owners transactions_owners ON transactions_owners.id = transactions.owner_id
		 LEFT JOIN ownerships tran_own ON transactions_owners.id = tran_own.owner_id
		 LEFT JOIN access tran_acc ON tran_acc.id = tran_own.access_id
WHERE (own.user_id = @userId AND (access.normalized_name = @access OR access.normalized_name = 'OWNER')
	OR (tran_own.user_id = @userId AND (tran_acc.normalized_name = @access OR tran_acc.normalized_name = 'OWNER')
		AND products.deleted_at IS NULL
		AND purchases.deleted_at IS NULL
		AND transactions.deleted_at IS NULL))
