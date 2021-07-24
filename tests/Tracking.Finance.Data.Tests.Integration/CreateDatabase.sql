DROP TABLE IF EXISTS "users";
CREATE TABLE "public"."users"
(
    "id"              uuid        DEFAULT uuid_generate_v4() NOT NULL,
    "created_at"      timestamptz DEFAULT CURRENT_TIMESTAMP  NOT NULL,
    "counterparty_id" uuid,
    CONSTRAINT "users_id" PRIMARY KEY ("id")
) WITH (OIDS = FALSE);


DROP TABLE IF EXISTS "owners";
CREATE TABLE "public"."owners"
(
    "id"         uuid        DEFAULT uuid_generate_v4() NOT NULL,
    "created_at" timestamptz DEFAULT CURRENT_TIMESTAMP  NOT NULL,
    CONSTRAINT "owners_id" PRIMARY KEY ("id")
) WITH (OIDS = FALSE);


DROP TABLE IF EXISTS "ownerships";
CREATE TABLE "public"."ownerships"
(
    "id"         uuid        DEFAULT uuid_generate_v4() NOT NULL,
    "created_at" timestamptz DEFAULT CURRENT_TIMESTAMP  NOT NULL,
    "owner_id"   uuid                                   NOT NULL,
    "user_id"    uuid                                   NOT NULL,
    CONSTRAINT "ownerships_id" PRIMARY KEY ("id"),
    CONSTRAINT "ownerships_owner_id_fkey" FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
    CONSTRAINT "ownerships_user_id_fkey" FOREIGN KEY (user_id) REFERENCES users (id) NOT DEFERRABLE
) WITH (OIDS = FALSE);


DROP TABLE IF EXISTS "currencies";
CREATE TABLE "public"."currencies"
(
    "id"              uuid        DEFAULT uuid_generate_v4() NOT NULL,
    "created_at"      timestamptz DEFAULT CURRENT_TIMESTAMP  NOT NULL,
    "name"            text                                   NOT NULL,
    "normalized_name" text                                   NOT NULL,
    "numeric_code"    smallint                               NOT NULL,
    "alphabetic_code" text                                   NOT NULL,
    "minor_unit"      smallint                               NOT NULL,
    "official"        boolean                                NOT NULL,
    "crypto"          boolean                                NOT NULL,
    "historical"      boolean                                NOT NULL,
    "active_from"     timestamptz,
    "active_until"    timestamptz,
    CONSTRAINT "currencies_pk" PRIMARY KEY ("id")
) WITH (OIDS = FALSE);

INSERT INTO "currencies" ("id", "created_at", "name", "normalized_name", "numeric_code", "alphabetic_code",
                          "minor_unit", "official", "crypto", "historical", "active_from", "active_until")
VALUES (uuid_generate_v4(), CURRENT_TIMESTAMP, 'Euro', 'EURO', 978, 'EUR', 2, TRUE, FALSE, FALSE,
        '1999-01-01 00:00:00+00', NULL),
       (uuid_generate_v4(), CURRENT_TIMESTAMP, 'United States dollar', 'UNITED STATES DOLLAR', 840, 'USD', 2, TRUE,
        FALSE, FALSE, '1792-04-02 00:00:00+00', NULL);

DROP TABLE IF EXISTS "accounts";
CREATE TABLE "public"."accounts"
(
    "id"                    uuid        DEFAULT uuid_generate_v4() NOT NULL,
    "created_at"            timestamptz DEFAULT CURRENT_TIMESTAMP  NOT NULL,
    "owner_id"              uuid                                   NOT NULL,
    "created_by_user_id"    uuid                                   NOT NULL,
    "modified_at"           timestamptz DEFAULT CURRENT_TIMESTAMP  NOT NULL,
    "modified_by_user_id"   uuid                                   NOT NULL,
    "name"                  text                                   NOT NULL,
    "normalized_name"       text                                   NOT NULL,
    "preferred_currency_id" uuid                                   NOT NULL,
    "bic"                   text,
    "iban"                  text,
    "account_number"        text,
    CONSTRAINT "accounts_pk" PRIMARY KEY ("id"),
    CONSTRAINT "accounts_created_by_user_id_fkey" FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
    CONSTRAINT "accounts_modified_by_user_id_fkey" FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
    CONSTRAINT "accounts_owner_id_fkey" FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
    CONSTRAINT "accounts_preferred_currency_id_fkey" FOREIGN KEY (preferred_currency_id) REFERENCES currencies (id) NOT DEFERRABLE
) WITH (OIDS = FALSE);


