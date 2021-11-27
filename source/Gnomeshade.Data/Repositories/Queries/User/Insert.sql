INSERT INTO users
    (id, modified_by_user_id, counterparty_id)
VALUES
    (@Id, @ModifiedByUserId, @CounterpartyId)
RETURNING id;
