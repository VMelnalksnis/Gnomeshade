ALTER TABLE units
	ADD COLUMN symbol text NULL,
	ADD CONSTRAINT "units_symbol_unique" UNIQUE (owner_id, symbol, deleted_at);
