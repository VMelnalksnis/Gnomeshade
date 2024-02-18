INSERT INTO loan_payments
	(id,
	 created_by_user_id,
	 owner_id,
	 modified_by_user_id,
	 loan_id,
	 transaction_id,
	 amount,
	 interest)
VALUES
	(@Id,
	 @CreatedByUserId,
	 @OwnerId,
	 @ModifiedByUserId,
	 @LoanId,
	 @TransactionId,
	 @Amount,
	 @Interest)
RETURNING id;
