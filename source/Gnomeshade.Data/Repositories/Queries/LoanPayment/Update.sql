WITH accessable AS
		 (SELECT loan_payments.id
		  FROM loan_payments
				   INNER JOIN loans2 ON loan_payments.loan_id = loans2.id
				   LEFT JOIN owners ON loans2.owner_id = owners.id
				   LEFT JOIN ownerships ON owners.id = ownerships.owner_id
				   LEFT JOIN access ON ownerships.access_id = access.id
		  WHERE (ownerships.user_id = @ModifiedByUserId
			  AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER')
			  AND loans2.deleted_at IS NULL)
			AND loan_payments.deleted_at IS NULL
			AND loan_payments.id = @Id)

UPDATE loan_payments
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = @ModifiedByUserId,
	loan_id             = @LoanId,
	transaction_id      = @TransactionId,
	amount              = @Amount,
	interest            = @Interest
FROM accessable
WHERE loan_payments.id IN (SELECT id FROM accessable);
