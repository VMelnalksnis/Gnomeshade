-- Weight
INSERT INTO units (owner_id, created_by_user_id, modified_by_user_id, name, normalized_name, parent_unit_id, multiplier, symbol, inverse_multiplier)
VALUES (get_system_user_id(), get_system_user_id(), get_system_user_id(), 'Kilogram', 'KILOGRAM', NULL, NULL, 'kg', false);

INSERT INTO units (owner_id, created_by_user_id, modified_by_user_id, name, normalized_name, parent_unit_id, multiplier, symbol, inverse_multiplier)
SELECT get_system_user_id(), get_system_user_id(), get_system_user_id(), 'Gram', 'GRAM', kilogram.id, 1000, 'g', false
FROM (SELECT id FROM units WHERE owner_id = get_system_user_id() AND normalized_name = 'KILOGRAM') AS kilogram;

-- Volume
INSERT INTO units (owner_id, created_by_user_id, modified_by_user_id, name, normalized_name, parent_unit_id, multiplier, symbol, inverse_multiplier)
VALUES (get_system_user_id(), get_system_user_id(), get_system_user_id(), 'Liter', 'LITER', NULL, NULL, 'l', false);

INSERT INTO units (owner_id, created_by_user_id, modified_by_user_id, name, normalized_name, parent_unit_id, multiplier, symbol, inverse_multiplier)
SELECT get_system_user_id(), get_system_user_id(), get_system_user_id(), 'Milliliter', 'MILLILITER', liter.id, 1000, 'ml', false
FROM (SELECT id FROM units WHERE owner_id = get_system_user_id() AND normalized_name = 'LITER') AS liter;

-- Time
INSERT INTO units (owner_id, created_by_user_id, modified_by_user_id, name, normalized_name, parent_unit_id, multiplier, symbol, inverse_multiplier)
VALUES (get_system_user_id(), get_system_user_id(), get_system_user_id(), 'Month', 'MONTH', NULL, NULL, NULL, false),
	   (get_system_user_id(), get_system_user_id(), get_system_user_id(), 'Year', 'YEAR', NULL, NULL, NULL, false),
	   (get_system_user_id(), get_system_user_id(), get_system_user_id(), 'Second', 'SECOND', NULL, NULL, 's', false);

INSERT INTO units (owner_id, created_by_user_id, modified_by_user_id, name, normalized_name, parent_unit_id, multiplier, symbol, inverse_multiplier)
SELECT get_system_user_id(), get_system_user_id(), get_system_user_id(), 'Minute', 'MINUTE', second.id, 60, 'm', true
FROM (SELECT id FROM units WHERE owner_id = get_system_user_id() AND normalized_name = 'SECOND') AS second;

INSERT INTO units (owner_id, created_by_user_id, modified_by_user_id, name, normalized_name, parent_unit_id, multiplier, symbol, inverse_multiplier)
SELECT get_system_user_id(), get_system_user_id(), get_system_user_id(), 'Hour', 'HOUR', minute.id, 60, 'h', true
FROM (SELECT id FROM units WHERE owner_id = get_system_user_id() AND normalized_name = 'MINUTE') AS minute;

INSERT INTO units (owner_id, created_by_user_id, modified_by_user_id, name, normalized_name, parent_unit_id, multiplier, symbol, inverse_multiplier)
SELECT get_system_user_id(), get_system_user_id(), get_system_user_id(), 'Day', 'DAY', hour.id, 24, NULL, true
FROM (SELECT id FROM units WHERE owner_id = get_system_user_id() AND normalized_name = 'HOUR') AS hour;

INSERT INTO units (owner_id, created_by_user_id, modified_by_user_id, name, normalized_name, parent_unit_id, multiplier, symbol, inverse_multiplier)
SELECT get_system_user_id(), get_system_user_id(), get_system_user_id(), 'Week', 'WEEK', day.id, 7, NULL, true
FROM (SELECT id FROM units WHERE owner_id = get_system_user_id() AND normalized_name = 'DAY') AS day;

-- Misc
INSERT INTO units (owner_id, created_by_user_id, modified_by_user_id, name, normalized_name, parent_unit_id, multiplier, symbol, inverse_multiplier)
VALUES (get_system_user_id(), get_system_user_id(), get_system_user_id(), 'Piece', 'PIECE', NULL, NULL, NULL, false);
