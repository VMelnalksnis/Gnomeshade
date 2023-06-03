SELECT u.id,
	   u.created_at            CreatedAt,
	   u.owner_id              OwnerId,
	   u.created_by_user_id    CreatedByUserId,
	   u.modified_at           ModifiedAt,
	   u.modified_by_user_id   ModifiedByUserId,
	   u.name               AS Name,
	   u.normalized_name       NormalizedName,
	   u.symbol             AS Symbol,
	   u.deleted_at         AS DeletedAt,
	   u.deleted_by_user_id AS DeletedByUserId,
	   u.parent_unit_id        ParentUnitId,
	   u.multiplier
FROM units u
		 INNER JOIN owners units_owners ON units_owners.id = u.owner_id
		 INNER JOIN ownerships unit_own ON units_owners.id = unit_own.owner_id
		 INNER JOIN access Unit_acc ON Unit_acc.id = unit_own.access_id

		 LEFT JOIN products ON u.id = products.unit_id
		 LEFT JOIN purchases ON products.id = purchases.product_id
		 LEFT JOIN transactions ON transactions.id = purchases.transaction_id
		 LEFT JOIN owners transactions_owners ON transactions_owners.id = transactions.owner_id
		 LEFT JOIN ownerships tran_own ON tran_own.owner_id = transactions_owners.id
		 LEFT JOIN access tran_acc ON tran_own.access_id = tran_acc.id

		 LEFT JOIN transfers ON transactions.id = transfers.transaction_id
		 LEFT JOIN accounts_in_currency on transfers.source_account_id = accounts_in_currency.id OR
										   transfers.target_account_id = accounts_in_currency.id
		 LEFT JOIN accounts ON accounts_in_currency.account_id = accounts.id
		 LEFT JOIN owners accounts_owners ON accounts_owners.id = accounts.owner_id
		 LEFT JOIN ownerships acc_own ON accounts_owners.id = acc_own.owner_id
		 LEFT JOIN access acc_acc ON acc_acc.id = acc_own.access_id
WHERE ((unit_own.user_id = @userId AND (Unit_acc.normalized_name = @access OR Unit_acc.normalized_name = 'OWNER'))
	OR (acc_own.user_id = @userId AND (acc_acc.normalized_name = @access OR acc_acc.normalized_name = 'OWNER')
		AND products.deleted_at IS NULL
		AND purchases.deleted_at IS NULL
		AND transactions.deleted_at IS NULL
		AND transfers.deleted_at IS NULL
		AND accounts_in_currency.deleted_at IS NULL
		AND accounts.deleted_at IS NULL)
	OR (tran_own.user_id = @userId AND (tran_acc.normalized_name = @access OR tran_acc.normalized_name = 'OWNER')
		AND purchases.deleted_at IS NULL
		AND transactions.deleted_at IS NULL))
