SELECT ti.id,
       ti.owner_id         OwnerId,
       transaction_id      TransactionId,
       source_amount       SourceAmount,
       source_account_id   SourceAccountId,
       target_amount       TargetAmount,
       target_account_id   TargetAccountId,
       created_by_user_id  CreatedByUserId,
       modified_by_user_id ModifiedByUserId,
       product_id          ProductId,
       amount,
       bank_reference      BankReference,
       external_reference  ExternalReference,
       internal_reference  InternalReference,
       description,
       delivery_date       DeliveryDate
FROM transaction_items ti
         INNER JOIN owners ON owners.id = ti.owner_id
         INNER JOIN ownerships ON owners.id = ownerships.owner_id
