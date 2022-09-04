WITH t AS (
    SELECT transfers.id
    FROM transfers
             INNER JOIN owners ON owners.id = transfers.owner_id
             INNER JOIN ownerships ON owners.id = ownerships.owner_id
             INNER JOIN access ON access.id = ownerships.access_id
    WHERE transfers.id = @Id
      AND ownerships.user_id = @OwnerId
      AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER')
)
UPDATE transfers
SET modified_at         = CURRENT_TIMESTAMP,
    modified_by_user_id = @ModifiedByUserId,
    transaction_id      = @TransactionId,
    source_amount       = @SourceAmount,
    source_account_id   = @SourceAccountId,
    target_amount       = @TargetAmount,
    target_account_id   = @TargetAccountId,
    bank_reference      = @BankReference,
    external_reference  = @ExternalReference,
    internal_reference  = @InternalReference
FROM t
WHERE transfers.id IN (SELECT id FROM t);
