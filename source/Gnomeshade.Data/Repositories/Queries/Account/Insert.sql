INSERT INTO accounts
    (owner_id,
     created_by_user_id,
     modified_by_user_id,
     name,
     normalized_name,
     counterparty_id,
     preferred_currency_id,
     disabled_at,
     disabled_by_user_id,
     bic,
     iban,
     account_number)
VALUES
    (@OwnerId,
     @CreatedByUserId,
     @ModifiedByUserId,
     @Name,
     @NormalizedName,
     @CounterpartyId,
     @PreferredCurrencyId,
     @DisabledAt,
     @DisabledByUserId,
     @Bic,
     @Iban,
     @AccountNumber)
RETURNING id;
