WITH accessable AS
		 (SELECT units.id
		  FROM units
				   INNER JOIN owners units_owners ON units_owners.id = units.owner_id
				   INNER JOIN ownerships unit_own ON units_owners.id = unit_own.owner_id
				   INNER JOIN access Unit_acc ON Unit_acc.id = unit_own.access_id

				   LEFT JOIN products ON units.id = products.unit_id
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
		  WHERE ((unit_own.user_id = @ModifiedByUserId
			  AND (Unit_acc.normalized_name = 'WRITE' OR Unit_acc.normalized_name = 'OWNER'))
			  OR (acc_own.user_id = @ModifiedByUserId
				  AND (acc_acc.normalized_name = 'WRITE' OR acc_acc.normalized_name = 'OWNER')
				  AND products.deleted_at IS NULL
				  AND purchases.deleted_at IS NULL
				  AND transactions.deleted_at IS NULL
				  AND transfers.deleted_at IS NULL
				  AND accounts_in_currency.deleted_at IS NULL
				  AND accounts.deleted_at IS NULL)
			  OR (tran_own.user_id = @ModifiedByUserId
				  AND (tran_acc.normalized_name = 'WRITE' OR tran_acc.normalized_name = 'OWNER')
				  AND purchases.deleted_at IS NULL
				  AND transactions.deleted_at IS NULL))
			AND units.deleted_at IS NULL
			AND units.id = @Id)

UPDATE units
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = @ModifiedByUserId,
	name                = @Name,
	normalized_name     = UPPER(@Name),
	symbol              = @Symbol,
	parent_unit_id      = @ParentUnitId,
	multiplier          = @Multiplier
FROM accessable
WHERE units.id IN (SELECT id FROM accessable);
