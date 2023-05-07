INSERT INTO ownerships
	(id, owner_id, user_id, access_id)
VALUES
	(@Id, @OwnerId, @UserId, @AccessId)
RETURNING id;
