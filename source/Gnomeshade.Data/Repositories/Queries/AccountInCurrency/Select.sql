SELECT a.id,
	   a.owner_id            OwnerId,
	   a.created_at          CreatedAt,
	   a.created_by_user_id  CreatedByUserId,
	   a.modified_at         ModifiedAt,
	   a.modified_by_user_id ModifiedByUserId,
	   a.account_id          AccountId,
	   a.currency_id         CurrencyId,
	   a.disabled_at         DisabledAt,
	   a.disabled_by_user_id DisabledByUserId
FROM accounts_in_currency a
		 INNER JOIN owners ON owners.id = a.owner_id
		 INNER JOIN ownerships ON owners.id = ownerships.owner_id
		 INNER JOIN access ON access.id = ownerships.access_id
