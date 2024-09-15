SELECT projects.id,
	   projects.created_at          CreatedAt,
	   projects.owner_id            OwnerId,
	   projects.created_by_user_id  CreatedByUserId,
	   projects.modified_at         ModifiedAt,
	   projects.modified_by_user_id ModifiedByUserId,
	   projects.name,
	   projects.normalized_name     NormalizedName,
	   projects.parent_project_id   ParentProjectId
FROM projects
		 INNER JOIN owners ON owners.id = projects.owner_id
		 INNER JOIN ownerships ON owners.id = ownerships.owner_id
		 INNER JOIN access ON access.id = ownerships.access_id
WHERE ownerships.user_id = @userId
  AND (access.normalized_name = @access OR access.normalized_name = 'OWNER')
