ALTER TABLE owners
	ADD COLUMN name TEXT NOT NULL DEFAULT 'foo';
ALTER TABLE owners
	ADD COLUMN normalized_name TEXT NOT NULL DEFAULT 'foo';

UPDATE owners
SET name            = (SELECT c.name
					   FROM owners o
								INNER JOIN ownerships ON ownerships.id = owners.id
								INNER JOIN users u ON u.id = ownerships.user_id
								INNER JOIN counterparties c ON c.id = u.counterparty_id
								INNER JOIN access a ON a.id = ownerships.access_id
					   WHERE o.id = owners.id
						 AND a.normalized_name = 'OWNER'),
	normalized_name = (SELECT c.normalized_name
					   FROM owners o
								INNER JOIN ownerships ON ownerships.id = owners.id
								INNER JOIN users u ON u.id = ownerships.user_id
								INNER JOIN counterparties c ON c.id = u.counterparty_id
								INNER JOIN access a ON a.id = ownerships.access_id
					   WHERE o.id = owners.id
						 AND a.normalized_name = 'OWNER')
WHERE EXISTS(SELECT c.name
			 FROM owners o
					  INNER JOIN ownerships ON ownerships.id = owners.id
					  INNER JOIN users u ON u.id = ownerships.user_id
					  INNER JOIN counterparties c ON c.id = u.counterparty_id
					  INNER JOIN access a ON a.id = ownerships.access_id
			 WHERE o.id = owners.id
			   AND a.normalized_name = 'OWNER');
