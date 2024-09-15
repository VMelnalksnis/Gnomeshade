WITH accessable AS
		 (SELECT projects.id
		  FROM projects
				   INNER JOIN owners ON owners.id = projects.owner_id
				   INNER JOIN ownerships ON owners.id = ownerships.owner_id
				   INNER JOIN access ON access.id = ownerships.access_id
		  WHERE ownerships.user_id = @ModifiedByUserId
			AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER')
			AND projects.deleted_at IS NULL
			AND projects.id = @Id)

UPDATE projects
SET modified_at         = CURRENT_TIMESTAMP,
	modified_by_user_id = @ModifiedByUserId,
	name                = @Name,
	normalized_name     = upper(@Name),
	parent_project_id   = @ParentProjectId
FROM accessable
WHERE projects.id IN (SELECT id FROM accessable);