DROP TABLE IF EXISTS "accounts_in_currency";
CREATE TABLE "public"."accounts_in_currency"
(
    "id"                  uuid        DEFAULT uuid_generate_v4() NOT NULL,
    "owner_id"            uuid                                   NOT NULL,
    "created_at"          timestamptz DEFAULT CURRENT_TIMESTAMP  NOT NULL,
    "created_by_user_id"  uuid                                   NOT NULL,
    "modified_at"         timestamptz DEFAULT CURRENT_TIMESTAMP  NOT NULL,
    "modified_by_user_id" uuid                                   NOT NULL,
    "account_id"          uuid                                   NOT NULL,
    "currency_id"         uuid                                   NOT NULL,
    CONSTRAINT "accounts_in_currency_pk" PRIMARY KEY ("id"),
    CONSTRAINT "accounts_in_currency_accounts_id_fk" FOREIGN KEY (account_id) REFERENCES accounts (id) NOT DEFERRABLE,
    CONSTRAINT "accounts_in_currency_currencies_id_fk" FOREIGN KEY (currency_id) REFERENCES currencies (id) NOT DEFERRABLE,
    CONSTRAINT "accounts_in_currency_owners_id_fk" FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
    CONSTRAINT "accounts_in_currency_users_id_fk" FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
    CONSTRAINT "accounts_in_currency_users_id_fk_2" FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE
) WITH (OIDS = FALSE);


DROP TABLE IF EXISTS "transactions";
CREATE TABLE "public"."transactions"
(
    "id"                   uuid        DEFAULT uuid_generate_v4() NOT NULL,
    "owner_id"             uuid                                   NOT NULL,
    "created_at"           timestamptz DEFAULT CURRENT_TIMESTAMP  NOT NULL,
    "created_by_user_id"   uuid                                   NOT NULL,
    "modified_at"          timestamptz DEFAULT CURRENT_TIMESTAMP  NOT NULL,
    "modified_by_user_id"  uuid                                   NOT NULL,
    "date"                 timestamptz                            NOT NULL,
    "description"          text,
    "imported_at"          timestamptz,
    "import_hash"          bytea,
    "validated_at"         timestamptz,
    "validated_by_user_id" uuid,
    CONSTRAINT "transactions_id" PRIMARY KEY ("id"),
    CONSTRAINT "transactions_created_by_user_id_fkey" FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
    CONSTRAINT "transactions_modified_by_user_id_fkey" FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
    CONSTRAINT "transactions_owner_id_fkey" FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
    CONSTRAINT "transactions_validated_by_user_id_fkey" FOREIGN KEY (validated_by_user_id) REFERENCES users(id) NOT DEFERRABLE
) WITH (OIDS = FALSE);


DROP TABLE IF EXISTS "units";
CREATE TABLE "public"."units"
(
    "id"                  uuid        DEFAULT uuid_generate_v4() NOT NULL,
    "created_at"          timestamptz DEFAULT CURRENT_TIMESTAMP  NOT NULL,
    "owner_id"            uuid                                   NOT NULL,
    "created_by_user_id"  uuid                                   NOT NULL,
    "modified_at"         timestamptz DEFAULT CURRENT_TIMESTAMP  NOT NULL,
    "modified_by_user_id" uuid                                   NOT NULL,
    "name"                text                                   NOT NULL,
    "normalized_name"     text                                   NOT NULL,
    "parent_unit_id"      uuid,
    "multiplier"          numeric,
    CONSTRAINT "units_id" PRIMARY KEY ("id"),
    CONSTRAINT "units_normalized_name" UNIQUE ("normalized_name"),
    CONSTRAINT "units_created_by_user_id_fkey" FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
    CONSTRAINT "units_modified_by_user_id_fkey" FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
    CONSTRAINT "units_owner_id_fkey" FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
    CONSTRAINT "units_parent_unit_id_fkey" FOREIGN KEY (parent_unit_id) REFERENCES units (id) NOT DEFERRABLE
) WITH (OIDS = FALSE);


