ALTER TABLE IF EXISTS "AspNetUserClaims"
	DROP CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId";

ALTER TABLE IF EXISTS "AspNetUserLogins"
	DROP CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId";

ALTER TABLE IF EXISTS "AspNetUserRoles"
	DROP CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId",
    DROP CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId";

ALTER TABLE IF EXISTS "AspNetUserTokens"
	DROP CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId";

ALTER TABLE IF EXISTS "AspNetRoleClaims"
	DROP CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId";

ALTER TABLE IF EXISTS "AspNetUsers"
	ALTER COLUMN "Id" TYPE uuid USING "Id"::uuid;

ALTER TABLE IF EXISTS "AspNetRoles"
	ALTER COLUMN "Id" TYPE uuid USING "Id"::uuid;

ALTER TABLE IF EXISTS "AspNetUserClaims"
	ALTER COLUMN "UserId" TYPE uuid USING "UserId"::uuid,
	ADD CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id");

ALTER TABLE IF EXISTS "AspNetUserLogins"
	ALTER COLUMN "UserId" TYPE uuid USING "UserId"::uuid,
	ADD CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id");

ALTER TABLE IF EXISTS "AspNetUserRoles"
	ALTER COLUMN "UserId" TYPE uuid USING "UserId"::uuid,
	ALTER COLUMN "RoleId" TYPE uuid USING "RoleId"::uuid,
	ADD CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id"),
	ADD CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id");

ALTER TABLE IF EXISTS "AspNetUserTokens"
	ALTER COLUMN "UserId" TYPE uuid USING "UserId"::uuid,
	ADD CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id");

ALTER TABLE IF EXISTS "AspNetRoleClaims"
	ALTER COLUMN "RoleId" TYPE uuid USING "RoleId"::uuid,
	ADD CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id");
