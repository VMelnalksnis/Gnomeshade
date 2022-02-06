WITH a AS (
    SELECT accounts.id
    FROM accounts
             INNER JOIN owners ON owners.id = accounts.owner_id
             INNER JOIN ownerships ON owners.id = ownerships.owner_id
             INNER JOIN access ON access.id = ownerships.access_id
    WHERE accounts.id = @Id
      AND ownerships.user_id = @OwnerId
      AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER')
)
UPDATE accounts
SET modified_at           = DEFAULT,
    modified_by_user_id   = @ModifiedByUserId,
    name                  = @Name,
    normalized_name       = @NormalizedName,
    counterparty_id       = @CounterpartyId,
    preferred_currency_id = @PreferredCurrencyId,
    disabled_at           = @DisabledAt,
    disabled_by_user_id   = @DisabledByUserId,
    bic                   = @Bic,
    iban                  = @Iban,
    account_number        = @AccountNumber
FROM a
WHERE accounts.id = a.id
RETURNING a.id;
