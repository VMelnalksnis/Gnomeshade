SELECT c.id,
	   c.created_at            CreatedAt,
	   c.owner_id              OwnerId,
	   c.created_by_user_id    CreatedByUserId,
	   c.modified_at           ModifiedAt,
	   c.modified_by_user_id   ModifiedByUserId,
	   c.name               AS Name,
	   c.normalized_name       NormalizedName,
	   c.deleted_at         AS DeletedAt,
	   c.deleted_by_user_id AS DeletedByUserId
FROM counterparties c
		 INNER JOIN owners o ON o.id = c.owner_id
		 INNER JOIN ownerships own ON o.id = own.owner_id
		 INNER JOIN access acc ON acc.id = own.access_id

		 LEFT JOIN users on users.id = c.id
		 LEFT JOIN "AspNetUsers" on "AspNetUsers"."Id" = users.id
		 LEFT JOIN accounts on c.id = accounts.counterparty_id
		 LEFT JOIN owners accounts_owners ON accounts_owners.id = accounts.owner_id
		 LEFT JOIN ownerships acc_own ON accounts_owners.id = acc_own.owner_id
		 LEFT JOIN access acc_acc ON acc_acc.id = acc_own.access_id
WHERE ((own.user_id = @userId AND (acc.normalized_name = @access OR acc.normalized_name = 'OWNER'))
	OR (acc_own.user_id = @userId AND (acc_acc.normalized_name = @access OR acc_acc.normalized_name = 'OWNER')
		AND accounts.deleted_at IS NULL)
	OR "AspNetUsers"."Id" = c.id)
