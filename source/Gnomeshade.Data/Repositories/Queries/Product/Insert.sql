﻿INSERT INTO products
    (id, owner_id, created_by_user_id, modified_by_user_id, name, normalized_name, sku, description, unit_id)
VALUES
    (@Id, @OwnerId, @CreatedByUserId, @ModifiedByUserId, @Name, @NormalizedName, @Sku, @Description, @UnitId)
RETURNING id;
