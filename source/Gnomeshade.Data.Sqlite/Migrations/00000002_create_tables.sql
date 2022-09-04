CREATE TABLE users
(
	id                  uuid        DEFAULT (uuid_generate_v4())   NOT NULL,
	created_at          timestamptz DEFAULT CURRENT_TIMESTAMP      NOT NULL,
	created_by_user_id  uuid        DEFAULT (get_system_user_id()) NOT NULL,
	modified_at         timestamptz DEFAULT CURRENT_TIMESTAMP      NOT NULL,
	modified_by_user_id uuid                                       NOT NULL,
	deleted_at          timestamptz,
	deleted_by_user_id  uuid,
-- 	"counterparty_id"     uuid                                     NOT NULL,
	CONSTRAINT users_id PRIMARY KEY (id),
	CONSTRAINT users_created_by_user_id_fkey FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT users_modified_by_user_id_fkey FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT users_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT users_deleted_check CHECK ((deleted_at IS NULL AND deleted_by_user_id IS NULL) OR
										  (deleted_at IS NOT NULL AND deleted_by_user_id IS NOT NULL))
);


CREATE TABLE owners
(
	id                 uuid        DEFAULT (uuid_generate_v4())   NOT NULL,
	created_at         timestamptz DEFAULT CURRENT_TIMESTAMP      NOT NULL,
	created_by_user_id uuid        DEFAULT (get_system_user_id()) NOT NULL,
	deleted_at         timestamptz,
	deleted_by_user_id uuid,

	CONSTRAINT owners_id PRIMARY KEY (id),
	CONSTRAINT owners_created_by_user_id_fkey FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT owners_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT owners_deleted_check CHECK ((deleted_at IS NULL AND deleted_by_user_id IS NULL) OR
										   (deleted_at IS NOT NULL AND deleted_by_user_id IS NOT NULL))
);


CREATE TABLE access
(
	id                 uuid        DEFAULT (uuid_generate_v4())   NOT NULL,
	created_at         timestamptz DEFAULT CURRENT_TIMESTAMP      NOT NULL,
	created_by_user_id uuid        DEFAULT (get_system_user_id()) NOT NULL,
	name               text                                       NOT NULL,
	normalized_name    text                                       NOT NULL,
	deleted_at         timestamptz,
	deleted_by_user_id uuid,

	CONSTRAINT access_id PRIMARY KEY (id),
	CONSTRAINT access_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT access_deleted_check CHECK ((deleted_at IS NULL AND deleted_by_user_id IS NULL) OR
										   (deleted_at IS NOT NULL AND deleted_by_user_id IS NOT NULL)),
	CONSTRAINT access_normalized_name_unique_index UNIQUE (normalized_name)
);

