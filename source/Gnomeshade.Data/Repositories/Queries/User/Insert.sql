INSERT INTO users
	(id, modified_by_user_id, counterparty_id)
VALUES
	(@Id, @ModifiedByUserId, @CounterpartyId)
RETURNING id;

INSERT INTO ownerships (owner_id, user_id, access_id, created_by_user_id)
VALUES (get_system_user_id(), @Id, (SELECT id FROM access WHERE normalized_name = 'READ'), get_system_user_id());
