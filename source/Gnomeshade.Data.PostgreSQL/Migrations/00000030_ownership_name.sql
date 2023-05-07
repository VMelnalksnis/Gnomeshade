ALTER TABLE owners
	ADD COLUMN name            TEXT NULL,
	ADD COLUMN normalized_name TEXT NULL;

UPDATE owners o
SET name            = c.name,
	normalized_name = c.normalized_name
FROM owners
		 INNER JOIN ownerships ON ownerships.id = owners.id
		 INNER JOIN users u ON u.id = ownerships.user_id
		 INNER JOIN counterparties c ON c.id = u.counterparty_id
		 INNER JOIN access a ON a.id = ownerships.access_id
WHERE a.normalized_name = 'OWNER';

ALTER TABLE owners
	ALTER COLUMN name SET NOT NULL,
	ALTER COLUMN normalized_name SET NOT NULL;
