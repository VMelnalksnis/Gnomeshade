INSERT INTO products
    (id, owner_id, created_by_user_id, modified_by_user_id, name, normalized_name, sku, description, unit_id, category_id)
VALUES
    (@Id, @OwnerId, @CreatedByUserId, @ModifiedByUserId, @Name, upper(@Name), @Sku, @Description, @UnitId, @CategoryId)
RETURNING id;
