INSERT INTO categories
	(id, owner_id, created_by_user_id, modified_by_user_id, name, normalized_name, description, category_id, linked_product_id)
VALUES
	(@Id, @OwnerId, @CreatedByUserId, @ModifiedByUserId, @Name, upper(@Name), @Description, @CategoryId, @LinkedProductId)
RETURNING id;
