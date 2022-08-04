INSERT INTO pending_transfers
	(id,
	 owner_id,
	 created_by_user_id,
	 modified_by_user_id,
	 transaction_id,
	 source_amount,
	 source_account_id,
	 target_counterparty_id,
	 transfer_id)
VALUES
	(@Id,
	 @OwnerId,
	 @CreatedByUserId,
	 @ModifiedByUserId,
	 @TransactionId,
	 @SourceAmount,
	 @SourceAccountId,
	 @TargetCounterpartyId,
	 @TransferId)
RETURNING id;
