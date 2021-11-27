INSERT INTO accounts_in_currency
    (owner_id, created_by_user_id, modified_by_user_id, account_id, currency_id, disabled_at, disabled_by_user_id)
VALUES
    (@OwnerId, @CreatedByUserId, @ModifiedByUserId, @AccountId, @CurrencyId, @DisabledAt, @DisabledByUserId)
RETURNING id;
