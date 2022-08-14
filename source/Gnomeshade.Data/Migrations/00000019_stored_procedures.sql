CREATE OR REPLACE PROCEDURE delete_account(id uuid, owner_id uuid)
	LANGUAGE sql
AS
$$
WITH a AS (SELECT accounts.id
		   FROM accounts
					INNER JOIN owners ON owners.id = accounts.owner_id
					INNER JOIN ownerships ON owners.id = ownerships.owner_id
					INNER JOIN access ON access.id = ownerships.access_id
		   WHERE accounts.id = $1
			 AND ownerships.user_id = $2
			 AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER'))
UPDATE accounts
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = $2,
	deleted_at          = CURRENT_TIMESTAMP,
	deleted_by_user_id  = $2
FROM a
WHERE accounts.id = a.id;
$$;

CREATE OR REPLACE PROCEDURE delete_account_in_currency(id uuid, owner_id uuid)
	LANGUAGE sql
AS
$$
WITH a AS (SELECT accounts_in_currency.id
		   FROM accounts_in_currency
					INNER JOIN owners ON owners.id = accounts_in_currency.owner_id
					INNER JOIN ownerships ON owners.id = ownerships.owner_id
					INNER JOIN access ON access.id = ownerships.access_id
		   WHERE accounts_in_currency.id = $1
			 AND ownerships.user_id = $2
			 AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER'))
UPDATE accounts_in_currency
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = $2,
	deleted_at          = CURRENT_TIMESTAMP,
	deleted_by_user_id  = $2
FROM a
WHERE accounts_in_currency.id = a.id;
$$;

CREATE OR REPLACE PROCEDURE delete_category(id uuid, owner_id uuid)
	LANGUAGE sql
AS
$$
WITH c AS (SELECT categories.id
		   FROM categories
					INNER JOIN owners ON owners.id = categories.owner_id
					INNER JOIN ownerships ON owners.id = ownerships.owner_id
					INNER JOIN access ON access.id = ownerships.access_id
		   WHERE categories.id = $1
			 AND ownerships.user_id = $2
			 AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER'))
UPDATE categories
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = $2,
	deleted_at          = CURRENT_TIMESTAMP,
	deleted_by_user_id  = $2
FROM c
WHERE categories.id = c.id;
$$;

CREATE OR REPLACE PROCEDURE delete_counterparty(id uuid, owner_id uuid)
	LANGUAGE sql
AS
$$
WITH c AS (SELECT counterparties.id
		   FROM counterparties
					INNER JOIN owners ON owners.id = counterparties.owner_id
					INNER JOIN ownerships ON owners.id = ownerships.owner_id
					INNER JOIN access ON access.id = ownerships.access_id
		   WHERE counterparties.id = $1
			 AND ownerships.user_id = $2
			 AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER'))
UPDATE counterparties
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = $2,
	deleted_at          = CURRENT_TIMESTAMP,
	deleted_by_user_id  = $2
FROM c
WHERE counterparties.id = c.id;
$$;

CREATE OR REPLACE PROCEDURE delete_link(id uuid, owner_id uuid)
	LANGUAGE sql
AS
$$
WITH l AS (SELECT links.id
		   FROM links
					INNER JOIN owners ON owners.id = links.owner_id
					INNER JOIN ownerships ON owners.id = ownerships.owner_id
					INNER JOIN access ON access.id = ownerships.access_id
		   WHERE links.id = $1
			 AND ownerships.user_id = $2
			 AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER'))
UPDATE links
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = $2,
	deleted_at          = CURRENT_TIMESTAMP,
	deleted_by_user_id  = $2
FROM l
WHERE links.id = l.id;
$$;

CREATE OR REPLACE PROCEDURE delete_loan(id uuid, owner_id uuid)
	LANGUAGE sql
AS
$$
WITH l AS (SELECT loans.id
		   FROM loans
					INNER JOIN owners ON owners.id = loans.owner_id
					INNER JOIN ownerships ON owners.id = ownerships.owner_id
					INNER JOIN access ON access.id = ownerships.access_id
		   WHERE loans.id = $1
			 AND ownerships.user_id = $2
			 AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER'))
UPDATE loans
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = $2,
	deleted_at          = CURRENT_TIMESTAMP,
	deleted_by_user_id  = $2
FROM l
WHERE loans.id = l.id;
$$;

CREATE OR REPLACE PROCEDURE delete_product(id uuid, owner_id uuid)
	LANGUAGE sql
AS
$$
WITH p AS (SELECT products.id
		   FROM products
					INNER JOIN owners ON owners.id = products.owner_id
					INNER JOIN ownerships ON owners.id = ownerships.owner_id
					INNER JOIN access ON access.id = ownerships.access_id
		   WHERE products.id = $1
			 AND ownerships.user_id = $2
			 AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER'))
UPDATE products
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = $2,
	deleted_at          = CURRENT_TIMESTAMP,
	deleted_by_user_id  = $2
FROM p
WHERE products.id = p.id;
$$;

CREATE OR REPLACE PROCEDURE delete_purchase(id uuid, owner_id uuid)
	LANGUAGE sql
AS
$$
WITH p AS (SELECT purchases.id
		   FROM purchases
					INNER JOIN owners ON owners.id = purchases.owner_id
					INNER JOIN ownerships ON owners.id = ownerships.owner_id
					INNER JOIN access ON access.id = ownerships.access_id
		   WHERE purchases.id = $1
			 AND ownerships.user_id = $2
			 AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER'))
UPDATE purchases
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = $2,
	deleted_at          = CURRENT_TIMESTAMP,
	deleted_by_user_id  = $2
FROM p
WHERE purchases.id = p.id;
$$;

CREATE OR REPLACE PROCEDURE delete_transaction(id uuid, owner_id uuid)
	LANGUAGE sql
AS
$$
WITH t AS (SELECT transactions.id
		   FROM transactions
					INNER JOIN owners ON owners.id = transactions.owner_id
					INNER JOIN ownerships ON owners.id = ownerships.owner_id
					INNER JOIN access ON access.id = ownerships.access_id
		   WHERE transactions.id = $1
			 AND ownerships.user_id = $2
			 AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER'))
UPDATE transactions
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = $2,
	deleted_at          = CURRENT_TIMESTAMP,
	deleted_by_user_id  = $2
FROM t
WHERE transactions.id = t.id;
$$;

CREATE OR REPLACE PROCEDURE delete_transfer(id uuid, owner_id uuid)
	LANGUAGE sql
AS
$$
WITH t AS (SELECT transfers.id
		   FROM transfers
					INNER JOIN owners ON owners.id = transfers.owner_id
					INNER JOIN ownerships ON owners.id = ownerships.owner_id
					INNER JOIN access ON access.id = ownerships.access_id
		   WHERE transfers.id = $1
			 AND ownerships.user_id = $2
			 AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER'))
UPDATE transfers
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = $2,
	deleted_at          = CURRENT_TIMESTAMP,
	deleted_by_user_id  = $2
FROM t
WHERE transfers.id = t.id;
$$;

CREATE OR REPLACE PROCEDURE delete_unit(id uuid, owner_id uuid)
	LANGUAGE sql
AS
$$
WITH u AS (SELECT units.id
		   FROM units
					INNER JOIN owners ON owners.id = units.owner_id
					INNER JOIN ownerships ON owners.id = ownerships.owner_id
					INNER JOIN access ON access.id = ownerships.access_id
		   WHERE units.id = $1
			 AND ownerships.user_id = $2
			 AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER'))
UPDATE units
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = $2,
	deleted_at          = CURRENT_TIMESTAMP,
	deleted_by_user_id  = $2
FROM u
WHERE units.id = u.id;
$$;
