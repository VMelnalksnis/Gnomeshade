INSERT INTO accounts
    (id,
     owner_id,
     created_by_user_id,
     modified_by_user_id,
     name,
     normalized_name,
     counterparty_id,
     preferred_currency_id,
     bic,
     iban,
     account_number)
VALUES
    (@Id,
     @OwnerId,
     @CreatedByUserId,
     @ModifiedByUserId,
     @Name,
     upper(@Name),
     @CounterpartyId,
     @PreferredCurrencyId,
     @Bic,
     @Iban,
     @AccountNumber)
RETURNING id;
