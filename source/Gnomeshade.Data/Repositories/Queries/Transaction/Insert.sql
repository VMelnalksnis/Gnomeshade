INSERT INTO transactions
    (id,
     owner_id,
     created_by_user_id,
     modified_by_user_id,
     description,
     imported_at,
     reconciled_at,
     reconciled_by_user_id,
     refunded_by)
VALUES
    (@Id,
     @OwnerId,
     @CreatedByUserId,
     @ModifiedByUserId,
     @Description,
     @ImportedAt,
     @ReconciledAt,
     @ReconciledByUserId,
     @RefundedBy)
RETURNING id;
