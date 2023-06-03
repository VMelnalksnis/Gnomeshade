WITH accessable AS
		 (SELECT a.id,
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
		  WHERE (ownerships.user_id = @userId AND (access.normalized_name = 'READ' OR access.normalized_name = 'OWNER')
			  AND accounts.deleted_at IS NULL)
		    AND a.deleted_at IS NULL
			AND a.account_id = @id),

	 b AS (SELECT accessable.id                    AS AccountInCurrencyId,
				  (source_transfers.source_amount) AS SourceAmount,
				  NULL                             AS TargetAmount
		   FROM accessable
					LEFT JOIN transfers source_transfers ON source_transfers.source_account_id = accessable.id
		   WHERE source_transfers.deleted_at IS NULL
		   UNION ALL
		   SELECT accessable.id                    AS AccountInCurrencyId,
				  NULL                             AS SourceAmount,
				  (target_transfers.target_amount) AS TargetAmount
		   FROM accessable
					LEFT JOIN transfers target_transfers ON target_transfers.target_account_id = accessable.id
		   WHERE target_transfers.deleted_at IS NULL)

SELECT b.AccountInCurrencyId, SUM(b.SourceAmount) AS SourceAmount, SUM(b.TargetAmount) AS TargetAmount
FROM b
GROUP BY b.AccountInCurrencyId;
