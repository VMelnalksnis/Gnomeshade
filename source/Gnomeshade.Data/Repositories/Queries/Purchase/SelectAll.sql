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
	   purchases."order"             AS "Order",
	   project_purchases.project_id  AS "Id"
FROM purchases
		 LEFT JOIN project_purchases ON purchases.id = project_purchases.purchase_id
