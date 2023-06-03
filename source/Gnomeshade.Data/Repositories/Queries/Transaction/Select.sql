﻿SELECT t.id,
	   t.owner_id              OwnerId,
	   t.created_at            CreatedAt,
	   t.created_by_user_id    CreatedByUserId,
	   t.modified_at           ModifiedAt,
	   t.modified_by_user_id   ModifiedByUserId,
	   t.deleted_at            DeletedAt,
	   t.deleted_by_user_id    DeletedByUserId,
	   t.description,
	   t.imported_at           ImportedAt,
	   t.reconciled_at         ReconciledAt,
	   t.reconciled_by_user_id ReconciledByUserId,
	   t.refunded_by           RefundedBy
FROM transactions t
		 INNER JOIN owners o ON o.id = t.owner_id
		 INNER JOIN ownerships own ON o.id = own.owner_id
		 INNER JOIN access acc ON acc.id = own.access_id

		 LEFT JOIN transfers ON t.id = transfers.transaction_id
		 LEFT JOIN accounts_in_currency on transfers.source_account_id = accounts_in_currency.id OR
										   transfers.target_account_id = accounts_in_currency.id
		 LEFT JOIN accounts ON accounts_in_currency.account_id = accounts.id
		 LEFT JOIN owners accounts_owners ON accounts_owners.id = accounts.owner_id
		 LEFT JOIN ownerships acc_own ON accounts_owners.id = acc_own.owner_id
		 LEFT JOIN access acc_acc ON acc_acc.id = acc_own.access_id
WHERE ((own.user_id = @userId AND (acc.normalized_name = @access OR acc.normalized_name = 'OWNER'))
	OR (acc_own.user_id = @userId AND (acc_acc.normalized_name = @access OR acc_acc.normalized_name = 'OWNER')
		AND transfers.deleted_at IS NULL
		AND accounts_in_currency.deleted_at IS NULL
		AND accounts.deleted_at IS NULL))
