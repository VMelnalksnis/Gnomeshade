SELECT ti.id,
       ti.created_at          CreatedAt,
       ti.owner_id            OwnerId,
       transaction_id         TransactionId,
       source_amount          SourceAmount,
       source_account_id      SourceAccountId,
       target_amount          TargetAmount,
       target_account_id      TargetAccountId,
       ti.created_by_user_id  CreatedByUserId,
       ti.modified_by_user_id ModifiedByUserId,
       ti.modified_at         ModifiedAt,
       product_id             ProductId,
       amount,
       bank_reference         BankReference,
       external_reference     ExternalReference,
       internal_reference     InternalReference,
       ti.description,
       delivery_date          DeliveryDate
FROM transaction_items ti
         INNER JOIN owners ON owners.id = ti.owner_id
         INNER JOIN ownerships ON owners.id = ownerships.owner_id
         INNER JOIN access ON access.id = ownerships.access_id
