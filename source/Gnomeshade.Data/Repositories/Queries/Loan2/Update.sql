WITH accessable AS
		 (SELECT loans2.id
		  FROM loans2
				   INNER JOIN owners ON owners.id = loans2.owner_id
				   INNER JOIN ownerships ON owners.id = ownerships.owner_id
				   INNER JOIN access ON access.id = ownerships.access_id
		  WHERE ownerships.user_id = @ModifiedByUserId
			AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER')
			AND loans2.deleted_at IS NULL
			AND loans2.id = @Id)

UPDATE loans2
SET modified_at               = CURRENT_TIMESTAMP,
	modified_by_user_id       = @ModifiedByUserId,
	owner_id                  = @OwnerId,
	name                      = @Name,
	normalized_name           = upper(@Name),
	issuing_counterparty_id   = @IssuingCounterpartyId,
	receiving_counterparty_id = @ReceivingCounterpartyId,
	principal                 = @Principal,
	currency_id               = @CurrencyId
FROM accessable
WHERE loans2.id IN (SELECT id FROM accessable);
