INSERT INTO purchases
    (id,
     owner_id,
     created_by_user_id,
     modified_by_user_id,
     transaction_id,
     price,
     currency_id,
     product_id,
     amount,
     delivery_date,
     "order")
VALUES
    (@Id,
     @OwnerId,
     @CreatedByUserId,
     @ModifiedByUserId,
     @TransactionId,
     @Price,
     @CurrencyId,
     @ProductId,
     @Amount,
     @DeliveryDate,
     @Order)
RETURNING id;
