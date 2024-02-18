﻿SELECT loan_payments.id,
	   loan_payments.created_at          CreatedAt,
	   loan_payments.created_by_user_id  CreatedByUserId,
	   loan_payments.deleted_at          DeletedAt,
	   loan_payments.deleted_by_user_id  DeletedByUserId,
	   loan_payments.owner_id            OwnerId,
	   loan_payments.modified_at         ModifiedAt,
	   loan_payments.modified_by_user_id ModifiedByUserId,
	   loan_payments.loan_id             LoanId,
	   loan_payments.transaction_id      TransactionId,
	   loan_payments.amount   AS         Amount,
	   loan_payments.interest AS         Interest
FROM loan_payments
		 INNER JOIN loans2 ON loan_payments.loan_id = loans2.id
		 LEFT JOIN owners ON loans2.owner_id = owners.id
		 LEFT JOIN ownerships ON owners.id = ownerships.owner_id
		 LEFT JOIN access ON ownerships.access_id = access.id
WHERE (ownerships.user_id = @userId
	AND (access.normalized_name = @access OR access.normalized_name = 'OWNER')
	AND loans2.deleted_at IS NULL)
