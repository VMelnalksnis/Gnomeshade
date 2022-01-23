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
WHERE id = @Id
RETURNING id;
