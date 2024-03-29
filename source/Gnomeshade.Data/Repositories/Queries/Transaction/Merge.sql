﻿WITH t AS (SELECT transfers.id
		   FROM transfers
					INNER JOIN owners ON owners.id = transfers.owner_id
					INNER JOIN ownerships ON owners.id = ownerships.owner_id
					INNER JOIN access ON access.id = ownerships.access_id
		   WHERE transfers.transaction_id = @sourceId
			 AND ownerships.user_id = @userId
			 AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER'))
UPDATE transfers
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = @userId,
	transaction_id      = @targetId
FROM t
WHERE transfers.id IN (SELECT id FROM t);

WITH p AS (SELECT purchases.id
		   FROM purchases
					INNER JOIN owners ON owners.id = purchases.owner_id
					INNER JOIN ownerships ON owners.id = ownerships.owner_id
					INNER JOIN access ON access.id = ownerships.access_id
		   WHERE purchases.transaction_id = @sourceId
			 AND ownerships.user_id = @userId
			 AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER'))
UPDATE purchases
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = @userId,
	transaction_id      = @targetId
FROM p
WHERE purchases.id IN (SELECT id FROM p);

WITH l AS (SELECT loans.id
		   FROM loans
					INNER JOIN owners ON owners.id = loans.owner_id
					INNER JOIN ownerships ON owners.id = ownerships.owner_id
					INNER JOIN access ON access.id = ownerships.access_id
		   WHERE loans.transaction_id = @sourceId
			 AND ownerships.user_id = @userId
			 AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER'))
UPDATE loans
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = @userId,
	transaction_id      = @targetId
FROM l
WHERE loans.id IN (SELECT id FROM l);

WITH lp AS (SELECT loan_payments.id
            FROM loan_payments
            	INNER JOIN owners ON loan_payments.owner_id = owners.id
            	INNER JOIN ownerships ON owners.id = ownerships.owner_id
            	INNER JOIN access ON ownerships.access_id = access.id
            WHERE loan_payments.transaction_id = @sourceId
            AND ownerships.user_id = @userId
			  AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER'))
UPDATE loan_payments
SET modified_at = CURRENT_TIMESTAMP,
    modified_by_user_id = @userId,
    transaction_id = @targetId
FROM lp
WHERE loan_payments.id IN (SELECT id from lp);

WITH l AS (SELECT links.id
		   FROM links
					INNER JOIN owners ON owners.id = links.owner_id
					INNER JOIN ownerships ON owners.id = ownerships.owner_id
					INNER JOIN access ON access.id = ownerships.access_id
					INNER JOIN transaction_links tl ON links.id = tl.link_id
		   WHERE tl.transaction_id = @sourceId
			 AND ownerships.user_id = @userId
			 AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER'))
UPDATE transaction_links
SET transaction_id = @targetId
FROM l
WHERE transaction_links.link_id IN (SELECT id FROM l);
