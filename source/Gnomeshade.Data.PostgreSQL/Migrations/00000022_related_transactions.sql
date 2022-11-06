CREATE TABLE related_transactions
(
	first  uuid NOT NULL,
	second uuid NOT NULL,

	CONSTRAINT related_transactions_first_fkey FOREIGN KEY (first) REFERENCES transactions,
	CONSTRAINT related_transactions_second_fkey FOREIGN KEY (second) REFERENCES transactions
) WITH (OIDS = FALSE);
