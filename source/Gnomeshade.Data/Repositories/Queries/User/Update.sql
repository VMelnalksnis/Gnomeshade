UPDATE users
SET modified_at     = DEFAULT,
    counterparty_id = @CounterpartyId
WHERE id = @Id;
