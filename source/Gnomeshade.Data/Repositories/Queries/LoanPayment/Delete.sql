WITH accessable AS
		 (SELECT loan_payments.id
		  FROM loan_payments
				   INNER JOIN loans2 ON loan_payments.loan_id = loans2.id
				   LEFT JOIN owners ON loans2.owner_id = owners.id
				   LEFT JOIN ownerships ON owners.id = ownerships.owner_id
				   LEFT JOIN access ON ownerships.access_id = access.id
		  WHERE (ownerships.user_id = @userId
			  AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER')
			  AND loans2.deleted_at IS NULL)
			AND loan_payments.deleted_at IS NULL
			AND loan_payments.id = @id)

UPDATE loan_payments
SET deleted_at         = CURRENT_TIMESTAMP,
	deleted_by_user_id = @userId
FROM accessable
WHERE loan_payments.id IN (SELECT id FROM accessable);
