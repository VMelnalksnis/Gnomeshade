﻿SELECT o.id AS Id,
	   o.created_at         AS CreatedAt,
	   o.created_by_user_id AS CreatedByUserId,
	   o.deleted_at         AS DeletedAt,
	   o.deleted_by_user_id AS DeletedByUserId,
	   o.name,
	   o.normalized_name    AS NormalizedName
FROM owners o
