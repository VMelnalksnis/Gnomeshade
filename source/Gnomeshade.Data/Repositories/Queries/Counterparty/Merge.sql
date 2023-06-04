WITH accessable AS
		 (SELECT accounts.id
		  FROM accounts
				   INNER JOIN owners ON owners.id = accounts.owner_id
				   INNER JOIN ownerships ON owners.id = ownerships.owner_id
				   INNER JOIN access ON access.id = ownerships.access_id
		  WHERE accounts.counterparty_id = @sourceId
			AND ownerships.user_id = @userId
			AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER'))

UPDATE accounts
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = @userId,
	counterparty_id     = @targetId
FROM accessable
WHERE accounts.id IN (SELECT id FROM accessable);
