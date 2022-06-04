UPDATE ownerships
SET owner_id  = @OwnerId,
	user_id   = @UserId,
	access_id = @AccessId
WHERE id = @Id;
