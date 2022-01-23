INSERT INTO transactions
    (id,
     owner_id,
     created_by_user_id,
     modified_by_user_id,
     date,
     description,
     import_hash,
     imported_at,
     validated_at,
     validated_by_user_id)
VALUES
    (@Id,
     @OwnerId,
     @CreatedByUserId,
     @ModifiedByUserId,
     @Date,
     @Description,
     @ImportHash,
     @ImportedAt,
     @ValidatedAt,
     @ValidatedByUserId)
RETURNING id;