CREATE TABLE ownerships
(
	id                 uuid        DEFAULT (uuid_generate_v4())   NOT NULL,
	created_at         timestamptz DEFAULT CURRENT_TIMESTAMP      NOT NULL,
	created_by_user_id uuid        DEFAULT (get_system_user_id()) NOT NULL,
	owner_id           uuid                                       NOT NULL,
	user_id            uuid                                       NOT NULL,
	access_id          uuid                                       NOT NULL,
	deleted_at         timestamptz,
	deleted_by_user_id uuid,

	CONSTRAINT ownerships_id PRIMARY KEY (id),
	CONSTRAINT ownerships_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
	CONSTRAINT ownerships_user_id_fkey FOREIGN KEY (user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT ownerships_access_id_fkey FOREIGN KEY (access_id) REFERENCES access (id) NOT DEFERRABLE,
	CONSTRAINT ownerships_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT ownerships_deleted_check CHECK ((deleted_at IS NULL AND deleted_by_user_id IS NULL) OR
											   (deleted_at IS NOT NULL AND deleted_by_user_id IS NOT NULL))
);


CREATE TABLE categories
(
	id                  uuid        DEFAULT (uuid_generate_v4()) NOT NULL,
	created_at          timestamptz DEFAULT CURRENT_TIMESTAMP    NOT NULL,
	owner_id            uuid                                     NOT NULL,
	created_by_user_id  uuid                                     NOT NULL,
	modified_at         timestamptz DEFAULT CURRENT_TIMESTAMP    NOT NULL,
	modified_by_user_id uuid                                     NOT NULL,
	name                text                                     NOT NULL,
	normalized_name     text                                     NOT NULL,
	deleted_at          timestamptz,
	deleted_by_user_id  uuid,
	description         text,
	category_id         uuid,

	CONSTRAINT categories_id PRIMARY KEY (id),
	CONSTRAINT categories_normalized_name UNIQUE (normalized_name),
	CONSTRAINT categories_created_by_user_id_fkey FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT categories_modified_by_user_id_fkey FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT categories_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
	CONSTRAINT categories_category_id FOREIGN KEY (category_id) REFERENCES categories (id) NOT DEFERRABLE,
	CONSTRAINT categories_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT categories_deleted_check CHECK ((deleted_at IS NULL AND deleted_by_user_id IS NULL) OR
											   (deleted_at IS NOT NULL AND deleted_by_user_id IS NOT NULL))
);


CREATE TABLE currencies
(
	id                 uuid        DEFAULT (uuid_generate_v4())   NOT NULL,
	created_at         timestamptz DEFAULT CURRENT_TIMESTAMP      NOT NULL,
	created_by_user_id uuid        DEFAULT (get_system_user_id()) NOT NULL,
	name               text                                       NOT NULL,
	normalized_name    text                                       NOT NULL,
	numeric_code       smallint                                   NOT NULL,
	alphabetic_code    text                                       NOT NULL,
	minor_unit         smallint                                   NOT NULL,
	official           boolean                                    NOT NULL,
	crypto             boolean                                    NOT NULL,
	historical         boolean                                    NOT NULL,
	deleted_at         timestamptz,
	deleted_by_user_id uuid,
	active_from        timestamptz,
	active_until       timestamptz,

	CONSTRAINT currencies_pk PRIMARY KEY (id),
	CONSTRAINT currencies_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT currencies_deleted_check CHECK ((deleted_at IS NULL AND deleted_by_user_id IS NULL) OR
											   (deleted_at IS NOT NULL AND deleted_by_user_id IS NOT NULL))
);


CREATE TABLE counterparties
(
	id                  uuid        DEFAULT (uuid_generate_v4()) NOT NULL,
	created_at          timestamptz DEFAULT CURRENT_TIMESTAMP    NOT NULL,
	owner_id            uuid                                     NOT NULL,
	created_by_user_id  uuid                                     NOT NULL,
	modified_at         timestamptz DEFAULT CURRENT_TIMESTAMP    NOT NULL,
	modified_by_user_id uuid                                     NOT NULL,
	name                text                                     NOT NULL,
	normalized_name     text                                     NOT NULL,
	deleted_at          timestamptz,
	deleted_by_user_id  uuid,

	CONSTRAINT counterparties_id PRIMARY KEY (id),
	CONSTRAINT counterparties_created_by_user_id_fkey FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT counterparties_modified_by_user_id_fkey FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT counterparties_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
	CONSTRAINT counterparties_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT counterparties_deleted_check CHECK ((deleted_at IS NULL AND deleted_by_user_id IS NULL) OR
												   (deleted_at IS NOT NULL AND deleted_by_user_id IS NOT NULL))
);

ALTER TABLE users
	ADD COLUMN counterparty_id uuid NOT NULL REFERENCES counterparties (id) DEFERRABLE INITIALLY DEFERRED;


CREATE TABLE accounts
(
	id                    uuid        DEFAULT (uuid_generate_v4()) NOT NULL,
	created_at            timestamptz DEFAULT CURRENT_TIMESTAMP    NOT NULL,
	owner_id              uuid                                     NOT NULL,
	created_by_user_id    uuid                                     NOT NULL,
	modified_at           timestamptz DEFAULT CURRENT_TIMESTAMP    NOT NULL,
	modified_by_user_id   uuid                                     NOT NULL,
	name                  text                                     NOT NULL,
	normalized_name       text                                     NOT NULL,
	counterparty_id       uuid                                     NOT NULL,
	preferred_currency_id uuid                                     NOT NULL,
	deleted_at            timestamptz,
	deleted_by_user_id    uuid,
	bic                   text,
	iban                  text,
	account_number        text,
	disabled_at           timestamptz,
	disabled_by_user_id   uuid,
	CONSTRAINT accounts_normalized_name UNIQUE (counterparty_id, normalized_name),
	CONSTRAINT accounts_pk PRIMARY KEY (id),
	CONSTRAINT accounts_counterparty_id_fkey FOREIGN KEY (counterparty_id) REFERENCES counterparties (id) NOT DEFERRABLE,
	CONSTRAINT accounts_created_by_user_id_fkey FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT accounts_disabled_by_user_id_fkey FOREIGN KEY (disabled_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT accounts_modified_by_user_id_fkey FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT accounts_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
	CONSTRAINT accounts_preferred_currency_id_fkey FOREIGN KEY (preferred_currency_id) REFERENCES currencies (id) NOT DEFERRABLE,
	CONSTRAINT accounts_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT accounts_deleted_check CHECK ((deleted_at IS NULL AND deleted_by_user_id IS NULL) OR
											 (deleted_at IS NOT NULL AND deleted_by_user_id IS NOT NULL))
);


CREATE TABLE accounts_in_currency
(
	id                  uuid        DEFAULT (uuid_generate_v4()) NOT NULL,
	owner_id            uuid                                     NOT NULL,
	created_at          timestamptz DEFAULT CURRENT_TIMESTAMP    NOT NULL,
	created_by_user_id  uuid                                     NOT NULL,
	modified_at         timestamptz DEFAULT CURRENT_TIMESTAMP    NOT NULL,
	modified_by_user_id uuid                                     NOT NULL,
	account_id          uuid                                     NOT NULL,
	currency_id         uuid                                     NOT NULL,
	deleted_at          timestamptz,
	deleted_by_user_id  uuid,
	disabled_at         timestamptz,
	disabled_by_user_id uuid,
	CONSTRAINT accounts_in_currency_pk PRIMARY KEY (id),
	CONSTRAINT accounts_in_currency_account_id_fkey FOREIGN KEY (account_id) REFERENCES accounts (id) NOT DEFERRABLE,
	CONSTRAINT accounts_in_currency_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES currencies (id) NOT DEFERRABLE,
	CONSTRAINT accounts_in_currency_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
	CONSTRAINT accounts_in_currency_created_by_user_id_fkey FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT accounts_in_currency_modified_by_user_id_fkey FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT accounts_in_currency_disabled_by_user_id_fkey FOREIGN KEY (disabled_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT accounts_in_currency_unique_account_id_currency_id UNIQUE (account_id, currency_id),
	CONSTRAINT accounts_in_currency_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT accounts_in_currency_deleted_check CHECK ((deleted_at IS NULL AND deleted_by_user_id IS NULL) OR
														 (deleted_at IS NOT NULL AND deleted_by_user_id IS NOT NULL))
);


CREATE TABLE "transactions"
(
	"id"                    uuid        DEFAULT (uuid_generate_v4()) NOT NULL,
	"owner_id"              uuid                                     NOT NULL,
	"created_at"            timestamptz DEFAULT CURRENT_TIMESTAMP    NOT NULL,
	"created_by_user_id"    uuid                                     NOT NULL,
	"modified_at"           timestamptz DEFAULT CURRENT_TIMESTAMP    NOT NULL,
	"modified_by_user_id"   uuid                                     NOT NULL,
	"booked_at"             timestamptz CHECK ( booked_at IS NOT NULL OR valued_at IS NOT NULL),
	"valued_at"             timestamptz CHECK ( valued_at IS NOT NULL OR booked_at IS NOT NULL),
	deleted_at              timestamptz,
	deleted_by_user_id      uuid,
	"description"           text,
	"imported_at"           timestamptz,
	"import_hash"           bytea,
	"reconciled_at"         timestamptz,
	"reconciled_by_user_id" uuid,
	CONSTRAINT "transactions_id" PRIMARY KEY ("id"),
	CONSTRAINT "transactions_created_by_user_id_fkey" FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT "transactions_import_hash" UNIQUE ("import_hash"),
	CONSTRAINT "transactions_modified_by_user_id_fkey" FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT "transactions_owner_id_fkey" FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
	CONSTRAINT "transactions_reconciled_by_user_id_fkey" FOREIGN KEY (reconciled_by_user_id) REFERENCES users (id) NOT DEFERRABLE
);


CREATE TABLE "units"
(
	"id"                  uuid        DEFAULT (uuid_generate_v4()) NOT NULL,
	"created_at"          timestamptz DEFAULT CURRENT_TIMESTAMP    NOT NULL,
	"owner_id"            uuid                                     NOT NULL,
	"created_by_user_id"  uuid                                     NOT NULL,
	"modified_at"         timestamptz DEFAULT CURRENT_TIMESTAMP    NOT NULL,
	"modified_by_user_id" uuid                                     NOT NULL,
	"name"                text                                     NOT NULL,
	"normalized_name"     text                                     NOT NULL,
	deleted_at            timestamptz,
	deleted_by_user_id    uuid,
	"parent_unit_id"      uuid,
	"multiplier"          numeric,
	CONSTRAINT "units_id" PRIMARY KEY ("id"),
	CONSTRAINT "units_normalized_name" UNIQUE ("normalized_name"),
	CONSTRAINT "units_created_by_user_id_fkey" FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT "units_modified_by_user_id_fkey" FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT "units_owner_id_fkey" FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
	CONSTRAINT "units_parent_unit_id_fkey" FOREIGN KEY (parent_unit_id) REFERENCES units (id) NOT DEFERRABLE
);


CREATE TABLE "products"
(
	"id"                  uuid        DEFAULT (uuid_generate_v4()) NOT NULL,
	"created_at"          timestamptz DEFAULT CURRENT_TIMESTAMP    NOT NULL,
	"owner_id"            uuid                                     NOT NULL,
	"created_by_user_id"  uuid                                     NOT NULL,
	"modified_at"         timestamptz DEFAULT CURRENT_TIMESTAMP    NOT NULL,
	"modified_by_user_id" uuid                                     NOT NULL,
	"name"                text                                     NOT NULL,
	"normalized_name"     text                                     NOT NULL,
	deleted_at            timestamptz,
	deleted_by_user_id    uuid,
	"description"         text,
	"unit_id"             uuid,
	"sku"                 text,
	"category_id"         uuid,
	CONSTRAINT "products_id" PRIMARY KEY ("id"),
	CONSTRAINT "products_normalized_name" UNIQUE ("normalized_name"),
	CONSTRAINT "products_created_by_user_id_fkey" FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT "products_modified_by_user_id_fkey" FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT "products_owner_id_fkey" FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
	CONSTRAINT "products_unit_id_fkey" FOREIGN KEY (unit_id) REFERENCES units (id) NOT DEFERRABLE,
	CONSTRAINT "products_category_id_fkey" FOREIGN KEY (category_id) REFERENCES categories (id) NOT DEFERRABLE
);


CREATE TABLE transfers
(
	id                   uuid        DEFAULT (uuid_generate_v4()) NOT NULL,
	created_at           timestamptz DEFAULT CURRENT_TIMESTAMP NOT NULL,
	owner_id             uuid                                  NOT NULL,
	created_by_user_id   uuid                                  NOT NULL,
	modified_at          timestamptz DEFAULT CURRENT_TIMESTAMP NOT NULL,
	modified_by_user_id  uuid                                  NOT NULL,

	transaction_id     uuid                                  NOT NULL,
	source_amount      numeric                               NOT NULL,
	source_account_id  uuid                                  NOT NULL,
	target_amount      numeric                               NOT NULL,
	target_account_id  uuid                                  NOT NULL,
	deleted_at         timestamptz,
	deleted_by_user_id uuid,

	bank_reference     text,
	external_reference text,
	internal_reference text,

	CONSTRAINT transfers_id PRIMARY KEY ("id"),
	CONSTRAINT transfers_created_by_user_id_fkey FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT transfers_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
	CONSTRAINT transfers_modified_by_user_id_fkey FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE,

	CONSTRAINT transfers_transaction_id_fkey FOREIGN KEY (transaction_id) REFERENCES transactions (id) NOT DEFERRABLE,
	CONSTRAINT transfers_source_account_id_fkey FOREIGN KEY (source_account_id) REFERENCES accounts_in_currency (id) NOT DEFERRABLE,
	CONSTRAINT transfers_target_account_id_fkey FOREIGN KEY (target_account_id) REFERENCES accounts_in_currency (id) NOT DEFERRABLE,

	CONSTRAINT transfers_bank_reference_unique UNIQUE (bank_reference, owner_id, deleted_at),
	CONSTRAINT transfers_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT transfers_deleted_check CHECK ((deleted_at IS NULL AND deleted_by_user_id IS NULL) OR
										  (deleted_at IS NOT NULL AND deleted_by_user_id IS NOT NULL))
);

CREATE TABLE "purchases"
(
	"id"                  uuid        DEFAULT (uuid_generate_v4()) NOT NULL,
	"created_at"          timestamptz DEFAULT CURRENT_TIMESTAMP    NOT NULL,
	"owner_id"            uuid                                     NOT NULL,
	"created_by_user_id"  uuid                                     NOT NULL,
	"modified_at"         timestamptz DEFAULT CURRENT_TIMESTAMP    NOT NULL,
	"modified_by_user_id" uuid                                     NOT NULL,

	"transaction_id"      uuid                                     NOT NULL,
	"price"               numeric                                  NOT NULL,
	"currency_id"         uuid                                     NOT NULL,
	"product_id"          uuid                                     NOT NULL,
	"amount"              numeric                                  NOT NULL,
	deleted_at            timestamptz,
	deleted_by_user_id    uuid,
	"delivery_date"       timestamptz,

	CONSTRAINT "purchases_id" PRIMARY KEY ("id"),
	CONSTRAINT "purchases_created_by_user_id_fkey" FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT "purchases_owner_id_fkey" FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
	CONSTRAINT "purchases_modified_by_user_id_fkey" FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE,

	CONSTRAINT "purchases_transaction_id_fkey" FOREIGN KEY (transaction_id) REFERENCES transactions (id) NOT DEFERRABLE,
	CONSTRAINT "purchases_currency_id_fkey" FOREIGN KEY (currency_id) REFERENCES currencies (id) NOT DEFERRABLE,
	CONSTRAINT "purchases_product_id_fkey" FOREIGN KEY (product_id) REFERENCES products (id) NOT DEFERRABLE
);

CREATE TABLE "links"
(
	"id"                  uuid        DEFAULT (uuid_generate_v4()) NOT NULL,
	"created_at"          timestamptz DEFAULT CURRENT_TIMESTAMP    NOT NULL,
	"owner_id"            uuid                                     NOT NULL,
	"created_by_user_id"  uuid                                     NOT NULL,
	"modified_at"         timestamptz DEFAULT CURRENT_TIMESTAMP    NOT NULL,
	"modified_by_user_id" uuid                                     NOT NULL,

	"uri"                 text                                     NOT NULL,
	deleted_at            timestamptz,
	deleted_by_user_id    uuid,

	CONSTRAINT "links_id" PRIMARY KEY ("id"),
	CONSTRAINT "links_created_by_user_id_fkey" FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT "links_owner_id_fkey" FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
	CONSTRAINT "links_modified_by_user_id_fkey" FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE,

	CONSTRAINT "links_uri_unique" UNIQUE (uri, owner_id, deleted_at)
);

CREATE TABLE "transaction_links"
(
	"created_at"         timestamptz DEFAULT CURRENT_TIMESTAMP NOT NULL,
	"created_by_user_id" uuid                                  NOT NULL,
	"link_id"            uuid                                  NOT NULL,
	"transaction_id"     uuid                                  NOT NULL,
	deleted_at           timestamptz,
	deleted_by_user_id   uuid,

	CONSTRAINT "transaction_links_id" PRIMARY KEY ("link_id", "transaction_id"),
	CONSTRAINT "transaction_links_created_by_user_id_fkey" FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT "transaction_links_link_id_fkey" FOREIGN KEY (link_id) REFERENCES links (id) NOT DEFERRABLE,
	CONSTRAINT "transaction_links_transaction_id_fkey" FOREIGN KEY (transaction_id) REFERENCES transactions (id) NOT DEFERRABLE
);

CREATE TABLE loans
(
	id                        uuid        DEFAULT (uuid_generate_v4()) NOT NULL,
	created_at                timestamptz DEFAULT CURRENT_TIMESTAMP    NOT NULL,
	owner_id                  uuid                                     NOT NULL,
	created_by_user_id        uuid                                     NOT NULL,
	modified_at               timestamptz DEFAULT CURRENT_TIMESTAMP    NOT NULL,
	modified_by_user_id       uuid                                     NOT NULL,

	transaction_id            uuid                                     NOT NULL,
	issuing_counterparty_id   uuid                                     NOT NULL,
	receiving_counterparty_id uuid                                     NOT NULL,
	amount                    numeric                                  NOT NULL,
	currency_id               uuid                                     NOT NULL,
	deleted_at                timestamptz,
	deleted_by_user_id        uuid,

	CONSTRAINT loans_id PRIMARY KEY (id),
	CONSTRAINT loans_created_by_user_id_fkey FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT loans_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
	CONSTRAINT loans_modified_by_user_id_fkey FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE,

	CONSTRAINT loans_transaction_id_fkey FOREIGN KEY (transaction_id) REFERENCES transactions (id) NOT DEFERRABLE,
	CONSTRAINT loans_issuing_counterparty_id_fkey FOREIGN KEY (issuing_counterparty_id) REFERENCES counterparties (id) NOT DEFERRABLE,
	CONSTRAINT loans_receiving_counterparty_id_fkey FOREIGN KEY (receiving_counterparty_id) REFERENCES counterparties (id) NOT DEFERRABLE,
	CONSTRAINT loans_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES currencies (id) NOT DEFERRABLE,
	CONSTRAINT loans_deleted_by_user_id_fkey FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT loans_deleted_check CHECK ((deleted_at IS NULL AND deleted_by_user_id IS NULL) OR
														 (deleted_at IS NOT NULL AND deleted_by_user_id IS NOT NULL))
);
