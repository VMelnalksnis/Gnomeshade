﻿WITH a AS (SELECT accounts.id
		   FROM accounts
					INNER JOIN owners ON owners.id = accounts.owner_id
					INNER JOIN ownerships ON owners.id = ownerships.owner_id
					INNER JOIN access ON access.id = ownerships.access_id
		   WHERE accounts.id = @id
			 AND ownerships.user_id = @ownerId
			 AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER'))
UPDATE accounts
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = @ownerId,
	deleted_at          = CURRENT_TIMESTAMP,
	deleted_by_user_id  = @ownerId
FROM a
WHERE accounts.id IN (SELECT id FROM a);