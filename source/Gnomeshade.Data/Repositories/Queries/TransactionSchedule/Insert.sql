INSERT INTO projects
(id,
 created_at,
 created_by_user_id,
 deleted_at,
 deleted_by_user_id,
 owner_id,
 modified_at,
 modified_by_user_id,
 name,
 normalized_name,
 parent_project_id)
VALUES (@Id,
		@CreatedAt,
		@CreatedByUserId,
		@DeletedAt,
		@DeletedByUserId,
		@OwnerId,
		@ModifiedAt,
		@ModifiedByUserId,
		@Name,
		upper(@Name),
		@ParentProjectId)
RETURNING id;