DROP TABLE IF EXISTS "products";
CREATE TABLE "public"."products"
(
    "id"                  uuid        DEFAULT uuid_generate_v4() NOT NULL,
    "created_at"          timestamptz DEFAULT CURRENT_TIMESTAMP  NOT NULL,
    "owner_id"            uuid                                   NOT NULL,
    "created_by_user_id"  uuid                                   NOT NULL,
    "modified_at"         timestamptz DEFAULT CURRENT_TIMESTAMP  NOT NULL,
    "modified_by_user_id" uuid                                   NOT NULL,
    "name"                text                                   NOT NULL,
    "normalized_name"     text                                   NOT NULL,
    "description"         text,
    "unit_id"             uuid,
    CONSTRAINT "products_id" PRIMARY KEY ("id"),
    CONSTRAINT "products_normalized_name" UNIQUE ("normalized_name"),
    CONSTRAINT "products_created_by_user_id_fkey" FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
    CONSTRAINT "products_modified_by_user_id_fkey" FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
    CONSTRAINT "products_owner_id_fkey" FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
    CONSTRAINT "products_unit_id_fkey" FOREIGN KEY (unit_id) REFERENCES units (id) NOT DEFERRABLE
) WITH (OIDS = FALSE);



DROP TABLE IF EXISTS "transaction_items";
CREATE TABLE "public"."transaction_items"
(
    "id"                  uuid        DEFAULT uuid_generate_v4() NOT NULL,
    "owner_id"            uuid                                   NOT NULL,
    "transaction_id"      uuid                                   NOT NULL,
    "source_amount"       numeric                                NOT NULL,
    "source_account_id"   uuid                                   NOT NULL,
    "target_amount"       numeric                                NOT NULL,
    "target_account_id"   uuid                                   NOT NULL,
    "created_at"          timestamptz DEFAULT CURRENT_TIMESTAMP  NOT NULL,
    "created_by_user_id"  uuid                                   NOT NULL,
    "modified_at"         timestamptz DEFAULT CURRENT_TIMESTAMP  NOT NULL,
    "modified_by_user_id" uuid                                   NOT NULL,
    "product_id"          uuid                                   NOT NULL,
    "amount"              integer                                NOT NULL,
    "bank_reference"      text,
    "external_reference"  text,
    "internal_reference"  text,
    "description"         text,
    "delivery_date"       timestamptz,
    CONSTRAINT "transaction_items_id" PRIMARY KEY ("id"),
    CONSTRAINT "transaction_items_created_by_user_id_fkey" FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
    CONSTRAINT "transaction_items_modified_by_user_id_fkey" FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
    CONSTRAINT "transaction_items_owner_id_fkey" FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
    CONSTRAINT "transaction_items_product_id_fkey" FOREIGN KEY (product_id) REFERENCES products (id) NOT DEFERRABLE,
    CONSTRAINT "transaction_items_source_account_id_fkey" FOREIGN KEY (source_account_id) REFERENCES accounts_in_currency (id) NOT DEFERRABLE,
    CONSTRAINT "transaction_items_target_account_id_fkey" FOREIGN KEY (target_account_id) REFERENCES accounts_in_currency (id) NOT DEFERRABLE,
    CONSTRAINT "transaction_items_transaction_id_fkey" FOREIGN KEY (transaction_id) REFERENCES transactions (id) NOT DEFERRABLE
) WITH (OIDS = FALSE);
