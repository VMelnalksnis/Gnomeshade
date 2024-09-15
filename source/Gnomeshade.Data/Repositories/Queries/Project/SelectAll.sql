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
