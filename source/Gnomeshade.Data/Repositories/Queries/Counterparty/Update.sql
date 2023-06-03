WITH accessable AS
		 (SELECT counterparties.id
		  FROM counterparties
				   INNER JOIN owners o ON o.id = counterparties.owner_id
				   INNER JOIN ownerships own ON o.id = own.owner_id
				   INNER JOIN access acc ON acc.id = own.access_id

				   LEFT JOIN users on users.id = counterparties.id
				   LEFT JOIN "AspNetUsers" on "AspNetUsers"."Id" = users.id
				   LEFT JOIN accounts on counterparties.id = accounts.counterparty_id
				   LEFT JOIN owners accounts_owners ON accounts_owners.id = accounts.owner_id
				   LEFT JOIN ownerships acc_own ON accounts_owners.id = acc_own.owner_id
				   LEFT JOIN access acc_acc ON acc_acc.id = acc_own.access_id
		  WHERE ((own.user_id = @ModifiedByUserId AND (acc.normalized_name = 'WRITE' OR acc.normalized_name = 'OWNER'))
			  OR (acc_own.user_id = @ModifiedByUserId
				  AND (acc_acc.normalized_name = 'WRITE' OR acc_acc.normalized_name = 'OWNER')
				  AND accounts.deleted_at IS NULL))
			AND counterparties.deleted_at IS NULL
			AND counterparties.id = @Id)

UPDATE counterparties
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = @ModifiedByUserId,
	name                = @Name,
	normalized_name     = upper(@Name)
FROM accessable
WHERE counterparties.id IN (SELECT id FROM accessable);
