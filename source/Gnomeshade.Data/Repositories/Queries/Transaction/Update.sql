WITH t AS (
    SELECT transactions.id
    FROM transactions
             INNER JOIN owners ON owners.id = transactions.owner_id
             INNER JOIN ownerships ON owners.id = ownerships.owner_id
             INNER JOIN access ON access.id = ownerships.access_id
    WHERE transactions.id = @Id
      AND ownerships.user_id = @OwnerId
      AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER')
)
UPDATE transactions
SET modified_at          = DEFAULT,
    modified_by_user_id  = @ModifiedByUserId,
    date                 = @Date,
    description          = @Description,
    import_hash          = @ImportHash,
    imported_at          = @ImportedAt,
    validated_at         = @ValidatedAt,
    validated_by_user_id = @ValidatedByUserId
FROM t
WHERE transactions.id = t.id
RETURNING t.id;
