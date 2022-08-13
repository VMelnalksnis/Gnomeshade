SELECT transfers.id                  AS Id,
	   transfers.created_at          AS CreatedAt,
	   transfers.owner_id            AS OwnerId,
	   transfers.created_by_user_id  AS CreatedByUserId,
	   transfers.modified_at         AS ModifiedAt,
	   transfers.modified_by_user_id AS ModifiedByUserId,
	   transfers.transaction_id      AS TransactionId,
	   transfers.source_amount       AS SourceAmount,
	   transfers.source_account_id   AS SourceAccountId,
	   transfers.target_amount       AS TargetAmount,
	   transfers.target_account_id   AS TargetAccountId,
	   transfers.bank_reference      AS BankReference,
	   transfers.external_reference  AS ExternalReference,
	   transfers.internal_reference  AS InternalReference
FROM transfers
		 INNER JOIN owners ON owners.id = transfers.owner_id
		 INNER JOIN ownerships ON owners.id = ownerships.owner_id
		 INNER JOIN access ON access.id = ownerships.access_id
