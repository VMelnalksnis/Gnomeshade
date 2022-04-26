WITH a AS (
	SELECT accounts_in_currency.id
	FROM accounts_in_currency
			 INNER JOIN owners ON owners.id = accounts_in_currency.owner_id
			 INNER JOIN ownerships ON owners.id = ownerships.owner_id
			 INNER JOIN access ON access.id = ownerships.access_id
	WHERE account_id = @id
	  AND ownerships.user_id = @ownerId
	  AND (access.normalized_name = 'READ' OR access.normalized_name = 'OWNER')),

	 b AS (SELECT a.id                             AS AccountInCurrencyId,
				  (source_transfers.source_amount) AS SourceAmount,
				  NULL                             AS TargetAmount
		   FROM a
					LEFT JOIN transfers source_transfers ON source_transfers.source_account_id = a.id
		   UNION
		   SELECT a.id                             AS AccountInCurrencyId,
				  NULL                             AS SourceAmount,
				  (target_transfers.target_amount) AS TargetAmount
		   FROM a
					LEFT JOIN transfers target_transfers ON target_transfers.target_account_id = a.id)

SELECT b.AccountInCurrencyId, SUM(b.SourceAmount) AS SourceAmount, SUM(b.TargetAmount) AS TargetAmount
FROM b
GROUP BY b.AccountInCurrencyId;
