ALTER TABLE transfers
	ADD COLUMN booked_at timestamptz NULL;

ALTER TABLE transfers
	ADD COLUMN valued_at timestamptz NULL CHECK (valued_at IS NOT NULL OR booked_at IS NOT NULL);

UPDATE transfers
SET booked_at = transactions.booked_at,
	valued_at = transactions.valued_at
FROM transactions
WHERE transactions.id = transfers.transaction_id;

PRAGMA writable_schema=1;

-- Removes checks from transaction table,
-- as otherwise need to recreate all constrains referencing transactions table
UPDATE sqlite_master
SET sql = REPLACE(
	REPLACE(sql, ' CHECK ( booked_at IS NOT NULL OR valued_at IS NOT NULL)', ''),
	' CHECK ( valued_at IS NOT NULL OR booked_at IS NOT NULL)',
	'')
WHERE type = 'table'
  AND name = 'transactions'
  AND tbl_name = 'transactions';

ALTER TABLE transactions
	DROP COLUMN booked_at;

ALTER TABLE transactions
	DROP COLUMN valued_at;
