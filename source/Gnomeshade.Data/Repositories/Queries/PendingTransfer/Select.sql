SELECT pending_transfers.id         AS Id,
	   pending_transfers.created_at AS CreatedAt,
	   pending_transfers.owner_id   AS OwnerId,
	   created_by_user_id           AS CreatedByUserId,
	   modified_at                  AS ModifiedAt,
	   modified_by_user_id          AS ModifiedByUserId,
	   transaction_id               AS TransactionId,
	   source_amount                AS SourceAmount,
	   source_account_id            AS SourceAccountId,
	   target_counterparty_id       AS TargetCounterpartyId,
	   transfer_id                  AS TransferId
FROM pending_transfers
		 INNER JOIN owners ON owners.id = pending_transfers.owner_id
		 INNER JOIN ownerships ON owners.id = ownerships.owner_id
		 INNER JOIN access ON access.id = ownerships.access_id
