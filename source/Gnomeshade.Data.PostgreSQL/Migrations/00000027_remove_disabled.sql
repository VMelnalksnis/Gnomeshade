ALTER TABLE "accounts"
	DROP COLUMN "disabled_at",
	DROP COLUMN "disabled_by_user_id";

ALTER TABLE "accounts_in_currency"
	DROP COLUMN "disabled_at",
	DROP COLUMN "disabled_by_user_id";
