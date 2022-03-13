INSERT INTO "access"
    (name, normalized_name)
VALUES
    ('Read', 'READ'),
    ('Write', 'WRITE'),
    ('Delete', 'DELETE'),
    ('Owner', 'OWNER');

INSERT INTO "currencies"
    ("name",
     "normalized_name",
     "numeric_code",
     "alphabetic_code",
     "minor_unit",
     "official",
     "crypto",
     "historical",
     "active_from",
     "active_until")
VALUES
    ('Czech koruna', 'CZECH KORUNA', 203, 'CZK', 2, TRUE, FALSE, FALSE, '1993-01-01 00:00:00+00', NULL),
    ('Euro', 'EURO', 978, 'EUR', 2, TRUE, FALSE, FALSE, '1999-01-01 00:00:00+00', NULL),
    ('Pound sterling', 'POUND STERLING', 826, 'GBP', 2, TRUE, FALSE, FALSE, '1694-07-27 00:00:00+00', NULL),
    ('Croatian kuna', 'CROATIAN KUNA', 191, 'HRK', 2, TRUE, FALSE, FALSE, '1994-05-30 00:00:00+00', NULL),
    ('Latvian lats', 'LATVIAN LATS', 428, 'LVL', 2, TRUE, FALSE, TRUE, '1993-03-05 00:00:00+00', '2013-12-31 23:59:59+00'),
    ('Polish złoty', 'POLISH ZŁOTY', 985, 'PLN', 2, TRUE, FALSE, FALSE, '1995-01-01 00:00:00+00', NULL),
    ('Russian ruble', 'RUSSIAN RUBLE', 643, 'RUB', 2, TRUE, FALSE, FALSE, '1998-01-01 00:00:00+00', NULL),
    ('United States dollar', 'UNITED STATES DOLLAR', 840, 'USD', 2, TRUE, FALSE, FALSE, '1792-04-02 00:00:00+00', NULL);
