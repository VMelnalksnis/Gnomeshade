WITH accessable AS
		 (SELECT projects.id
		  FROM projects
				   INNER JOIN owners ON owners.id = projects.owner_id
				   INNER JOIN ownerships ON owners.id = ownerships.owner_id
				   INNER JOIN access ON access.id = ownerships.access_id
		  WHERE ownerships.user_id = @userId
			AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER')
			AND projects.deleted_at IS NULL
			AND projects.id = @id),

	 referencing_projects AS
		 (SELECT projects.id
		  FROM projects
		  where projects.deleted_at IS NULL
			and projects.parent_project_id = @id)

UPDATE projects
SET deleted_at         = CURRENT_TIMESTAMP,
	deleted_by_user_id = @userId
FROM accessable
WHERE projects.id IN (SELECT id FROM accessable)
  AND (SELECT count(id) FROM referencing_projects) = 0;
