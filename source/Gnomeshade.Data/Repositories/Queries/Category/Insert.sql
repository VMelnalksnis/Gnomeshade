INSERT INTO categories
	(id, owner_id, created_by_user_id, modified_by_user_id, name, normalized_name, description, category_id)
VALUES
	(@Id, @OwnerId, @CreatedByUserId, @ModifiedByUserId, @Name, upper(@Name), @Description, @CategoryId)
RETURNING id;
