WITH accessable AS
		 (SELECT accounts.id
		  FROM accounts
				   INNER JOIN owners ON owners.id = accounts.owner_id
				   INNER JOIN ownerships ON owners.id = ownerships.owner_id
				   INNER JOIN access ON access.id = ownerships.access_id
		  WHERE ownerships.user_id = @ModifiedByUserId
			AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER')
			AND accounts.deleted_at IS NULL
			AND accounts.id = @Id)

UPDATE accounts
SET modified_at           = CURRENT_TIMESTAMP,
	modified_by_user_id   = @ModifiedByUserId,
	owner_id              = @OwnerId,
	name                  = @Name,
	normalized_name       = upper(@Name),
	counterparty_id       = @CounterpartyId,
	preferred_currency_id = @PreferredCurrencyId,
	bic                   = @Bic,
	iban                  = @Iban,
	account_number        = @AccountNumber
FROM accessable
WHERE accounts.id IN (SELECT id FROM accessable);
