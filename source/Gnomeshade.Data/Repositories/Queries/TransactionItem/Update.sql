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
WHERE transaction_items.id = @Id;
