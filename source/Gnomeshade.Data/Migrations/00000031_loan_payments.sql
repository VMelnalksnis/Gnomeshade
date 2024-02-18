CREATE TABLE "loans2"
(
	"id"                        uuid        DEFAULT (uuid_generate_v4()) NOT NULL,
	"created_at"                timestamptz DEFAULT CURRENT_TIMESTAMP    NOT NULL,
	"created_by_user_id"        uuid                                     NOT NULL,
	"deleted_at"                timestamptz,
	"deleted_by_user_id"        uuid,
	"owner_id"                  uuid                                     NOT NULL,
	"modified_at"               timestamptz DEFAULT CURRENT_TIMESTAMP    NOT NULL,
	"modified_by_user_id"       uuid                                     NOT NULL,
	"name"                      text                                     NOT NULL,
	"normalized_name"           text                                     NOT NULL,

	"issuing_counterparty_id"   uuid                                     NOT NULL,
	"receiving_counterparty_id" uuid                                     NOT NULL,
	"principal"                 numeric                                  NOT NULL,
	"currency_id"               uuid                                     NOT NULL,

	CONSTRAINT "loans2_id" PRIMARY KEY (id),
	CONSTRAINT "loans2_created_by_user_id_fkey" FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT "loans2_deleted_by_user_id_fkey" FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT "loans2_deleted_check" CHECK ((deleted_at IS NULL AND deleted_by_user_id IS NULL) OR
											 (deleted_at IS NOT NULL AND deleted_by_user_id IS NOT NULL)),
	CONSTRAINT "loans2_owner_id_fkey" FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
	CONSTRAINT "loans2_modified_by_user_id_fkey" FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE,

	CONSTRAINT "loans2_issuing_counterparty_id_fkey" FOREIGN KEY (issuing_counterparty_id) REFERENCES counterparties (id) NOT DEFERRABLE,
	CONSTRAINT "loans2_receiving_counterparty_id_fkey" FOREIGN KEY (receiving_counterparty_id) REFERENCES counterparties (id) NOT DEFERRABLE,
	CONSTRAINT "loans2_currency_id_fkey" FOREIGN KEY (currency_id) REFERENCES currencies (id) NOT DEFERRABLE
);

CREATE TABLE "loan_payments"
(
	"id"                  uuid        DEFAULT (uuid_generate_v4()) NOT NULL,
	"created_at"          timestamptz DEFAULT CURRENT_TIMESTAMP    NOT NULL,
	"created_by_user_id"  uuid                                     NOT NULL,
	"deleted_at"          timestamptz,
	"deleted_by_user_id"  uuid,
	"owner_id"            uuid                                     NOT NULL,
	"modified_at"         timestamptz DEFAULT CURRENT_TIMESTAMP    NOT NULL,
	"modified_by_user_id" uuid                                     NOT NULL,

	"loan_id"             uuid                                     NOT NULL,
	"transaction_id"      uuid                                     NOT NULL,
	"amount"              numeric                                  NOT NULL,
	"interest"            numeric                                  NOT NULL,

	CONSTRAINT "loan_payments_id" PRIMARY KEY (id),
	CONSTRAINT "loan_payments_created_by_user_id_fkey" FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT "loan_payments_deleted_by_user_id_fkey" FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT "loan_payments_deleted_check" CHECK ((deleted_at IS NULL AND deleted_by_user_id IS NULL) OR
													(deleted_at IS NOT NULL AND deleted_by_user_id IS NOT NULL)),
	CONSTRAINT "loan_payments_owner_id_fkey" FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
	CONSTRAINT "loan_payments_modified_by_user_id_fkey" FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE,

	CONSTRAINT "loan_payments_loan_id_fkey" FOREIGN KEY (loan_id) REFERENCES loans2 (id) NOT DEFERRABLE,
	CONSTRAINT "loan_payments_transaction_id_fkey" FOREIGN KEY (transaction_id) REFERENCES transactions (id) NOT DEFERRABLE
);
