﻿SELECT t.id,
	   t.owner_id              OwnerId,
	   t.created_at            CreatedAt,
	   t.created_by_user_id    CreatedByUserId,
	   t.modified_at           ModifiedAt,
	   t.modified_by_user_id   ModifiedByUserId,
	   t.deleted_at            DeletedAt,
	   t.deleted_by_user_id    DeletedByUserId,
	   t.description,
	   t.imported_at           ImportedAt,
	   t.reconciled_at         ReconciledAt,
	   t.reconciled_by_user_id ReconciledByUserId,
	   t.refunded_by           RefundedBy
FROM transactions t
