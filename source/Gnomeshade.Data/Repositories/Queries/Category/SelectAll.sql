﻿SELECT c.id,
	   c.created_at          CreatedAt,
	   c.owner_id            OwnerId,
	   c.created_by_user_id  CreatedByUserId,
	   c.modified_at         ModifiedAt,
	   c.modified_by_user_id ModifiedByUserId,
	   c.name AS             Name,
	   c.normalized_name     NormalizedName,
	   c.description,
	   c.category_id         CategoryId,
	   c.linked_product_id   LinkedProductId
FROM categories c
