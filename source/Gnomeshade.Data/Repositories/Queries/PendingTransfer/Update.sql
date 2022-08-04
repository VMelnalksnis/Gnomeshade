WITH t AS (SELECT pending_transfers.id
		   FROM pending_transfers
					INNER JOIN owners ON owners.id = pending_transfers.owner_id
					INNER JOIN ownerships ON owners.id = ownerships.owner_id
					INNER JOIN access ON access.id = ownerships.access_id
		   WHERE pending_transfers.id = @Id
			 AND ownerships.user_id = @OwnerId
			 AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER'))
UPDATE pending_transfers
SET modified_at            = DEFAULT,
	modified_by_user_id    = @ModifiedByUserId,
	transaction_id         = @TransactionId,
	source_amount          = @SourceAmount,
	source_account_id      = @SourceAccountId,
	target_counterparty_id = @TargetCounterpartyId,
	transfer_id            = @TransferId
FROM t
WHERE pending_transfers.id = t.id;
