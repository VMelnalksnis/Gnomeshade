UPDATE users
SET modified_at     = CURRENT_TIMESTAMP,
    counterparty_id = @CounterpartyId
WHERE id = @Id;
