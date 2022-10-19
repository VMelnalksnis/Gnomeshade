﻿SELECT purchases.id                  AS Id,
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
		 INNER JOIN owners ON owners.id = purchases.owner_id
		 INNER JOIN ownerships ON owners.id = ownerships.owner_id
		 INNER JOIN access ON access.id = ownerships.access_id
