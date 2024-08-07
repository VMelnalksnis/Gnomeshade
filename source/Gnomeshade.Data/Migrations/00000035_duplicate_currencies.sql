

-- Updates accounts_in_currency.currency_id
WITH ordered AS (SELECT alphabetic_code, id
				 FROM currencies
				 ORDER BY alphabetic_code, id),

	 duplicates AS (SELECT alphabetic_code
					FROM ordered
					GROUP BY alphabetic_code
					HAVING COUNT(alphabetic_code) > 1),

	 deleteable AS (SELECT c.alphabetic_code, id
					FROM ordered c
					WHERE c.alphabetic_code IN (SELECT duplicates.alphabetic_code FROM duplicates)
					  AND c.id >
						  (SELECT ordered.id FROM ordered WHERE ordered.alphabetic_code = c.alphabetic_code LIMIT 1)),

	 remaining AS (SELECT c.alphabetic_code, id
				   FROM ordered c
				   WHERE c.alphabetic_code IN (SELECT duplicates.alphabetic_code FROM duplicates)
					 AND c.id =
						 (SELECT ordered.id FROM ordered WHERE ordered.alphabetic_code = c.alphabetic_code LIMIT 1))

UPDATE accounts_in_currency
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = get_system_user_id(),
	currency_id         = remaining.id
FROM accounts_in_currency AS a
		 INNER JOIN deleteable ON a.currency_id = deleteable.id
		 INNER JOIN remaining ON remaining.alphabetic_code = deleteable.alphabetic_code
WHERE accounts_in_currency.id = a.id;

-- Updates accounts.preferred_currency_id
WITH ordered AS (SELECT alphabetic_code, id
				 FROM currencies
				 ORDER BY alphabetic_code, id),

	 duplicates AS (SELECT alphabetic_code
					FROM ordered
					GROUP BY alphabetic_code
					HAVING COUNT(alphabetic_code) > 1),

	 deleteable AS (SELECT c.alphabetic_code, id
					FROM ordered c
					WHERE c.alphabetic_code IN (SELECT duplicates.alphabetic_code FROM duplicates)
					  AND c.id >
						  (SELECT ordered.id FROM ordered WHERE ordered.alphabetic_code = c.alphabetic_code LIMIT 1)),

	 remaining AS (SELECT c.alphabetic_code, id
				   FROM ordered c
				   WHERE c.alphabetic_code IN (SELECT duplicates.alphabetic_code FROM duplicates)
					 AND c.id =
						 (SELECT ordered.id FROM ordered WHERE ordered.alphabetic_code = c.alphabetic_code LIMIT 1))

UPDATE accounts
SET modified_at           = CURRENT_TIMESTAMP,
	modified_by_user_id   = get_system_user_id(),
	preferred_currency_id = remaining.id
FROM accounts AS a
		 INNER JOIN deleteable ON a.preferred_currency_id = deleteable.id
		 INNER JOIN remaining ON remaining.alphabetic_code = deleteable.alphabetic_code
WHERE accounts.id = a.id;

-- Updates purchases.currency_id
WITH ordered AS (SELECT alphabetic_code, id
				 FROM currencies
				 ORDER BY alphabetic_code, id),

	 duplicates AS (SELECT alphabetic_code
					FROM ordered
					GROUP BY alphabetic_code
					HAVING COUNT(alphabetic_code) > 1),

	 deleteable AS (SELECT c.alphabetic_code, id
					FROM ordered c
					WHERE c.alphabetic_code IN (SELECT duplicates.alphabetic_code FROM duplicates)
					  AND c.id >
						  (SELECT ordered.id FROM ordered WHERE ordered.alphabetic_code = c.alphabetic_code LIMIT 1)),

	 remaining AS (SELECT c.alphabetic_code, id
				   FROM ordered c
				   WHERE c.alphabetic_code IN (SELECT duplicates.alphabetic_code FROM duplicates)
					 AND c.id =
						 (SELECT ordered.id FROM ordered WHERE ordered.alphabetic_code = c.alphabetic_code LIMIT 1))

UPDATE purchases
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = get_system_user_id(),
	currency_id         = remaining.id
FROM purchases AS p
		 INNER JOIN deleteable ON p.currency_id = deleteable.id
		 INNER JOIN remaining ON remaining.alphabetic_code = deleteable.alphabetic_code
WHERE purchases.id = p.id;

-- Deletes duplicate currencies
WITH ordered AS (SELECT alphabetic_code, id
				 FROM currencies
				 ORDER BY alphabetic_code, id),

	 duplicates AS (SELECT alphabetic_code
					FROM ordered
					GROUP BY alphabetic_code
					HAVING COUNT(alphabetic_code) > 1),

	 deleteable AS (SELECT c.alphabetic_code, id
					FROM ordered c
					WHERE c.alphabetic_code IN (SELECT duplicates.alphabetic_code FROM duplicates)
					  AND c.id >
						  (SELECT ordered.id FROM ordered WHERE ordered.alphabetic_code = c.alphabetic_code LIMIT 1))

UPDATE currencies
SET deleted_at         = CURRENT_TIMESTAMP,
	deleted_by_user_id = get_system_user_id()
WHERE currencies.id IN (SELECT id FROM deleteable);
