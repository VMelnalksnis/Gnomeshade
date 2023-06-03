SELECT a.id,
	   a.owner_id            OwnerId,
	   a.created_at          CreatedAt,
	   a.created_by_user_id  CreatedByUserId,
	   a.modified_at         ModifiedAt,
	   a.modified_by_user_id ModifiedByUserId,
	   a.account_id          AccountId,
	   a.currency_id         CurrencyId
FROM accounts_in_currency a
