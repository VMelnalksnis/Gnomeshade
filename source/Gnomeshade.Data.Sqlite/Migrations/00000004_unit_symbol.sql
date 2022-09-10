ALTER TABLE units
	ADD COLUMN symbol text NULL;

-- Update raw SQL statements, since SQLite does not support adding constraints
-- https://stackoverflow.com/a/42970982

SELECT sql
FROM sqlite_master
WHERE type = 'table'
  AND name = 'units';

PRAGMA writable_schema=1;

UPDATE sqlite_master
SET sql=REPLACE(sql, '\r\n)', '\tCONSTRAINT "units_symbol_unique" UNIQUE (owner_id, symbol, deleted_at)\r\n)')
WHERE type = 'table'
  AND name = 'units';

PRAGMA writable_schema=0;
