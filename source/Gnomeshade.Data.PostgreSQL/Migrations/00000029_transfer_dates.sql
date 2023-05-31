ALTER TABLE transfers
	ADD COLUMN booked_at timestamptz NULL,
	ADD COLUMN valued_at timestamptz NULL;

UPDATE transfers
SET booked_at = transactions.booked_at,
	valued_at = transactions.valued_at
FROM transactions
WHERE transactions.id = transfers.transaction_id;

ALTER TABLE transactions
	DROP CONSTRAINT transactions_one_of_dates_required,
	DROP COLUMN booked_at,
	DROP COLUMN valued_at;

ALTER TABLE transfers
	ADD CONSTRAINT transfers_one_of_dates_required CHECK (num_nonnulls(booked_at, valued_at) > 0);
