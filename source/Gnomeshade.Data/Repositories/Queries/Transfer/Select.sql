SELECT transfers.id         AS Id,
       transfers.created_at AS CreatedAt,
       transfers.owner_id   AS OwnerId,
       created_by_user_id   AS CreatedByUserId,
       modified_at          AS ModifiedAt,
       modified_by_user_id  AS ModifiedByUserId,
       transaction_id       AS TransactionId,
       source_amount        AS SourceAmount,
       source_account_id    AS SourceAccountId,
       target_amount        AS TargetAmount,
       target_account_id    AS TargetAccountId,
       bank_reference       AS BankReference,
       external_reference   AS ExternalReference,
       internal_reference   AS InternalReference
FROM transfers
         INNER JOIN owners ON owners.id = transfers.owner_id
         INNER JOIN ownerships ON owners.id = ownerships.owner_id
         INNER JOIN access ON access.id = ownerships.access_id
