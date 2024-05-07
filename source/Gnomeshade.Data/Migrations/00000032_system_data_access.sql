INSERT INTO ownerships (owner_id, user_id, access_id, created_by_user_id)
SELECT get_system_user_id(), users.id, (SELECT id FROM access WHERE normalized_name = 'READ'), get_system_user_id()
FROM users;
