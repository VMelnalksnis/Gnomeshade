CREATE FUNCTION create_user(id uuid, name text)
	RETURNS uuid
	LANGUAGE sql
AS
$$
INSERT INTO users
	(id, modified_by_user_id, counterparty_id)
VALUES
	($1, $1, $1);

INSERT INTO owners
	(id)
VALUES
	($1);

WITH a AS (SELECT ACCESS.id FROM ACCESS WHERE normalized_name = 'OWNER')

INSERT
INTO ownerships
	(id, owner_id, user_id, access_id)
VALUES
	($1, $1, $1, (SELECT * FROM a));

INSERT INTO counterparties
	(id, owner_id, created_by_user_id, modified_by_user_id, NAME, normalized_name)
VALUES
	($1, $1, $1, $1, $2, UPPER($2))
RETURNING id;
$$;

CREATE FUNCTION get_system_user_id()
	RETURNS uuid
	LANGUAGE sql
AS
$$
SELECT CASE
		   WHEN EXISTS(SELECT id FROM counterparties WHERE normalized_name = 'SYSTEM')
			   THEN (SELECT id FROM counterparties WHERE normalized_name = 'SYSTEM')
		   ELSE create_user(uuid_generate_v4(), 'SYSTEM')
		   END;
$$
;

SELECT get_system_user_id();
