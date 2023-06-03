SELECT a.id,
	   a.owner_id            OwnerId,
	   a.created_at          CreatedAt,
	   a.created_by_user_id  CreatedByUserId,
	   a.modified_at         ModifiedAt,
	   a.modified_by_user_id ModifiedByUserId,
	   a.account_id          AccountId,
	   a.currency_id         CurrencyId
FROM accounts_in_currency a
		 INNER JOIN accounts on accounts.id = a.account_id
		 INNER JOIN owners ON owners.id = accounts.owner_id
		 INNER JOIN ownerships ON owners.id = ownerships.owner_id
		 INNER JOIN access ON access.id = ownerships.access_id
WHERE (ownerships.user_id = @userId AND (access.normalized_name = @access OR access.normalized_name = 'OWNER')
	AND accounts.deleted_at IS NULL)
