SELECT transactions.id,
	   transactions.owner_id              OwnerId,
	   transactions.created_at            CreatedAt,
	   transactions.created_by_user_id    CreatedByUserId,
	   transactions.modified_at           ModifiedAt,
	   transactions.modified_by_user_id   ModifiedByUserId,
	   transactions.deleted_at            DeletedAt,
	   transactions.deleted_by_user_id    DeletedByUserId,
	   transactions.description,
	   transactions.imported_at           ImportedAt,
	   transactions.reconciled_at         ReconciledAt,
	   transactions.reconciled_by_user_id ReconciledByUserId,
	   transactions.refunded_by           RefundedBy,
	   purchases.id                  AS   Id,
	   purchases.created_at          AS   CreatedAt,
	   purchases.owner_id            AS   OwnerId,
	   purchases.created_by_user_id  AS   CreatedByUserId,
	   purchases.modified_at         AS   ModifiedAt,
	   purchases.modified_by_user_id AS   ModifiedByUserId,
	   purchases.deleted_at          AS   DeletedAt,
	   purchases.deleted_by_user_id  AS   DeletedByUaerId,
	   purchases.transaction_id      AS   TransactionId,
	   purchases.price               AS   Price,
	   purchases.currency_id         AS   CurrencyId,
	   purchases.product_id          AS   ProductId,
	   purchases.amount              AS   Amount,
	   purchases.delivery_date       AS   DeliveryDate,
	   purchases."order"             AS   "Order",
	   project_purchases.project_id  AS   "Id",
	   transfers.id                  AS   Id,
	   transfers.created_at          AS   CreatedAt,
	   transfers.owner_id            AS   OwnerId,
	   transfers.created_by_user_id  AS   CreatedByUserId,
	   transfers.modified_at         AS   ModifiedAt,
	   transfers.modified_by_user_id AS   ModifiedByUserId,
	   transfers.deleted_at          AS   DeletedAt,
	   transfers.deleted_by_user_id  AS   DeletedByUserId,
	   transfers.transaction_id      AS   TransactionId,
	   transfers.source_amount       AS   SourceAmount,
	   transfers.source_account_id   AS   SourceAccountId,
	   transfers.target_amount       AS   TargetAmount,
	   transfers.target_account_id   AS   TargetAccountId,
	   transfers.bank_reference      AS   BankReference,
	   transfers.external_reference  AS   ExternalReference,
	   transfers.internal_reference  AS   InternalReference,
	   transfers."order"             AS   "Order",
	   transfers.booked_at                BookedAt,
	   transfers.valued_at                ValuedAt,
	   loan_payments.id,
	   loan_payments.created_at           CreatedAt,
	   loan_payments.created_by_user_id   CreatedByUserId,
	   loan_payments.deleted_at           DeletedAt,
	   loan_payments.deleted_by_user_id   DeletedByUserId,
	   loan_payments.owner_id             OwnerId,
	   loan_payments.modified_at          ModifiedAt,
	   loan_payments.modified_by_user_id  ModifiedByUserId,
	   loan_payments.loan_id              LoanId,
	   loan_payments.transaction_id       TransactionId,
	   loan_payments.amount          AS   Amount,
	   loan_payments.interest        AS   Interest,
	   links.id                      AS   Id,
	   links.created_at                   CreatedAt,
	   links.created_by_user_id           CreatedByUserId,
	   links.owner_id                     OwnerId,
	   links.modified_at                  ModifiedAt,
	   links.modified_by_user_id          ModifiedByUserId,
	   links.uri                     AS   Uri,
	   links.deleted_at              AS   DeletedAt,
	   links.deleted_by_user_id      AS   DeletedByUserId
FROM transactions
		 LEFT JOIN purchases ON purchases.transaction_id = transactions.id
		 LEFT JOIN project_purchases ON purchases.id = project_purchases.purchase_id
		 LEFT JOIN transfers ON transfers.transaction_id = transactions.id
		 LEFT JOIN loan_payments ON loan_payments.transaction_id = transactions.id
		 LEFT JOIN transaction_links ON transaction_links.transaction_id = transactions.id
		 LEFT JOIN links ON links.id = transaction_links.link_id

		 INNER JOIN owners ON owners.id = transactions.owner_id
		 INNER JOIN ownerships ON owners.id = ownerships.owner_id
		 INNER JOIN access ON access.id = ownerships.access_id

		 LEFT JOIN accounts_in_currency on transfers.source_account_id = accounts_in_currency.id OR
										   transfers.target_account_id = accounts_in_currency.id
		 LEFT JOIN accounts ON accounts_in_currency.account_id = accounts.id
		 LEFT JOIN owners accounts_owners ON accounts_owners.id = accounts.owner_id
		 LEFT JOIN ownerships acc_own ON accounts_owners.id = acc_own.owner_id
		 LEFT JOIN access acc_acc ON acc_acc.id = acc_own.access_id
WHERE (((ownerships.user_id = @userId AND (access.normalized_name = @access OR access.normalized_name = 'OWNER'))
	OR (acc_own.user_id = @userId AND (acc_acc.normalized_name = @access OR acc_acc.normalized_name = 'OWNER') AND
		transfers.deleted_at IS NULL AND
		accounts_in_currency.deleted_at IS NULL AND
		accounts.deleted_at IS NULL))
	AND transactions.deleted_at IS NULL)
