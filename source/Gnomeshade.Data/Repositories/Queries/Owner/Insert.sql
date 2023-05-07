INSERT INTO owners
    (id, name, normalized_name, created_by_user_id, created_at)
VALUES
    (@Id, @Name, upper(@Name), @CreatedByUserId, CURRENT_TIMESTAMP)
RETURNING id;
