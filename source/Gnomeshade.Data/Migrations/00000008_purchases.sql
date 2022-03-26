DROP TABLE IF EXISTS "public"."purchases";
CREATE TABLE "public"."purchases"
(
    "id"                  uuid        DEFAULT uuid_generate_v4() NOT NULL,
    "created_at"          timestamptz DEFAULT CURRENT_TIMESTAMP  NOT NULL,
    "owner_id"            uuid                                   NOT NULL,
    "created_by_user_id"  uuid                                   NOT NULL,
    "modified_at"         timestamptz DEFAULT CURRENT_TIMESTAMP  NOT NULL,
    "modified_by_user_id" uuid                                   NOT NULL,

    "transaction_id"      uuid                                   NOT NULL,
    "price"               numeric                                NOT NULL,
    "currency_id"         uuid                                   NOT NULL,
    "product_id"          uuid                                   NOT NULL,
    "amount"              numeric                                NOT NULL,
    "delivery_date"       timestamptz,

    CONSTRAINT "purchases_id" PRIMARY KEY ("id"),
    CONSTRAINT "purchases_created_by_user_id_fkey" FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
    CONSTRAINT "purchases_owner_id_fkey" FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
    CONSTRAINT "purchases_modified_by_user_id_fkey" FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE,

    CONSTRAINT "purchases_transaction_id_fkey" FOREIGN KEY (transaction_id) REFERENCES transactions (id) NOT DEFERRABLE,
    CONSTRAINT "purchases_currency_id_fkey" FOREIGN KEY (currency_id) REFERENCES currencies (id) NOT DEFERRABLE,
    CONSTRAINT "purchases_product_id_fkey" FOREIGN KEY (product_id) REFERENCES products (id) NOT DEFERRABLE
) WITH (OIDS = FALSE);
