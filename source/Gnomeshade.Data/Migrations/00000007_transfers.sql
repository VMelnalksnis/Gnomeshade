DROP TABLE IF EXISTS "public"."transfers";
CREATE TABLE "public"."transfers"
(
    "id"                  uuid        DEFAULT uuid_generate_v4() NOT NULL,
    "created_at"          timestamptz DEFAULT CURRENT_TIMESTAMP  NOT NULL,
    "owner_id"            uuid                                   NOT NULL,
    "created_by_user_id"  uuid                                   NOT NULL,
    "modified_at"         timestamptz DEFAULT CURRENT_TIMESTAMP  NOT NULL,
    "modified_by_user_id" uuid                                   NOT NULL,

    "transaction_id"      uuid                                   NOT NULL,
    "source_amount"       numeric                                NOT NULL,
    "source_account_id"   uuid                                   NOT NULL,
    "target_amount"       numeric                                NOT NULL,
    "target_account_id"   uuid                                   NOT NULL,

    "bank_reference"      text,
    "external_reference"  text,
    "internal_reference"  text,

    CONSTRAINT "transfers_id" PRIMARY KEY ("id"),
    CONSTRAINT "transfers_created_by_user_id_fkey" FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
    CONSTRAINT "transfers_owner_id_fkey" FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
    CONSTRAINT "transfers_modified_by_user_id_fkey" FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE,

    CONSTRAINT "transfers_transaction_id_fkey" FOREIGN KEY (transaction_id) REFERENCES transactions (id) NOT DEFERRABLE,
    CONSTRAINT "transfers_source_account_id_fkey" FOREIGN KEY (source_account_id) REFERENCES accounts_in_currency (id) NOT DEFERRABLE,
    CONSTRAINT "transfers_target_account_id_fkey" FOREIGN KEY (target_account_id) REFERENCES accounts_in_currency (id) NOT DEFERRABLE,

    CONSTRAINT "transfers_bank_reference_unique" UNIQUE (bank_reference) NOT DEFERRABLE
) WITH (OIDS = FALSE);
