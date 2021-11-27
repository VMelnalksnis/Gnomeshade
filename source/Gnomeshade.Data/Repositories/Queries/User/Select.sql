SELECT id, created_at CreatedAt, counterparty_id CounterpartyId
FROM users
WHERE id = @id;
