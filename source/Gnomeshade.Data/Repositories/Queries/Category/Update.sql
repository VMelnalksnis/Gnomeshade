WITH c AS (
	SELECT categories.id
	FROM categories
			 INNER JOIN owners ON owners.id = categories.owner_id
			 INNER JOIN ownerships ON owners.id = ownerships.owner_id
			 INNER JOIN access ON access.id = ownerships.access_id
	WHERE categories.id = @Id
	  AND ownerships.user_id = @OwnerId
	  AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER')
)
UPDATE categories
SET modified_at         = DEFAULT,
	modified_by_user_id = @ModifiedByUserId,
	name                = @Name,
	normalized_name     = @NormalizedName,
	description         = @Description
FROM c
WHERE categories.id = c.id
RETURNING c.id;
