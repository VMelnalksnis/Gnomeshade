SELECT purchases.id         AS Id,
       purchases.created_at AS CreatedAt,
       purchases.owner_id   AS OwnerId,
       created_by_user_id   AS CreatedByUserId,
       modified_at          AS ModifiedAt,
       modified_by_user_id  AS ModifiedByUserId,
       transaction_id       AS TransactionId,
       price                AS Price,
       currency_id          AS CurrencyId,
       product_id           AS ProductId,
       amount               AS Amount,
       delivery_date        AS DeliveryDate
FROM purchases
         INNER JOIN owners ON owners.id = purchases.owner_id
         INNER JOIN ownerships ON owners.id = ownerships.owner_id
         INNER JOIN access ON access.id = ownerships.access_id
