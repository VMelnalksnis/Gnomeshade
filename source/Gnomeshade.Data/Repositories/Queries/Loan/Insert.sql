INSERT INTO loans
	(id,
	 owner_id,
	 created_by_user_id,
	 modified_by_user_id,
	 transaction_id,
	 issuing_counterparty_id,
	 receiving_counterparty_id,
	 amount,
	 currency_id)
VALUES
	(@Id,
	 @OwnerId,
	 @CreatedByUserId,
	 @ModifiedByUserId,
	 @TransactionId,
	 @IssuingCounterpartyId,
	 @ReceivingCounterpartyId,
	 @Amount,
	 @CurrencyId)
RETURNING id;
