INSERT INTO transactions
    (id,
     owner_id,
     created_by_user_id,
     modified_by_user_id,
     booked_at,
     valued_at,
     description,
     import_hash,
     imported_at,
     reconciled_at,
     reconciled_by_user_id)
VALUES
    (@Id,
     @OwnerId,
     @CreatedByUserId,
     @ModifiedByUserId,
     @BookedAt,
     @ValuedAt,
     @Description,
     @ImportHash,
     CASE WHEN @ImportHash IS NULL THEN NULL ELSE CURRENT_TIMESTAMP END,
     @ReconciledAt,
     @ReconciledByUserId)
RETURNING id;
