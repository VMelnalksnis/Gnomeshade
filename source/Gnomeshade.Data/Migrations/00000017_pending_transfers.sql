CREATE TABLE IF NOT EXISTS public.pending_transfers
(
	id                     uuid        DEFAULT uuid_generate_v4() NOT NULL,
	created_at             timestamptz DEFAULT CURRENT_TIMESTAMP  NOT NULL,
	owner_id               uuid                                   NOT NULL,
	created_by_user_id     uuid                                   NOT NULL,
	modified_at            timestamptz DEFAULT CURRENT_TIMESTAMP  NOT NULL,
	modified_by_user_id    uuid                                   NOT NULL,

	transaction_id         uuid                                   NOT NULL,
	source_amount          numeric                                NOT NULL,
	source_account_id      uuid                                   NOT NULL,
	target_counterparty_id uuid                                   NOT NULL,
	transfer_id            uuid                                   NULL,

	CONSTRAINT pending_transfers_id PRIMARY KEY (id),
	CONSTRAINT pending_transfers_created_by_user_id_fkey FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT pending_transfers_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
	CONSTRAINT pending_transfers_modified_by_user_id_fkey FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE,

	CONSTRAINT pending_transfers_transaction_id_fkey FOREIGN KEY (transaction_id) REFERENCES transactions (id) NOT DEFERRABLE,
	CONSTRAINT pending_transfers_source_account_id_fkey FOREIGN KEY (source_account_id) REFERENCES accounts_in_currency (id) NOT DEFERRABLE,
	CONSTRAINT pending_transfers_target_counterparty_id_fkey FOREIGN KEY (target_counterparty_id) REFERENCES counterparties (id) NOT DEFERRABLE,
	CONSTRAINT pending_transfers_transfer_id_fkey FOREIGN KEY (transfer_id) REFERENCES transfers (id) NOT DEFERRABLE
) WITH (OIDS = FALSE);
