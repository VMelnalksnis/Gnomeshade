ALTER TABLE transactions
	ADD COLUMN refunded_by uuid NULL REFERENCES transactions;
