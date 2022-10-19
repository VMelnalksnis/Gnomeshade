ALTER TABLE purchases
	ADD COLUMN "order" int NULL CHECK ("order" IS NULL OR "order" >= 0);

ALTER TABLE transfers
	ADD COLUMN "order" int NULL CHECK ("order" IS NULL OR "order" >= 0);
