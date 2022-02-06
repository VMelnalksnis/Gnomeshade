WITH t AS (
    SELECT transaction_items.id
    FROM transaction_items
             INNER JOIN owners ON owners.id = transaction_items.owner_id
             INNER JOIN ownerships ON owners.id = ownerships.owner_id
             INNER JOIN access ON access.id = ownerships.access_id
    WHERE transaction_items.id = @Id
      AND ownerships.user_id = @OwnerId
      AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER')
)
UPDATE transaction_items
SET modified_at         = DEFAULT,
    modified_by_user_id = @ModifiedByUserId,
    transaction_id      = @TransactionId,
    source_amount       = @SourceAmount,
    source_account_id   = @SourceAccountId,
    target_amount       = @TargetAmount,
    target_account_id   = @TargetAccountId,
    product_id          = @ProductId,
    amount              = @Amount,
    bank_reference      = @BankReference,
    external_reference  = @ExternalReference,
    internal_reference  = @InternalReference,
    delivery_date       = @DeliveryDate,
    description         = @Description
FROM t
WHERE transaction_items.id = t.id
RETURNING t.id;
