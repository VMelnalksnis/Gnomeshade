WITH l AS (SELECT loans.id
		   FROM loans
					INNER JOIN owners ON owners.id = loans.owner_id
					INNER JOIN ownerships ON owners.id = ownerships.owner_id
					INNER JOIN access ON access.id = ownerships.access_id
		   WHERE loans.id = @Id
			 AND ownerships.user_id = @OwnerId
			 AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER'))
UPDATE loans
SET modified_at               = CURRENT_TIMESTAMP,
	modified_by_user_id       = @ModifiedByUserId,
	transaction_id            = @TransactionId,
	issuing_counterparty_id   = @IssuingCounterpartyId,
	receiving_counterparty_id = @ReceivingCounterpartyId,
	amount                    = @Amount,
	currency_id               = @CurrencyId
FROM l
WHERE loans.id IN (SELECT id FROM l)
RETURNING (SELECT id FROM l);
