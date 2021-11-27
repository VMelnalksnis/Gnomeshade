UPDATE transactions
SET modified_at          = DEFAULT,
    modified_by_user_id  = @ModifiedByUserId,
    date                 = @Date,
    description          = @Description,
    import_hash          = @ImportHash,
    imported_at          = @ImportedAt,
    validated_at         = @ValidatedAt,
    validated_by_user_id = @ValidatedByUserId
WHERE id = @Id
RETURNING id;
