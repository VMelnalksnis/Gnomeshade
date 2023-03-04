WITH t AS (SELECT transactions.id
		   FROM transactions
					INNER JOIN owners ON owners.id = transactions.owner_id
					INNER JOIN ownerships ON owners.id = ownerships.owner_id
					INNER JOIN access ON access.id = ownerships.access_id
		   WHERE transactions.id = @Id
			 AND ownerships.user_id = @OwnerId
			 AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER'))
UPDATE transactions
SET modified_at           = CURRENT_TIMESTAMP,
	modified_by_user_id   = @ModifiedByUserId,
	booked_at             = @BookedAt,
	valued_at             = @ValuedAt,
	description           = @Description,
	imported_at           = @ImportedAt,
	reconciled_at         = @ReconciledAt,
	reconciled_by_user_id = @ReconciledByUserId,
	refunded_by           = @RefundedBy
FROM t
WHERE transactions.id IN (SELECT id FROM t)
RETURNING (SELECT id FROM t);
