ALTER TABLE access
	ADD COLUMN created_by_user_id uuid DEFAULT get_system_user_id(),
	ADD CONSTRAINT access_created_by_user_id_fkey FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	ADD COLUMN deleted_at         timestamptz NULL,
	ADD COLUMN deleted_by_user_id uuid        NULL,
	ADD CONSTRAINT access_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	ADD CONSTRAINT access_deleted_check CHECK (num_nulls(deleted_at, deleted_by_user_id) IN (0, 2));

ALTER TABLE accounts
	ADD COLUMN deleted_at         timestamptz NULL,
	ADD COLUMN deleted_by_user_id uuid        NULL,
	ADD CONSTRAINT accounts_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	ADD CONSTRAINT accounts_deleted_check CHECK (num_nulls(deleted_at, deleted_by_user_id) IN (0, 2));

ALTER TABLE accounts_in_currency
	ADD COLUMN deleted_at         timestamptz NULL,
	ADD COLUMN deleted_by_user_id uuid        NULL,
	ADD CONSTRAINT accounts_in_currency_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	ADD CONSTRAINT accounts_in_currency_deleted_check CHECK (num_nulls(deleted_at, deleted_by_user_id) IN (0, 2));

ALTER TABLE categories
	ADD COLUMN deleted_at         timestamptz NULL,
	ADD COLUMN deleted_by_user_id uuid        NULL,
	ADD CONSTRAINT categories_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	ADD CONSTRAINT categories_deleted_check CHECK (num_nulls(deleted_at, deleted_by_user_id) IN (0, 2));

ALTER TABLE counterparties
	ADD COLUMN deleted_at         timestamptz NULL,
	ADD COLUMN deleted_by_user_id uuid        NULL,
	ADD CONSTRAINT counterparties_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	ADD CONSTRAINT counterparties_deleted_check CHECK (num_nulls(deleted_at, deleted_by_user_id) IN (0, 2));

ALTER TABLE currencies
	ADD COLUMN created_by_user_id uuid DEFAULT get_system_user_id(),
	ADD CONSTRAINT currencies_created_by_user_id_fkey FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	ADD COLUMN deleted_at         timestamptz NULL,
	ADD COLUMN deleted_by_user_id uuid        NULL,
	ADD CONSTRAINT currencies_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	ADD CONSTRAINT currencies_deleted_check CHECK (num_nulls(deleted_at, deleted_by_user_id) IN (0, 2));

ALTER TABLE links
	ADD COLUMN deleted_at         timestamptz NULL,
	ADD COLUMN deleted_by_user_id uuid        NULL,
	ADD CONSTRAINT links_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	ADD CONSTRAINT links_deleted_check CHECK (num_nulls(deleted_at, deleted_by_user_id) IN (0, 2)),
	DROP CONSTRAINT links_uri_unique,
	ADD CONSTRAINT links_uri_unique UNIQUE (uri, owner_id, deleted_at) NOT DEFERRABLE;

ALTER TABLE loans
	ADD COLUMN deleted_at         timestamptz NULL,
	ADD COLUMN deleted_by_user_id uuid        NULL,
	ADD CONSTRAINT loans_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	ADD CONSTRAINT loans_deleted_check CHECK (num_nulls(deleted_at, deleted_by_user_id) IN (0, 2));

ALTER TABLE owners
	ADD COLUMN created_by_user_id uuid DEFAULT get_system_user_id(),
	ADD CONSTRAINT owners_created_by_user_id_fkey FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	ADD COLUMN deleted_at         timestamptz NULL,
	ADD COLUMN deleted_by_user_id uuid        NULL,
	ADD CONSTRAINT owners_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	ADD CONSTRAINT owners_deleted_check CHECK (num_nulls(deleted_at, deleted_by_user_id) IN (0, 2));

ALTER TABLE ownerships
	ADD COLUMN created_by_user_id uuid DEFAULT get_system_user_id(),
	ADD CONSTRAINT ownerships_created_by_user_id_fkey FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	ADD COLUMN deleted_at         timestamptz NULL,
	ADD COLUMN deleted_by_user_id uuid        NULL,
	ADD CONSTRAINT ownerships_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	ADD CONSTRAINT ownerships_deleted_check CHECK (num_nulls(deleted_at, deleted_by_user_id) IN (0, 2));

ALTER TABLE products
	ADD COLUMN deleted_at         timestamptz NULL,
	ADD COLUMN deleted_by_user_id uuid        NULL,
	ADD CONSTRAINT products_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	ADD CONSTRAINT products_deleted_check CHECK (num_nulls(deleted_at, deleted_by_user_id) IN (0, 2));

ALTER TABLE purchases
	ADD COLUMN deleted_at         timestamptz NULL,
	ADD COLUMN deleted_by_user_id uuid        NULL,
	ADD CONSTRAINT purchases_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	ADD CONSTRAINT purchases_deleted_check CHECK (num_nulls(deleted_at, deleted_by_user_id) IN (0, 2));

ALTER TABLE transactions
	ADD COLUMN deleted_at         timestamptz NULL,
	ADD COLUMN deleted_by_user_id uuid        NULL,
	ADD CONSTRAINT transactions_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	ADD CONSTRAINT transactions_deleted_check CHECK (num_nulls(deleted_at, deleted_by_user_id) IN (0, 2));

ALTER TABLE transfers
	ADD COLUMN deleted_at         timestamptz NULL,
	ADD COLUMN deleted_by_user_id uuid        NULL,
	ADD CONSTRAINT transfers_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	ADD CONSTRAINT transfers_deleted_check CHECK (num_nulls(deleted_at, deleted_by_user_id) IN (0, 2)),
	DROP CONSTRAINT transfers_bank_reference_unique,
	ADD CONSTRAINT transfers_bank_reference_unique UNIQUE (bank_reference, owner_id, deleted_at);

ALTER TABLE units
	ADD COLUMN deleted_at         timestamptz NULL,
	ADD COLUMN deleted_by_user_id uuid        NULL,
	ADD CONSTRAINT units_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	ADD CONSTRAINT units_deleted_check CHECK (num_nulls(deleted_at, deleted_by_user_id) IN (0, 2)),
    DROP CONSTRAINT units_normalized_name,
    ADD CONSTRAINT units_normalized_name UNIQUE (normalized_name, owner_id, deleted_at);

COMMIT;
BEGIN;

ALTER TABLE users
	ADD COLUMN created_by_user_id uuid DEFAULT get_system_user_id(),
	ADD CONSTRAINT users_created_by_user_id_fkey FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	ADD COLUMN deleted_at         timestamptz NULL,
	ADD COLUMN deleted_by_user_id uuid        NULL,
	ADD CONSTRAINT users_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	ADD CONSTRAINT users_deleted_check CHECK (num_nulls(deleted_at, deleted_by_user_id) IN (0, 2));
