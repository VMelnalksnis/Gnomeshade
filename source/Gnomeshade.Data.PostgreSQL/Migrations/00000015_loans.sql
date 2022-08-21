DROP TABLE IF EXISTS "public"."loans";

CREATE TABLE "public"."loans"
(
	"id"                        uuid        DEFAULT uuid_generate_v4() NOT NULL,
	"created_at"                timestamptz DEFAULT CURRENT_TIMESTAMP  NOT NULL,
	"owner_id"                  uuid                                   NOT NULL,
	"created_by_user_id"        uuid                                   NOT NULL,
	"modified_at"               timestamptz DEFAULT CURRENT_TIMESTAMP  NOT NULL,
	"modified_by_user_id"       uuid                                   NOT NULL,

	"transaction_id"            uuid                                   NOT NULL,
	"issuing_counterparty_id"   uuid                                   NOT NULL,
	"receiving_counterparty_id" uuid                                   NOT NULL,
	"amount"                    numeric                                NOT NULL,
	"currency_id"               uuid                                   NOT NULL,

	CONSTRAINT "loans_id" PRIMARY KEY ("id"),
	CONSTRAINT "loans_created_by_user_id_fkey" FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT "loans_owner_id_fkey" FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
	CONSTRAINT "loans_modified_by_user_id_fkey" FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE,

	CONSTRAINT "loans_transaction_id_fkey" FOREIGN KEY (transaction_id) REFERENCES transactions (id) NOT DEFERRABLE,
	CONSTRAINT "loans_issuing_counterparty_id_fkey" FOREIGN KEY (issuing_counterparty_id) REFERENCES counterparties (id) NOT DEFERRABLE,
	CONSTRAINT "loans_receiving_counterparty_id_fkey" FOREIGN KEY (receiving_counterparty_id) REFERENCES counterparties (id) NOT DEFERRABLE,
	CONSTRAINT "loans_currency_id_fkey" FOREIGN KEY (currency_id) REFERENCES currencies (id) NOT DEFERRABLE

) WITH (OIDS = FALSE);
